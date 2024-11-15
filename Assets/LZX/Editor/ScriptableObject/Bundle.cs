using System;
using System.Collections.Generic;
using System.IO;
using LZX.MEditor.Enum;
using LZX.MEditor.Window;
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
        public void OnDragAsset(List<string> assetNames)
        {
            foreach (var asset in assetNames)
            {
                string guid = AssetDatabase.AssetPathToGUID(asset);
                if (AssetGUIDs.Contains(guid))
                {
                    var window = EditorWindow.GetWindow<LeftRootWindow>();
                    if(!window.TempRepeatedAssets.ContainsKey(asset))
                        window.TempRepeatedAssets.Add(asset, new List<string>());
                    window.TempRepeatedAssets[asset].Add(Name);
                }
            }
        }
    }
}