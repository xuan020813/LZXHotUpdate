using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using HybridCLR.Editor;
using HybridCLR.Editor.Commands;
using HybridCLR.Editor.Settings;
using LZX.MEditor.Enum;
using LZX.MEditor.MScriptableObject;
using LZX.MScriptableObject;
using UnityEditor;
using UnityEditor.Build;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;
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
            return AssetDatabase.LoadAssetAtPath<Texture2D>($"Assets/LZX/Editor/Sprite/{iconType}.png");;
        }
        public static VersionController GetVersionController()
        {
            if (!File.Exists(LZXResourcesUtil.GetVersionControllerPath().Replace("Assets", Application.dataPath)))
                throw new Exception("未找到VersionController，请先Installer");
            return AssetDatabase.LoadAssetAtPath<VersionController>(LZXResourcesUtil.GetVersionControllerPath());
        }
        public static BundleGroup GetBundleGroup()
        {
            if (!File.Exists(LZXResourcesUtil.GetBundleGroupPath().Replace("Assets", Application.dataPath)))
                throw new Exception("未找到BundleGroup，请先Installer");
            return AssetDatabase.LoadAssetAtPath<BundleGroup>(LZXResourcesUtil.GetBundleGroupPath());
        }
        public static Setting GetSetting()
        {
            if (!File.Exists(LZXResourcesUtil.GetSettingsPath().Replace("Assets", Application.dataPath)))
                throw new Exception("未找到Setting，请先Installer");
            return AssetDatabase.LoadAssetAtPath<Setting>(LZXResourcesUtil.GetSettingsPath());
        }
        public static BuildOptions GetBuildOptions()
        {
            if (!File.Exists(LZXResourcesUtil.GetBuildOptionsPath().Replace("Assets", Application.dataPath)))
                throw new Exception("未找到BuildOptions，请先Installer");
            return AssetDatabase.LoadAssetAtPath<BuildOptions>(LZXResourcesUtil.GetBuildOptionsPath());
        }
        public static AssemblyDefinitionAsset GetHotUpdateAssemblyDefinition()
        {
            return AssetDatabase.LoadAssetAtPath<AssemblyDefinitionAsset>(LZXResourcesUtil.GetHotUpdateAssembleDefinePath());
        }
        #endregion
        public static string GetMD5(string path)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] hash = md5.ComputeHash(File.ReadAllBytes(path));
                return BitConverter.ToString(hash).Replace("-", "").ToLower();;
            }
        }
        public static VisualTreeAsset GetItemUXML()
        {
            return AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(LZXResourcesUtil.GetItemUxmlPath());
        }
        public static StyleSheet GetItemUSS()
        {
            return AssetDatabase.LoadAssetAtPath<StyleSheet>(LZXResourcesUtil.GetItemUssPath());
        }
        public static Bundle[] EditorBundle2RunTimeBundle(List<MScriptableObject.Bundle> bundles,string bundleOutPath)
        {
            List<Bundle> runtimeBundles = new List<Bundle>();
            foreach (var bundle in bundles)
            {
                string editorBundlePath = "";
                if(!bundle.Name.EndsWith(".bytes"))
                    editorBundlePath = Path.Combine(bundleOutPath, bundle.Name + GetSetting().BundleEx);
                else
                    editorBundlePath = Path.Combine(bundleOutPath, bundle.Name);
                if (!File.Exists(editorBundlePath))
                {
                    Debug.LogError($"Bundle:{bundle.Name} 尚未构建，已跳过加入资产清单");
                    continue;
                }
                Bundle runtimeBundle = new Bundle();
                runtimeBundle.Name = bundle.Name;
                runtimeBundle.MD5 = GetMD5(editorBundlePath);
                List<Asset> runtimeAssets = new List<Asset>();
                foreach (var aspath in bundle.GetAssetPaths())
                {
                    Asset asset = new Asset();
                    asset.LoadPath = aspath;
                    asset.MD5 = GetMD5(aspath.Replace("Assets", Application.dataPath));
                    asset.Dependencies = GetDependencies(aspath).ToArray();
                    runtimeAssets.Add(asset);
                }
                runtimeBundle.Assets = runtimeAssets.ToArray();
                runtimeBundles.Add(runtimeBundle);
            }
            return runtimeBundles.ToArray();
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