using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LZX.MEditor.LZXStatic;
using LZX.MEditor.MScriptableObject;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace LZX.MEditor.Window
{
    public class SelectWindow : EditorWindow
    {
        public VisualTreeAsset window;
        public StyleSheet uss;

        public VisualElement container;
        private DropdownField dropdown;

        public VisualTreeAsset DateInput;
        public StyleSheet DateInputUss;


        private VisualElement dateinput;

        private void CreateGUI()
        {
            var root = window.CloneTree();
            this.rootVisualElement.Add(root);
            root.styleSheets.Add(uss);
            container = root.Q("containter");
            dateinput = DateInput.CloneTree();
            dateinput.styleSheets.Add(DateInputUss);

            var btn_before = dateinput.Q<Button>("btn_before");
            btn_before.clicked += () => { ShowDateSelectWindow(true); };
            var btn_after = dateinput.Q<Button>("btn_after");
            btn_after.clicked += () => { ShowDateSelectWindow();};
            label_left = dateinput.Q<Label>("label_left");
            label_right = dateinput.Q<Label>("label_right");
            #region 按钮

            Button btn = new Button() { text = "筛选" };
            btn.clicked += () => { OnYesButtonClick();};
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

            container.Add(dateinput);

            dropdown = root.Q<DropdownField>("rule-dropdown");

            dropdown.RegisterValueChangedCallback(OnDropdownChanged);
        }
        private void OnYesButtonClick()
        {
            switch (dropdown.value)
            {
                case "日期":
                    ScreenWithDate();
                    break;
                case "分组":
                    ScreenWithGroup();
                    break;
                case "None":
                    ScreenWithNone();
                    break;
            }
        }
        private void ScreenWithNone()
        {
            var window = EditorWindow.GetWindow<LeftRootWindow>();
            window.Show();
            window.minSize = new Vector2(600, 600);
            window.OnScreening("", LeftRootWindow.ScreeningType.NONE);
            Close();
        }
        private void ScreenWithGroup()
        {
            var window = EditorWindow.GetWindow<LeftRootWindow>();
            window.Show();
            window.minSize = new Vector2(600, 600);
            var chioce = TempGroupNameList.Where(v => v.value).Select(v => v.text).ToList();
            window.OnScreening(string.Join("_",chioce), LeftRootWindow.ScreeningType.GROUP);
            Close();
        }
        private void ScreenWithDate()
        {
            if (!string.IsNullOrEmpty(label_left.text) && !string.IsNullOrEmpty(label_right.text))
            {
                var before = DateTime.Parse(label_left.text);
                var after = DateTime.Parse(label_right.text);
                if (before > after)
                    throw new Exception("左边日期必须小于右边");
            }
            else
            {
                throw new Exception("没选日期你筛选你*呢？");
            }
            var window = EditorWindow.GetWindow<LeftRootWindow>();
            window.Show();
            window.minSize = new Vector2(600, 600);
            string str = label_left.text + "+" + label_right.text;
            window.OnScreening(str, LeftRootWindow.ScreeningType.DATETIME);
            Close();
        }
        private Label label_left;
        private Label label_right;
        private void ShowDateSelectWindow(bool IsLeft = false)
        {
            var window = EditorWindow.GetWindow<DateSelectWindow>();
            window.Show();
            window.maxSize = new Vector2(300, 300);
            window.minSize = new Vector2(300, 300);
            window.OnYesButtonClick += (date) =>
            {
                if(IsLeft)
                    label_left.text = date;
                else
                    label_right.text = date;
                window.Close();
            };
        }
        private void OnDropdownChanged(ChangeEvent<string> evt)
        {
            switch (this.dropdown.index)
            {
                case 0:
                    container.Clear();
                    container.Add(dateinput);
                    break;
                case 1:
                    container.Clear();
                    AddToggle();
                    break;
                case 2:
                    container.Clear();
                    break;
            }
        }
        private List<Toggle> TempGroupNameList = new List<Toggle>();
        private void AddToggle()
        {
            TempGroupNameList.Clear();
            var group = LZXEditorResources.GetBundleGroup();
            for (int i = 0; i < group.GroupName.Count; i++)
            {
                Toggle toggle = new Toggle() { text = group.GroupName[i] };
                TempGroupNameList.Add(toggle);
                container.Add(toggle);
            }
        }
    }
}
