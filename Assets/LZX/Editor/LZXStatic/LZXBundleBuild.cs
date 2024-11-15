using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LZX.MScriptableObject;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using BuildOptions = LZX.MEditor.MScriptableObject.BuildOptions;
using Bundle = LZX.MEditor.MScriptableObject.Bundle;

namespace LZX.MEditor.LZXStatic
{
    public class LZXBundleBuild
    {
        public class LZXBundleBuildParameter
        {
            public BuildTarget Target;
            public List<AssetBundleBuild> Bundles;

            public BuildAssetBundleOptions BuildOption;
            public string OutputPath;
            public string LoadingUIPath;
            public string LoadingBundleName;
            public bool ClearFloder;
            public bool CopyToStreamingAssets;
            public bool ForceReStart;
            public bool UseVersionControl;
            public LZXBundleBuildParameter(BuildOptions options,BuildTarget target)
            {
                BuildOption = options.compressionType;
                if (options.ForceRebuild)
                    BuildOption |= BuildAssetBundleOptions.ForceRebuildAssetBundle;
                if(options.ExcludeTypeInformation)
                    BuildOption |= BuildAssetBundleOptions.DisableWriteTypeTree;
                if(options.AppendHash)
                    BuildOption |= BuildAssetBundleOptions.AppendHashToAssetBundleName;
                if (options.IgnoreTypeTreeChanges)
                    BuildOption |= BuildAssetBundleOptions.IgnoreTypeTreeChanges;
                if (options.DryRunBuild)
                    BuildOption |= BuildAssetBundleOptions.DryRunBuild;
                if (options.StrictMode)
                    BuildOption |= BuildAssetBundleOptions.StrictMode;
                OutputPath = Path.Combine(options.outputPath, target.ToString());
                Target = target;
                LoadingUIPath = options.LoadingUIPath;
                LoadingBundleName = options.LoadingBundleName;
                ClearFloder = options.ClearFloder;
                CopyToStreamingAssets = options.ClearFloder;
                ForceReStart = options.ForceReStart;
                UseVersionControl = options.UseVersionControl;
            }
        }
        public static void Build(LZXBundleBuildParameter parameter)
        {
            if (parameter.Target == BuildTarget.NoTarget)
                throw new Exception("LZXBundleBuildParameter.BuildTarget is not set.");
            if (parameter.Bundles == null || parameter.Bundles.Count == 0)
                throw new Exception("LZXBundleBuildParameter.Bundles is not set.");
            if (parameter.ClearFloder && Directory.Exists(parameter.OutputPath))
                Directory.Delete(parameter.OutputPath, true);
            Directory.CreateDirectory(parameter.OutputPath);
            BuildPipeline.BuildAssetBundles(
                parameter.OutputPath, 
                parameter.Bundles.ToArray(),
                parameter.BuildOption,
                parameter.Target);
            if (parameter.UseVersionControl)
                VersionControll(parameter);
            if (parameter.CopyToStreamingAssets && parameter.Target == EditorUserBuildSettings.activeBuildTarget)
            {
                Copy2StreamingAssets(parameter);
            }
        }
        private static void Copy2StreamingAssets(LZXBundleBuildParameter parameter)
        {
            if(Directory.Exists(Application.streamingAssetsPath))
                Directory.Delete(Application.streamingAssetsPath, true);
            Directory.CreateDirectory(Application.streamingAssetsPath);
            string sourcePath = parameter.OutputPath;
            string destPath = Application.streamingAssetsPath;
            foreach (string file in Directory.GetFiles(sourcePath))
            {
                if(file.EndsWith(".meta") || file.EndsWith(".manifest") || file.EndsWith("."+parameter.Target))
                    continue;
                string destFile = Path.Combine(destPath, Path.GetFileName(file));
                File.Copy(file, destFile, true);
            }
        }
        private static void VersionControll(LZXBundleBuildParameter parameter)
        {
            var versionController = LZXEditorResources.GetVersionController();
            versionController.Version[4] += 1;
            var versionObject = versionController.GeneratorVersionObject(parameter);
            var bundles = versionController.Bundles.Values.ToList();
            bundles.Add(LZXDllController.GetMetaDataBundle(parameter.Target));
            bundles.AddRange(LZXDllController.GetHotUpdateButNonHotUpdateLogicDlls(parameter.Target));
            versionObject.Bundles = LZXEditorResources.EditorBundle2RunTimeBundle(bundles, parameter.OutputPath);
            string json = JsonUtility.ToJson(versionObject);
            string path = Path.Combine(parameter.OutputPath, "Version.json");
            File.WriteAllText(path, json);
        }
        public static void Build(List<Bundle> bundles,bool buildAll = true)
        {
            var options = LZXEditorResources.GetBuildOptions();
            var builds = GenerateAssetBundleBuilds(options,bundles);
            foreach (var kv in builds)
            {
                options.ForceReStart = LZXDllController.CheckGenerate(kv.Key) || options.ForceReStart;
                var files = Directory.GetFiles(Path.Combine(Application.dataPath,"LZX/AOTDlls",kv.Key.ToString()));
                if(files.Length == 0)
                    throw new Exception("Can not find AOTDlls for " + kv.Key.ToString());
                List<string> assetPaths = new List<string>();
                foreach (var file in files)
                {
                    if(!file.EndsWith(".bytes"))
                        continue;
                    assetPaths.Add(file.Replace(Application.dataPath, "Assets"));
                }
                AssetBundleBuild build = new AssetBundleBuild();
                build.assetBundleName = "MetaDataDlls" + LZXEditorResources.GetSetting().BundleEx;
                build.assetNames = assetPaths.ToArray();
                builds[kv.Key].Add(build);
                BuildTarget target = kv.Key;
                LZXBundleBuildParameter parameter = new LZXBundleBuildParameter(options,target);
                if(!buildAll)
                    parameter.ClearFloder = false;
                parameter.Bundles = kv.Value;
                Build(parameter);
            }
        }
        private static Dictionary<BuildTarget,List<AssetBundleBuild>> GenerateAssetBundleBuilds(BuildOptions options,List<Bundle> bundles)
        {
            var setting = LZXEditorResources.GetSetting();
            Dictionary<BuildTarget,List<AssetBundleBuild>> builds = new Dictionary<BuildTarget, List<AssetBundleBuild>>();
            if (options.FollowBundle)
            {
                foreach (Bundle bundle in bundles)
                {
                    string[] assetPaths = bundle.GetAssetPaths();
                    if (assetPaths.Contains(options.LoadingUIPath))
                        options.LoadingBundleName = bundle.Name;
                    bundle.IsBuild = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
                    AssetBundleBuild build = new AssetBundleBuild();
                    build.assetBundleName = bundle.Name + setting.BundleEx;
                    build.assetNames = assetPaths;
                    foreach (var plat in bundle.Platform)
                    {
                        if (!builds.ContainsKey(plat))
                        {
                            builds.Add(plat, new List<AssetBundleBuild>());
                        }
                        builds[plat].Add(build);
                    }
                }
            }
            else
            {
                foreach (var target in options.buildTargets)
                {
                    builds.Add(target, new List<AssetBundleBuild>());
                    foreach (var bundle in bundles)
                    {
                        string[] assetPaths = bundle.GetAssetPaths();
                        if (assetPaths.Contains(options.LoadingUIPath))
                            options.LoadingBundleName = bundle.Name;
                        bundle.IsBuild = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
                        AssetBundleBuild build = new AssetBundleBuild();
                        build.assetBundleName = bundle.Name + setting.BundleEx;
                        build.assetNames = assetPaths;
                        builds[target].Add(build);
                    }
                }
            }
            return builds;
        }
    }
}