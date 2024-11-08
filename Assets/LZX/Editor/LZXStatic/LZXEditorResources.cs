using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using LZX.MEditor.Enum;
using LZX.MEditor.MScriptableObject;
using LZX.MScriptableObject;
using UnityEditor;
using UnityEngine;
using Asset = LZX.MScriptableObject.Asset;
using BuildOptions = LZX.MEditor.MScriptableObject.BuildOptions;
using Bundle = LZX.MScriptableObject.Bundle;

namespace LZX.MEditor.LZXStatic
{
    public class LZXEditorResources
    {
        public static List<string> GetDependencies(string assetpath)
        {
            List<string> dependence;
            //根据输入文件返回所有依赖资源路径，bool值表示是否递归查找依赖项的依赖项
            string[] files = AssetDatabase.GetDependencies(GetStanderPath(assetpath), true);
            dependence = files.Where(
                file => !file.EndsWith(".cs")
                          && !file.Equals(GetStanderPath(assetpath)))
                .Select(
                    file => file = Path.GetFileNameWithoutExtension(file))
                .ToList();
            return dependence;
        }
        public static string GetStanderPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return string.Empty;
            return path.Trim().Replace("\\", "/");
        }
        #region GetScriptableObject
        public static Texture2D GetIcon(LZXIconType iconType)
        {
            switch (iconType)
            {
                case LZXIconType.cs:
                    return AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/LZX/Editor/Sprite/cs.png");;
                case LZXIconType.delete:
                    return AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/LZX/Editor/Sprite/delete.png");
                case LZXIconType.music:
                    return AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/LZX/Editor/Sprite/music.png");
                case LZXIconType.prefab:
                    return AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/LZX/Editor/Sprite/prefab.png");
                case LZXIconType.scene:
                    return AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/LZX/Editor/Sprite/scene.png");
                case LZXIconType.setting:
                    return AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/LZX/Editor/Sprite/setting.png");
                case LZXIconType.trans:
                    return AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/LZX/Editor/Sprite/trans.png");
                case LZXIconType.build:
                    return AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/LZX/Editor/Sprite/build.png");
                default:
                    return null;
            }
        }
        public static VersionController GetVersionController()
        {
            if (!File.Exists(
                    "Assets/LZX/Editor/ScriptableObject/VersionController.asset".Replace("Assets",
                        Application.dataPath)))
                throw new Exception("请先Installer");
            return AssetDatabase.LoadAssetAtPath<VersionController>("Assets/LZX/Editor/ScriptableObject/VersionController.asset");
        }
        public static BundleGroup GetBundleGroup()
        {
            if (!File.Exists(
                    "Assets/LZX/Editor/ScriptableObject/BundleGroup.asset".Replace("Assets",
                        Application.dataPath)))
                throw new Exception("请先Installer");
            return AssetDatabase.LoadAssetAtPath<BundleGroup>("Assets/LZX/Editor/ScriptableObject/BundleGroup.asset");
        }
        public static Setting GetSetting()
        {
            if (!File.Exists(
                    "Assets/LZX/ScriptableObject/Setting.asset".Replace("Assets",
                        Application.dataPath)))
                return null;
            return AssetDatabase.LoadAssetAtPath<Setting>("Assets/LZX/ScriptableObject/Setting.asset");
        }
        public static BuildOptions GetBuildOptions()
        {
            if (!File.Exists(
                    "Assets/LZX/Editor/ScriptableObject/BuildOptions.asset".Replace("Assets",
                        Application.dataPath)))
                return null;
            return AssetDatabase.LoadAssetAtPath<BuildOptions>("Assets/LZX/Editor/ScriptableObject/BuildOptions.asset");
        }

        #endregion
        public static void BuildBundle(List<MScriptableObject.Bundle> bundles,bool BuidlAll)
        {
            var setting = GetSetting();
            var buildOptions = GetBuildOptions();
            Dictionary<BuildTarget,List<AssetBundleBuild>> builds = new Dictionary<BuildTarget, List<AssetBundleBuild>>();
            if (buildOptions.FollowBundle)
            {
                foreach (MScriptableObject.Bundle bundle in bundles)
                {
                    string[] assetPaths = bundle.GetAssetPaths();
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
                foreach (var target in buildOptions.buildTargets)
                {
                    builds.Add(target, new List<AssetBundleBuild>());
                    foreach (var bundle in bundles)
                    {
                        string[] assetPaths = bundle.GetAssetPaths();
                        bundle.IsBuild = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
                        AssetBundleBuild build = new AssetBundleBuild();
                        build.assetBundleName = bundle.Name + setting.BundleEx;
                        build.assetNames = assetPaths;
                        builds[target].Add(build);
                    }
                }
            }
            Build(builds,BuidlAll);
            CopyToStreamingAssets();
        }
        private static void Build(Dictionary<BuildTarget, List<AssetBundleBuild>> builds,bool BuidlAll)
        {
            var versionController = GetVersionController();
            versionController.Version[4] += 1;
            EditorUtility.SetDirty(versionController);
            AssetDatabase.SaveAssets();
            var buildOptions = GetBuildOptions();
            #region 参数设置
            BuildAssetBundleOptions assetBundleOptions = buildOptions.compressionType;
            if (buildOptions.ForceRebuild)
                assetBundleOptions |= BuildAssetBundleOptions.ForceRebuildAssetBundle;
            if(buildOptions.ExcludeTypeInformation)
                assetBundleOptions |= BuildAssetBundleOptions.DisableWriteTypeTree;
            if(buildOptions.AppendHash)
                assetBundleOptions |= BuildAssetBundleOptions.AppendHashToAssetBundleName;
            if (buildOptions.IgnoreTypeTreeChanges)
                assetBundleOptions |= BuildAssetBundleOptions.IgnoreTypeTreeChanges;
            if (buildOptions.DryRunBuild)
                assetBundleOptions |= BuildAssetBundleOptions.DryRunBuild;
            if (buildOptions.StrictMode)
                assetBundleOptions |= BuildAssetBundleOptions.StrictMode;
            #endregion
            foreach (var kv in builds)
            {
                string targetPath = Path.Combine(buildOptions.outputPath, kv.Key.ToString())
                    .Replace(Application.dataPath, "Assets");
                if(BuidlAll && buildOptions.ClearFloder && Directory.Exists(targetPath))
                    Directory.Delete(targetPath, true);
                if (!Directory.Exists(targetPath))
                    Directory.CreateDirectory(targetPath);
                BuildPipeline.BuildAssetBundles(
                    targetPath, 
                    kv.Value.ToArray(),
                    assetBundleOptions, 
                    kv.Key);
                GenerateVersionList(targetPath);
            }
        }
        private static void CopyToStreamingAssets()
        {
            BuildOptions buildOptions = GetBuildOptions();
            if (buildOptions.CopyTOStreamingAssets)
            {
                string copyPath = Path.Combine(buildOptions.outputPath,EditorUserBuildSettings.activeBuildTarget.ToString());
                if (!Directory.Exists(copyPath))
                    throw new Exception("当前构建平台未生成ab包");
                if(Directory.Exists(Application.streamingAssetsPath))
                    Directory.Delete(Application.streamingAssetsPath, true);
                Directory.CreateDirectory(Application.streamingAssetsPath);
                var files = Directory.GetFiles(copyPath, "*.*", SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    if(file.EndsWith(".meta") || file.EndsWith("manifest") || file.EndsWith(EditorUserBuildSettings.activeBuildTarget.ToString()))
                        continue;
                    string dest = Path.Combine(Application.streamingAssetsPath, Path.GetFileName(file));
                    File.Copy(file, dest, true);
                }
            }
        }
        private static void GenerateVersionList(string targetPath)
        {
            BuildOptions buildOptions = GetBuildOptions();
            VersionController versionController = GetVersionController();
            Setting setting = GetSetting();
            if (buildOptions.UseVersionControl)
            {
                var versionObj = new VersionObject();
                List<LZX.MScriptableObject.Bundle> bundleObjs = new List<LZX.MScriptableObject.Bundle>();
                foreach (var kvBundle in versionController.Bundles)
                {
                    if (kvBundle.Value.IsBuild == "FALSE" || 
                        !File.Exists(Path.Combine(targetPath,kvBundle.Value.Name + setting.BundleEx)
                                        .Replace("Assets", Application.dataPath)))
                    {
                        Debug.LogWarning($"AssetBundle：[{kvBundle.Value.Name}]还未构建，将不加入版本文件列表");
                        continue;
                    }
                    var Versionbundle = new Bundle();
                    Versionbundle.Name = kvBundle.Value.Name;
                    var assets = kvBundle.Value.GetAssetPaths();
                    List<LZX.MScriptableObject.Asset> assetObjs = new List<LZX.MScriptableObject.Asset>();
                    foreach (var asset in assets)
                    {
                        LZX.MScriptableObject.Asset assetObj = new Asset();
                        assetObj.LoadPath = asset;
                        assetObj.MD5 = GetMD5(asset.Replace("Assets", Application.dataPath));
                        assetObj.Dependencies = GetDependencies(asset).ToArray();
                        assetObjs.Add(assetObj);
                    }
                    Versionbundle.MD5 = GetMD5(
                        Path.Combine(targetPath,kvBundle.Value.Name + setting.BundleEx)
                            .Replace("Assets", Application.dataPath));
                    Versionbundle.Assets = assetObjs.ToArray();
                    bundleObjs.Add(Versionbundle);
                }
                versionObj.Bundles = bundleObjs.ToArray();
                versionObj.version = string.Join(".", versionController.Version);
                string json = JsonUtility.ToJson(versionObj);
                File.WriteAllText(Path.Combine(targetPath, "version.json"), json);
            }
        }
        private static string GetMD5(string path)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] hash = md5.ComputeHash(File.ReadAllBytes(path));
                return BitConverter.ToString(hash).Replace("-", "").ToLower();;
            }
        }
        public static MScriptableObject.Bundle GetBundleWithBundleName(string bundleName)
        {
            return !File.Exists(Path.Combine(Application.dataPath, "LZX/Bundles/" + bundleName + ".asset"))
                ? null : AssetDatabase.LoadAssetAtPath<MScriptableObject.Bundle>("Assets/LZX/Bundles/" + bundleName + ".asset");
        }
    }
    public enum LZXIconType
    {
        cs,
        delete,
        music,
        prefab,
        scene,
        setting,
        trans,
        build,
    }
}