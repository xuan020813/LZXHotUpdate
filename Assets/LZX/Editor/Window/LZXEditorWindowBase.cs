using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace LZX.MEditor.Window
{
    public class LZXEditorWindowBase: EditorWindow
    {
        public VisualTreeAsset uxml;
        public StyleSheet uss;
        public VisualElement root;
        public virtual void CreateGUI()
        {
            root = uxml.CloneTree();
            root.name = "root";
            root.styleSheets.Add(uss);
            rootVisualElement.Add(root);
        }
    }
}