using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LZX.MEditor.MScriptableObject;
using LZX.MEditor.Window;
using LZX.MEditor.Window.Item;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace LZX.MEditor.Window
{
    public class RightRootWindow:LZXEditorWindowBase
    {
        public Button btn_1;
        public ScrollView scv_asset;
        public TextField txtf_search;
        public Action<string> OnSearchChanged;
        public override void CreateGUI()
        {
            base.CreateGUI();
            btn_1 = root.Q<Button>("btn_1");
            btn_1.clicked += DeleteAllAsset;
            scv_asset = root.Q<ScrollView>("scv_asset");
            scv_asset.RegisterCallback<DragEnterEvent>(OnDragEnter);
            scv_asset.RegisterCallback<DragUpdatedEvent>(OnDragUpdated);
            scv_asset.RegisterCallback<DragPerformEvent>(OnDragPerform);
            txtf_search = root.Q<TextField>("txtf_assetsearch");
            txtf_search.RegisterCallback<ChangeEvent<string>>(OnSearchChange);
        }
        public Bundle curSelectBundle;
        public void InitAsset(Bundle bundle)
        {
            scv_asset.Clear();
            curSelectBundle = bundle;
            foreach (var GUID in bundle.AssetGUIDs)
            {
                var asset = new Asset(GUID);
                AssetItem assetItem = new AssetItem(asset,scv_asset);
                OnSearchChanged += assetItem.OnSearchChanged;
            }
        }
        #region 拖拽
        private void OnDragEnter(DragEnterEvent evt)
        {
            if (DragAndDrop.objectReferences.Length > 0)
            {
                // 设置拖拽操作的效果
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            }
            else
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
            }
        }
        private void OnDragUpdated(DragUpdatedEvent evt)
        {
            if (DragAndDrop.objectReferences.Length > 0)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            }
            else
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
            }
        }
        private void OnDragPerform(DragPerformEvent evt)
        {
            if (curSelectBundle == null)
                throw new Exception("未选择Bundle,您往哪拖？");
            foreach (var obj in DragAndDrop.objectReferences)
            {
                string assetPath = AssetDatabase.GetAssetPath(obj);
                if (assetPath.EndsWith(".meta") || assetPath.EndsWith(".asset"))
                    continue;
                string absolutePath = assetPath.Replace("Assets", Application.dataPath);
                List<string> assets = new List<string>();
                if (IsDir(absolutePath))
                {
                    assets.AddRange(Directory.GetFiles(absolutePath, "*", SearchOption.AllDirectories)
                        .Where(v => !v.EndsWith(".meta") && !v.EndsWith(".asset"))
                        .Select(v => v.Replace(Application.dataPath, "Assets"))
                        .ToList());
                }
                else
                {
                    assets.Add(assetPath);
                }
                var window = GetWindow<LeftRootWindow>();
                window.OnDragAsset?.Invoke(assets);
                string warning = "";
                if (window.TempRepeatedAssets.Count > 0)
                {
                    foreach (var kv in window.TempRepeatedAssets)
                    {
                        warning += $"资产：{kv.Key} 已经存在于Bundle：{string.Join(",", kv.Value)}中\n";
                    }

                    MessageBoxWindow.Show(warning, () => AddAsset(assets));
                }
                else
                {
                    AddAsset(assets);
                }
            }
        }
        private bool IsDir(string absolutePath)
        {
            if (Directory.Exists(absolutePath))
                return true;
            return false;
        }
        #endregion
        private void AddAsset(List<string> assets)
        {
            foreach (var path in assets)
            {
                string guid = AssetDatabase.AssetPathToGUID(path);
                if (!curSelectBundle.AssetGUIDs.Contains(guid))
                    curSelectBundle.AssetGUIDs.Add(guid);
                Asset asset = new Asset(guid);
                AssetItem assetItem = new AssetItem(asset, scv_asset);
                OnSearchChanged += assetItem.OnSearchChanged;
            }
            EditorUtility.SetDirty(curSelectBundle);
            AssetDatabase.SaveAssets();
        }
        public void DeleteAsset(AssetItem assetItem)
        {
            if(!curSelectBundle.AssetGUIDs.Contains(assetItem.Asset.GUID))
                return;
            if (EditorUtility.DisplayDialog("删除资产", $"确定要删除资产：{assetItem.Asset.Name}吗？", "确定", "取消"))
            {
                curSelectBundle.AssetGUIDs.Remove(assetItem.Asset.GUID);
                scv_asset.Remove(assetItem.root);
                OnSearchChanged -= assetItem.OnSearchChanged;
            }
        }
        private void DeleteAllAsset()
        {
            if (curSelectBundle == null)
                return;
            if (EditorUtility.DisplayDialog("删除资产", $"确定要删除Bundle：{curSelectBundle.Name}中的所有资产吗？", "确定", "取消"))
            {
                curSelectBundle.AssetGUIDs.Clear();
                scv_asset.Clear();
                OnSearchChanged = null;
            }
        }
        private void OnSearchChange(ChangeEvent<string> evt)
        {
            OnSearchChanged?.Invoke(evt.newValue);
        }
    }
}