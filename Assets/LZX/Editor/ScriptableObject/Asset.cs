using System.IO;
using System.Linq;
using LZX.MEditor.LZXStatic;
using UnityEditor;
using UnityEngine;
using Application = UnityEngine.Device.Application;

namespace LZX.MEditor.MScriptableObject
{
    public class Asset
    {
        public string Name { get; }
        public string GUID { get; }
        public string LoadPath{ get; }
        public string[] Dependences;
        public string Size{ get; }
        public Asset(string GUID)
        {
            this.GUID = GUID;
            LoadPath = AssetDatabase.GUIDToAssetPath(GUID);
            if (!string.IsNullOrEmpty(LoadPath))
            {
                Size = EditorUtility.FormatBytes(File.ReadAllBytes(LoadPath.Replace("Assets",Application.dataPath)).LongLength);
            }
            else
            {
                Debug.LogError("Asset not found--GUID: " + GUID);
            }
            Name = Path.GetFileNameWithoutExtension(LoadPath);
            Dependences = LZXEditorResources.GetDependencies(LoadPath).ToArray();
        }
    }
}