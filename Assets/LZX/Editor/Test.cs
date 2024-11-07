using LZX.MEditor.MScriptableObject;
using UnityEditor;
using UnityEngine;

namespace LZX.MEditor
{
    public class Test : Editor
    {
        [MenuItem("LZX/Test")]
        static void TestMenu()
        {
            VersionController version = ScriptableObject.CreateInstance<VersionController>();
            var bundle = ScriptableObject.CreateInstance<Bundle>();
            bundle.Name = "111";
            version.Add("111", bundle);
            AssetDatabase.CreateAsset(version, "Assets/LZX/Editor/ScriptableObject/Test.asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
        }

        [MenuItem("LZX/Test2")]
        static void TestMenu2()
        {
            var version = AssetDatabase.LoadAssetAtPath<VersionController>("Assets/LZX/Editor/ScriptableObject/Test.asset");
            foreach (var kv in version.Bundles)
            {
                Debug.Log(kv.Key+"--"+kv.Value.Name);
            }
        }
        
    }
}