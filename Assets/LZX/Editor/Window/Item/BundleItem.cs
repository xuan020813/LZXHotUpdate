using System;
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
        public VisualElement Root;
        public Button btn_reset;
        public Button btn_delete;
        public BundleItem(Bundle bundle, VisualElement root)
        {
            Name = root.Q<Label>("label_name");
            btn_reset = root.Q<Button>("btn_remove");
            var label_name = root.Q<Label>("label_name");
            Bundle = bundle;
            Name.text = bundle.Name;
            Root = root;
            label_name.RegisterCallback<ClickEvent>(OnClick);
        }
        private void OnClick(ClickEvent evt)
        {
            BundleInfoWindow window = EditorWindow.GetWindow<BundleInfoWindow>();
            window.RefreshAsset(Bundle);
        }
    }
}