using System;
using System.Collections.Generic;
using System.Linq;
using LZX.MEditor.LZXStatic;
using LZX.MEditor.MScriptableObject;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace LZX.MEditor.Window.Item
{
    public class BundleItem: ItemBase
    {
        public Label Name;
        public Bundle Bundle;
        public Button btn_build;
        public Button btn_setting;
        public Button btn_delete;
        public Label label_idx;
        public Toggle toggle_select;
        public bool IsSelected;
        public BundleItem(Bundle bundle, VisualElement scv)
        {
            Name = root.Q<Label>("label_name");
            btn_build = root.Q<Button>("btn_1");
            btn_setting = root.Q<Button>("btn_2");
            btn_delete = root.Q<Button>("btn_3");
            InitButton();
            label_idx = root.Q<Label>("label_idx");
            toggle_select = root.Q<Toggle>("toggle_select");
            var label_name = root.Q<Label>("label_name");
            Bundle = bundle;
            Name.text = bundle.Name;
            label_name.RegisterCallback<ClickEvent>(OnClick);
            scv.Add(root);
        }
        private void InitButton()
        {
            btn_build = root.Q<Button>("btn_1");
            btn_build.clicked += OnBuildClick;
            btn_build.style.backgroundImage = new StyleBackground(LZXEditorResources.GetIcon(LZXIconType.build));
            btn_setting = root.Q<Button>("btn_2");
            btn_setting.clicked += OnSettingClick;
            btn_setting.style.backgroundImage = new StyleBackground(LZXEditorResources.GetIcon(LZXIconType.setting));
            btn_delete = root.Q<Button>("btn_3");
            btn_delete.clicked += OnDeleteClick;
            btn_delete.style.backgroundImage = new StyleBackground(LZXEditorResources.GetIcon(LZXIconType.delete));
        }
        private void OnBuildClick()
        {
            List<Bundle> bundles = new List<Bundle>();
            bundles.Add(Bundle);
            BuidlOptionsWindow window = EditorWindow.GetWindow<BuidlOptionsWindow>();
            window.minSize = new Vector2(600, 370);
            window.bundles = bundles;
            window.togl_clearfloder.style.display = DisplayStyle.None;
        }
        private void OnSettingClick()
        {
            var window = EditorWindow.GetWindow<AddGroupWindow>();
            window.minSize = new Vector2(400, 200);
            window.maxSize = new Vector2(400, 200);
            window.Name.value = Bundle.Name;
            window.Win.value = Bundle.Platform.Contains(BuildTarget.StandaloneWindows64);
            window.IOS.value = Bundle.Platform.Contains(BuildTarget.iOS);
            window.Android.value = Bundle.Platform.Contains(BuildTarget.Android);
            foreach (var element in window.Group.Children())
            {
                if (element is ScrollView)
                {
                    foreach (var toggle in element.Children())
                    {
                        if (toggle is Toggle t)
                        {
                            t.value = Bundle.Group.Contains(t.text);
                        }
                    }
                }
            }
        }
        private void OnDeleteClick()
        {
            MessageBoxWindow.Show("确定要删除嘛？", () =>
            {
                var window = EditorWindow.GetWindow<LeftRootWindow>();
                window.DeleteBundle(this);
            });
        }
        public void OnMultiSelection(bool isMultiSelect)
        {
            label_idx.style.display = isMultiSelect ? DisplayStyle.None : DisplayStyle.Flex;
            toggle_select.style.display = isMultiSelect ? DisplayStyle.Flex : DisplayStyle.None;
            if(!isMultiSelect)
            {
                IsSelected = false;
                toggle_select.value = IsSelected;
            }
        }
        public void OnScreening(string context, LeftRootWindow.ScreeningType type)
        {
            switch (type)
            {
                case LeftRootWindow.ScreeningType.NONE:
                    root.style.display = DisplayStyle.Flex;
                    break;
                case LeftRootWindow.ScreeningType.GROUP:
                    var groupname = context.Split("_");
                    root.style.display = DisplayStyle.None;
                    foreach (var group in groupname)
                    {
                        if(Bundle.Group.Contains(group))
                            root.style.display = DisplayStyle.Flex;
                        break;
                    }
                    break;
                case LeftRootWindow.ScreeningType.DATETIME:
                    var date = context.Split("+");
                    root.style.display = DisplayStyle.None;
                    if (date.Length == 2)
                    {
                        DateTime before = DateTime.Parse(date[0]);
                        DateTime after = DateTime.Parse(date[1]);
                        DateTime createDate = DateTime.Parse(Bundle.CreateDate);
                        if (createDate >= before && createDate <= after)
                        {
                            root.style.display = DisplayStyle.Flex;
                        }
                    }
                    break;
            }
        }
        public void OnAllSelection(bool IsAllSelect)
        {
            IsSelected = IsAllSelect;
            toggle_select.value = IsAllSelect;
        }
        public void OnInvertSelection()
        {
            IsSelected = !IsSelected;
            toggle_select.value = IsSelected;
        }
        public void OnDeleteSelection()
        {
            if (IsSelected)
            {
                var window = EditorWindow.GetWindow<LeftRootWindow>();
                window.DeleteBundle(this);
            }
        }
        private void OnClick(ClickEvent evt)
        {
            RightRootWindow window = EditorWindow.GetWindow<RightRootWindow>();
            window.InitAsset(Bundle);
            InfoWindow info = EditorWindow.GetWindow<InfoWindow>();
            info.Init(Bundle);
        }
        public void OnSearchChanged(string context)
        {
            root.style.display = Name.text.ContainsInvariantCultureIgnoreCase(context) ? DisplayStyle.Flex : DisplayStyle.None;
        }
        public void OnBuildSelection()
        {
            if (IsSelected)
            {
                var window = EditorWindow.GetWindow<BuidlOptionsWindow>();
                if(window.bundles == null)
                    window.bundles = new List<Bundle>();
                window.bundles.Add(Bundle);
            }
        }
    }
}