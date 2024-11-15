using System;
using System.Collections.Generic;
using System.IO;
using HybridCLR.Editor.Settings;
using LZX.MEditor.LZXStatic;
using LZX.MEditor.MScriptableObject;
using LZX.MScriptableObject;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace LZX.MEditor.Window
{
    public class SettingWindow: EditorWindow
    {
        public VisualTreeAsset uxml;
        public StyleSheet uss;
        
        [MenuItem("LZX/Setting")]
        public static void ShowWindow()
        {
            var window = EditorWindow.GetWindow<SettingWindow>();
            window.minSize = new Vector2(480, 180);
        }
        private TextField txtf_resurl;
        private TextField txtf_bundlefileex;
        
        private void CreateGUI()
        {
            var root = uxml.CloneTree();
            root.styleSheets.Add(uss);
            rootVisualElement.Add(root);
            
            #region AddCallBack
            txtf_bundlefileex = root.Q<TextField>("txtf_bundlefileex");
            txtf_resurl =  root.Q<TextField>("txtf_resurl");
            txtf_resurl.RegisterCallback<ClickEvent>(OnPathTextFieldClick);
            var btn_save = new Button();
            btn_save.text = "保存";
            btn_save.clicked += () => { SaveSetting(); };
            #endregion
            var setting = LZXEditorResources.GetSetting();
            if(setting == null)
                throw new Exception("Setting.asset not found!");
            txtf_resurl.value = setting.ResourcesURL;
            txtf_bundlefileex.value = setting.BundleEx;
            
            SerializedObject asmdeflist = new SerializedObject(setting);
            SerializedProperty sp_asmdefs = asmdeflist.FindProperty("asmdefs");
            PropertyField asmder_field = new PropertyField(sp_asmdefs,"HotUpdate Assemblies");
            asmder_field.Bind(asmdeflist);
            root.Add(asmder_field);
            root.Add(btn_save);
            
        }
        private void SaveSetting()
        {
            if(string.IsNullOrEmpty(txtf_resurl.value))
                throw new Exception("资源服务器地址不能为空！");
            if (!string.IsNullOrEmpty(txtf_bundlefileex.value))
            {
                if (!txtf_bundlefileex.value.StartsWith("."))
                {
                    var str = txtf_bundlefileex.value;
                    txtf_bundlefileex.value = "." + str;
                }
            }
            else
                throw new Exception("AB包文件扩展名不能为空！");
            Setting obj = LZXEditorResources.GetSetting();
            obj.ResourcesURL = txtf_resurl.value;
            obj.BundleEx = txtf_bundlefileex.value;
            if (obj.asmdefs.Count == 0)
            {
                if(!EditorUtility.DisplayDialog("提示", "确定要删除HotUpdate程序集嘛？此程序集为热更新逻辑程序集", "确定"))
                    return;
            }

            HybridCLRSettings.Instance.hotUpdateAssemblyDefinitions = obj.asmdefs.ToArray();
            HybridCLRSettings.Save();
            EditorUtility.SetDirty(obj);
            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("提示", "设置保存成功！", "确定");
            AssetDatabase.Refresh();
            Close();
        }
        private void OnPathTextFieldClick(ClickEvent evt)
        {
            var txtf = evt.target as TextField; 
            if (evt.clickCount == 2)
            {
                // 弹出文件选择窗口
                string path = EditorUtility.OpenFolderPanel("Select a Folder", Application.dataPath, "");
                if (!string.IsNullOrEmpty(path))
                {
                    // 将选择的路径设置为 TextField 的值
                    txtf.value = path;
                }
            }
        }
    }
}