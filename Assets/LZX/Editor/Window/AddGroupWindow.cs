using System;
using System.Collections.Generic;
using System.IO;
using LZX.MEditor.Enum;
using LZX.MEditor.LZXStatic;
using LZX.MEditor.MScriptableObject;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace LZX.MEditor.Window
{
    public class AddGroupWindow : EditorWindow
    {
        public VisualTreeAsset window;
        public StyleSheet uss;
        public TextField Name;
        public Toggle Win;
        public Toggle IOS;
        public Toggle Android;
        public VisualElement Group;
        public BundleGroup group;
        public VisualElement root;

        public Action<Bundle> OnCreateCompleted;
        public bool IsCreate = false;
        private void CreateGUI()
        {
            root = window.CloneTree();
            this.rootVisualElement.Add(root);
            root.styleSheets.Add(uss);
            Name = root.Q<TextField>("txf_name");
            Win = root.Q<Toggle>("tgl_win");
            IOS = root.Q<Toggle>("tgl_ios");
            Android = root.Q<Toggle>("tgl_android");
            Group = root.Q<VisualElement>("dpdn_group");
            AddGroupChoices();
            var btn_addgroup = root.Q<Button>("btn_addgroup");
            btn_addgroup.clicked += () => { CreateGroup(); };
            #region Button
            Button btn = new Button() { text = "确定" };
            btn.clicked += () =>
            {
                OnYseButtonClick();
            };
            btn.style.position = Position.Absolute;
            btn.style.bottom = 0; // 定位到底部
            btn.style.left = 0; // 定位到左边
            var buttonStyle = new StyleLength(80); // 按钮的宽度
            var buttonHeight = new StyleLength(30); // 按钮的高度
            btn.style.width = buttonStyle;
            btn.style.height = buttonHeight;
            btn.style.marginBottom = 0;
            this.rootVisualElement.Add(btn);
            #endregion
        }
        private void OnYseButtonClick()
        {
            if (!Directory.Exists(Path.Combine(Application.dataPath, "LZX/Bundles")))
                throw new Exception("Installer LZX FIRST!!!");
            Bundle bundle = ScriptableObject.CreateInstance<Bundle>();
            bundle.Name = Name.value;
            if (Win.value)
                bundle.Platform.Add(BuildTarget.StandaloneWindows64);
            if (IOS.value)
                bundle.Platform.Add(BuildTarget.iOS);
            if (Android.value)
                bundle.Platform.Add(BuildTarget.Android);
            bundle.CreateDate = DateTime.Now.ToString();
            bundle.IsBuild = "FALSE";
            foreach (var item in Group.Children())
            {
                if (item is ScrollView)
                {
                    foreach (var toggle in item.Children())
                    {
                        if (toggle is Toggle { value: true } t)
                            bundle.Group.Add(t.text);
                    }
                }

            }
            AssetDatabase.CreateAsset(bundle, "Assets/LZX/Bundles/" + bundle.Name + ".asset");
            bundle.GUID = AssetDatabase.AssetPathToGUID("Assets/LZX/Bundles/" + bundle.Name + ".asset");
            var version = LZXEditorResources.GetVersionController();
            version.Add(bundle.GUID,bundle);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            OnCreateCompleted?.Invoke(bundle);
            Close();
        }
        private void CreateGroup()
        {
            if (group == null)
                group = LZXEditorResources.GetBundleGroup();
            var input = GetWindow<TextFieldWindow>();
            input.minSize = new Vector2(200,100);
            input.maxSize = new Vector2(200, 100);
            input.OnCreateGroupComplete += (IsCreate) => { AddGroupChoices(IsCreate); };
        }
        private void AddGroupChoices(bool IsCreate = false)
        {
            Group.Clear();
            if(group == null)
                group = LZXEditorResources.GetBundleGroup();
            ScrollView scrollView = new ScrollView();
            Group.Add(scrollView);
            foreach (var group in group.GroupName)
            {
                var toggle = new Toggle();
                toggle.text = group;
                toggle.style.backgroundColor = new StyleColor(Color.gray);
                scrollView.Add(toggle);
            }
        }
    }
}
