using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LZX.MEditor.Enum;
using LZX.MEditor.MScriptableObject;
using LZX.MScriptableObject;
using UnityEditor;
using UnityEngine;

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
        public static void BuildBundle(List<Bundle> bundles)
        {
            var setting = GetSetting();
            Dictionary<BuildTarget,List<AssetBundleBuild>> builds = new Dictionary<BuildTarget, List<AssetBundleBuild>>();
            foreach (Bundle bundle in bundles)
            {
                string[] assetPaths = bundle.GetAssetPaths();
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
            foreach (var kv in builds)
            {
                string targetPath = Path.Combine(setting.BundleOutPath, kv.Key.ToString());
                if (!Directory.Exists(targetPath))
                {
                    Directory.CreateDirectory(targetPath);
                }
                BuildPipeline.BuildAssetBundles(
                    targetPath, 
                    kv.Value.ToArray(),
                    BuildAssetBundleOptions.UncompressedAssetBundle|BuildAssetBundleOptions.ForceRebuildAssetBundle, 
                    kv.Key);
            }
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