using System.IO;
using LZX.MEditor.MScriptableObject;
using LZX.MEditor.Window;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace LZX.MEditor.Tool
{
    public class LZXInstaller:Editor
    {
        [MenuItem("LZX/Installer")]
        public static void Install()
        {
            string path = "Assets/LZX/Editor/ScriptableObject/";
            string BundleGroupFileName = "BundleGroup.asset";

            if (!File.Exists(path + BundleGroupFileName))
            {
                BundleGroup bundleGroup = ScriptableObject.CreateInstance<BundleGroup>();
                AssetDatabase.CreateAsset(bundleGroup, path + BundleGroupFileName);
            }
            
            string BundleInfoPath = Path.Combine(Application.dataPath, "LZX/Bundles");
            if (!Directory.Exists(BundleInfoPath))
                Directory.CreateDirectory(BundleInfoPath);    
            
            string VersionControllerPath = Path.Combine(path,"VersionController.asset");
            if (!File.Exists(VersionControllerPath))
            {
                VersionController versionController = ScriptableObject.CreateInstance<VersionController>();
                AssetDatabase.CreateAsset(versionController, VersionControllerPath);
            }
            
            var settingwindow = EditorWindow.GetWindow<SettingWindow>();
            settingwindow.minSize = new Vector2(480, 180);
            settingwindow.loadingName.style.display = DisplayStyle.None;
            settingwindow.loadingPath.style.display = DisplayStyle.None;
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}