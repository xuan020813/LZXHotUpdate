using System;
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
        public string GUID;
        public string IsBuild;
        public List<BuildTarget> Platform = new List<BuildTarget>();
        public string CreateDate;
        public string[] Dependences;
        public List<string> Group = new List<string>();
        public List<string> AssetGUIDs = new List<string>();
        public string[] GetAssetPaths()
        {
            List<string> paths = new List<string>();
            foreach (var GUID in AssetGUIDs)
            {
                var path = AssetDatabase.GUIDToAssetPath(GUID);
                if (string.IsNullOrEmpty(path) || !File.Exists(path.Replace("Assets", Application.dataPath)))
                    throw new Exception("Asset not found -- GUID: " + GUID);
                paths.Add(path);
            }
            return paths.ToArray();
        }
    }
}