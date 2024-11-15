using System;
using System.Collections.Generic;
using System.IO;
using LZX.MEditor.LZXStatic;
using LZX.MEditor.MScriptableObject;
using LZX.MEditor.Window.Item;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

namespace LZX.MEditor.Window
{
    public class LeftRootWindow:LZXEditorWindowBase
    {
        public Button btn_1;
        public Button btn_2;
        public Button btn_3;
        public Button btn_4;
        public Button btn_5;
        
        public TextField txtf_bundleSearch;
        public Action<string> OnSeraceChanged;

        #region Action
        /// <summary>
        ///  筛选
        /// </summary>
        public Action<string, ScreeningType> OnScreening;
        public enum ScreeningType
        {
            NONE,
            DATETIME,
            GROUP,
        }
        /// <summary>
        /// 多选
        /// </summary>
        private bool IsMultiSelecting;
        public Action<bool> OnMutiSelecting;
        /// <summary>
        /// 全选
        /// </summary>
        public bool IsSelectAll;
        public Action<bool> OnAllSelection;
        /// <summary>
        /// 反选
        /// </summary>
        public Action OnInvertSelection;
        /// <summary>
        /// 打包所选
        /// </summary>
        public Action OnBuildBundleSelection;
        /// <summary>
        /// 删除所选
        /// </summary>
        public Action OnDeleteSelection;
        /// <summary>
        /// 拖拽资源
        /// </summary>
        public Action<List<string>> OnDragAsset;
        /// <summary>
        /// KEY=asset,VALUE=bundles
        /// </summary>
        public Dictionary<string, List<string>> TempRepeatedAssets = new Dictionary<string, List<string>>();
        #endregion
        public override void CreateGUI()
        {
            base.CreateGUI();
            btn_1 = root.Q<Button>("btn_1");
            btn_1.clicked += OnBtn1Click;
            btn_2 = root.Q<Button>("btn_2");
            btn_2.clicked += OnBtn2Click;
            btn_3 = root.Q<Button>("btn_3");
            btn_3.clicked += OnBtn3Click;
            btn_4 = root.Q<Button>("btn_4");
            btn_4.clicked += OnBtn4Click;
            btn_5 = root.Q<Button>("btn_5");
            btn_5.clicked += OnBtn5Click;
            InitBundle();
            txtf_bundleSearch = root.Q<TextField>("txtf_bundlesearch");
            txtf_bundleSearch.RegisterValueChangedCallback(OnSearchChange);
        }
        public ScrollView scv_bundle;
        public List<BundleItem> bundles = new List<BundleItem>();
        private void InitBundle()
        {
            scv_bundle = root.Q<ScrollView>("scv_bundle");
            var versionController = LZXEditorResources.GetVersionController();
            foreach (var kv in versionController.Bundles)
            { 
                CreateBundleItem(kv.Value);
                OnDragAsset += kv.Value.OnDragAsset;
            }
        }
        private void CreateBundleItem(Bundle bundle)
        {
            BundleItem bundleItem = new BundleItem(bundle, scv_bundle);
            bundles.Add(bundleItem);
            bundleItem.label_idx.text = bundles.Count.ToString();
            OnMutiSelecting += bundleItem.OnMultiSelection;
            OnScreening += bundleItem.OnScreening;
            OnAllSelection += bundleItem.OnAllSelection;
            OnInvertSelection += bundleItem.OnInvertSelection;
            OnDeleteSelection += bundleItem.OnDeleteSelection;
            OnSeraceChanged += bundleItem.OnSearchChanged;
            OnBuildBundleSelection += bundleItem.OnBuildSelection;
            AssetDatabase.SaveAssets();
        }
        #region btn_click

        /// <summary>
        /// 多选和取消多选Button
        /// </summary>
        private void OnBtn1Click()
        {
            if (!IsMultiSelecting)
            {
                btn_1.text = "取消多选";
                btn_2.text = "全选";
                btn_3.text = "反选";
                btn_4.text = "打包所选";
                btn_5.text = "删除所选";
            }
            else
            {
                btn_1.text = "多选";
                btn_2.text = "添加新Bundle";
                btn_3.text = "筛选";
                btn_4.text = "生成清单文件";
                btn_5.text = "打包";
            }
            IsMultiSelecting = !IsMultiSelecting;
            OnMutiSelecting?.Invoke(IsMultiSelecting);
        }
        /// <summary>
        /// 非多选：添加新Bundle；多选：全选
        /// </summary>
        private void OnBtn2Click()
        {
            if (IsMultiSelecting)
            {
                //全选
                IsSelectAll = !IsSelectAll;
                btn_2.text = IsSelectAll ? "取消全选" : "全选";
                OnAllSelection?.Invoke(IsSelectAll);
            }
            else
            {
                //添加新Bundle
                var window = GetWindow<AddGroupWindow>();
                window.minSize = new Vector2(400, 200);
                window.maxSize = new Vector2(400, 200);
                window.OnCreateCompleted += CreateBundleItem;
            }
        }
        /// <summary>
        /// 非多选：筛选；多选：反选
        /// </summary>
        private void OnBtn3Click()
        {
            if (IsMultiSelecting)
            {
                //反选
                OnInvertSelection?.Invoke();
            }
            else
            {
                //筛选
                GetWindow<SelectWindow>().minSize = new Vector2(400, 200);
            }
        }
        /// <summary>
        /// 非多选：生成清单文件；多选：打包所选
        /// </summary>
        private void OnBtn4Click()
        {
            if (IsMultiSelecting)
            {
                //打包所选
                OnBuildBundleSelection?.Invoke();
            }
            else
            {
                //生成清单文件
                //TODO:生成清单文件
            }
        }
        /// <summary>
        /// 非多选：打包；多选：删除所选
        /// </summary>
        private void OnBtn5Click()
        {
            if (IsMultiSelecting)
            {
                //删除所选
                if(EditorUtility.DisplayDialog("警告！", "确定删除所选？", "确定", "取消"))
                    OnDeleteSelection?.Invoke();
            }
            else
            {
                //打包
                var window = GetWindow<BuidlOptionsWindow>();
                window.BuildAll = true;
            }
        }
        private void OnSearchChange(ChangeEvent<string> evt)
        {
            OnSeraceChanged?.Invoke(evt.newValue);
        }

        #endregion
        public void DeleteBundle(BundleItem bundleItem)
        {
            bundles.Remove(bundleItem);
            var name = bundleItem.Bundle.Name;
            File.Delete("Assets/LZX/Bundles/" + name + ".asset");
            File.Delete("Assets/LZX/Bundles/" + name + ".asset.meta");
            var version = LZXEditorResources.GetVersionController();
            version.Remove(bundleItem.Bundle.GUID);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            scv_bundle.Remove(bundleItem.root);
            OnDragAsset -= bundleItem.Bundle.OnDragAsset;
            OnMutiSelecting -= bundleItem.OnMultiSelection;
            OnScreening -= bundleItem.OnScreening;
            OnAllSelection -= bundleItem.OnAllSelection;
            OnInvertSelection -= bundleItem.OnInvertSelection;
            OnDeleteSelection -= bundleItem.OnDeleteSelection;
            OnSeraceChanged -= bundleItem.OnSearchChanged;
            OnBuildBundleSelection -= bundleItem.OnBuildSelection;
        }
    }
}