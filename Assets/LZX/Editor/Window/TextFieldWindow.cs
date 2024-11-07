using System;
using System.Collections.Generic;
using System.IO;
using LZX.MEditor.LZXStatic;
using LZX.MEditor.MScriptableObject;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class TextFieldWindow : EditorWindow
{
    public VisualTreeAsset UXML;
    public StyleSheet USS;
    private TextField textField;

    private BundleGroup group;

    public Action<bool> OnCreateGroupComplete;
    private void CreateGUI()
    {
        var root = UXML.CloneTree();
        root.styleSheets.Add(USS);
        textField = root.Q<TextField>("txf_groupname");
        var btn_yes = root.Q<Button>("btn_yes");
        btn_yes.clicked += () => { CreateGroup(textField.value); };
        this.rootVisualElement.Add(root);
    }
    private void CreateGroup(string textFieldValue)
    {
        if (string.IsNullOrEmpty(textFieldValue))
            throw new Exception("请输入组名");
        if (group == null)
            group = LZXEditorResources.GetBundleGroup();
        HashSet<string> Grouphash = new HashSet<string>(group.GroupName);
        if(Grouphash.Contains(textFieldValue))
            throw new Exception("该组名已存在");
        group.GroupName.Add(textFieldValue);
        EditorUtility.SetDirty(group);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        OnCreateGroupComplete?.Invoke(true);
        Close();
    }
}
