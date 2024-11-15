using System.Collections.Generic;
using System.IO;
using HybridCLR.Editor.Settings;
using LZX.MEditor.LZXStatic;
using LZX.MEditor.MScriptableObject;
using LZX.MEditor.Window;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;

namespace LZX.MEditor.Tool
{
    public class LZXInstaller:Editor
    {
        [MenuItem("LZX/Installer")]
        public static void Install()
        {
            if (!File.Exists(LZXResourcesUtil.GetBundleGroupPath().Replace("Assets", Application.dataPath)))
            {
                BundleGroup bundleGroup = ScriptableObject.CreateInstance<BundleGroup>();
                AssetDatabase.CreateAsset(bundleGroup, LZXResourcesUtil.GetBundleGroupPath());
            }
            
            if (!Directory.Exists(LZXResourcesUtil.GetBundlesPath().Replace("Assets", Application.dataPath)))
                Directory.CreateDirectory(LZXResourcesUtil.GetBundlesPath().Replace("Assets", Application.dataPath));    
            
            if (!File.Exists(LZXResourcesUtil.GetVersionControllerPath().Replace("Assets", Application.dataPath)))
            {
                VersionController versionController = ScriptableObject.CreateInstance<VersionController>();
                AssetDatabase.CreateAsset(versionController, LZXResourcesUtil.GetVersionControllerPath());
            }
            
            if (!File.Exists(LZXResourcesUtil.GetBuildOptionsPath().Replace("Assets", Application.dataPath)))
            {
                MScriptableObject.BuildOptions buildOptions = ScriptableObject.CreateInstance<MScriptableObject.BuildOptions>();
                buildOptions.Compression = 0;
                AssetDatabase.CreateAsset(buildOptions, LZXResourcesUtil.GetBuildOptionsPath());
            }
            
            if (!File.Exists(LZXResourcesUtil.GetSettingsPath().Replace("Assets", Application.dataPath)))
            {
                MScriptableObject.Setting setting = ScriptableObject.CreateInstance<MScriptableObject.Setting>();
                setting.asmdefs = new List<AssemblyDefinitionAsset>();
                setting.asmdefs.Add(LZXEditorResources.GetHotUpdateAssemblyDefinition());
                AssetDatabase.CreateAsset(setting, LZXResourcesUtil.GetSettingsPath());
            }
            
            var settingwindow = EditorWindow.GetWindow<SettingWindow>();
            settingwindow.minSize = new Vector2(480, 180);
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}