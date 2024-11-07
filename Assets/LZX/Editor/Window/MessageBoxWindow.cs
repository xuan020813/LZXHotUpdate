using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace LZX.MEditor.Window
{
    public class MessageBoxWindow : EditorWindow
    {
        public VisualTreeAsset uxml;
        public StyleSheet uss;

        public Action OnYesButtonClick;
        public Action OnCancelButtonClick;

        public Label message;

        public static void Show(string message, Action onYesButtonClick = null, Action onCancelButtonClick = null)
        {
            var window = GetWindow<MessageBoxWindow>();
            window.minSize = new Vector2(300, 120);
            window.maxSize = new Vector2(300, 120);
            window.message.text = message;
            window.OnYesButtonClick = onYesButtonClick;
            window.OnCancelButtonClick = onCancelButtonClick;
        }
        private void CreateGUI()
        {
            var root = uxml.CloneTree();
            root.styleSheets.Add(uss);
            message = root.Q<Label>("messagebox");
            root.Q<Button>("btn_yes").clicked += () => 
            { 
                OnYesButtonClick?.Invoke(); 
                Close();
            };
            root.Q<Button>("btn_cancel").clicked += () =>
            {
                OnCancelButtonClick?.Invoke();
                Close();
            };
            rootVisualElement.Add(root);
            
        }
    }    
}

