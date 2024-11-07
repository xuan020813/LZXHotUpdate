using System.Collections.Generic;
using System.IO;
using LZX.MEditor.Enum;
using UnityEditor;
using UnityEngine;
namespace LZX.MEditor.MScriptableObject
{
    public class Bundle:ScriptableObject
    {
        public string Name;
        public int AssetCount;
        public string GUID;
        public string IsBuild;
        public List<BuildTarget> Platform = new List<BuildTarget>();
        public string CreateDate;
        public string[] Dependences;
        public List<string> Group = new List<string>();

        public string[] GetAssetPaths()
        {
            List<string> paths = new List<string>();
            var assets = Directory.GetFiles(Path.Combine(Application.dataPath, "LZX/Bundles", Name));
            foreach (var asset in assets)
            {
                paths.Add(asset.Replace(Application.dataPath, "Assets"));
            }
            return paths.ToArray();
        }
    }
}