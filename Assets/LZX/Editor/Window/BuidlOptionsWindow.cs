using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using LZX.MEditor.LZXStatic;
using LZX.MEditor.MScriptableObject;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

namespace LZX.MEditor.Window.Item
{
    public class BuidlOptionsWindow:EditorWindow
    {
        public VisualTreeAsset uxml;
        public StyleSheet uss;

        #region UI引用
        public DropdownField dropdown_compression;
        public TextField txt_output;
        public TextField txt_loadinguipath;
        public Toggle togl_win;
        public Toggle togl_ios;
        public Toggle togl_android;
        public Toggle togl_followbundle;
        public Toggle togl_clearfloder;
        public Toggle togl_copytostreamingassets;
        public Toggle togl_excludetypeinfomataion;
        public Toggle togl_forcerebuild;
        public Toggle togl_ignoretypetreechanges;
        public Toggle togl_appendhash;
        public Toggle togl_strictmode;
        public Toggle togl_dryrunbuild;
        public Toggle togl_useversioncontrol;
        public Toggle togl_forcereplay;
        public Button btn_build;
        #endregion
        
        public List<Bundle> bundles;
        public MScriptableObject.BuildOptions buildOptions;
        public bool BuildAll = false;
        private void CreateGUI()
        {
            var root = uxml.CloneTree();
            root.styleSheets.Add(uss);
            rootVisualElement.Add(root);
            buildOptions = LZXEditorResources.GetBuildOptions();
            #region UI引用
            dropdown_compression = root.Q<DropdownField>("dropdown_compression");
            txt_output = root.Q<TextField>("txt_output");
            txt_output.RegisterCallback<ClickEvent>(OnClickTextField);
            txt_loadinguipath = root.Q<TextField>("txt_loadinguiname");
            txt_loadinguipath.RegisterCallback<ClickEvent>(OnClickTextFieldFile);
            togl_win = root.Q<Toggle>("togl_win");
            togl_ios = root.Q<Toggle>("togl_ios");
            togl_android = root.Q<Toggle>("togl_android");
            togl_clearfloder = root.Q<Toggle>("togl_clearfloder");
            togl_copytostreamingassets = root.Q<Toggle>("togl_copytostreamingassets");
            togl_excludetypeinfomataion = root.Q<Toggle>("togl_excludetypeinfomataion");
            togl_forcerebuild = root.Q<Toggle>("togl_forcerebuild");
            togl_ignoretypetreechanges = root.Q<Toggle>("togl_ignoretypetreechanges");
            togl_appendhash = root.Q<Toggle>("togl_appendhash");
            togl_strictmode = root.Q<Toggle>("togl_strictmode");
            togl_dryrunbuild = root.Q<Toggle>("togl_dryrunbuild");
            togl_followbundle = root.Q<Toggle>("togl_followbundle");
            togl_followbundle.RegisterValueChangedCallback((evt) => { OnFollowBundle(); });
            togl_useversioncontrol = root.Q<Toggle>("togl_useversioncontrol");
            togl_forcereplay = root.Q<Toggle>("togl_forcereplay");
            btn_build = root.Q<Button>("btn_build");
            btn_build.clicked += Build;
            #endregion
            #region 赋值
            dropdown_compression.index = buildOptions.Compression;
            txt_output.value = buildOptions.outputPath;
            txt_loadinguipath.value = buildOptions.LoadingUIPath;
            togl_win.value = buildOptions.buildTargets.Contains(BuildTarget.StandaloneWindows64);
            togl_ios.value = buildOptions.buildTargets.Contains(BuildTarget.iOS);
            togl_android.value = buildOptions.buildTargets.Contains(BuildTarget.Android);
            togl_followbundle.value = buildOptions.FollowBundle;
            togl_clearfloder.value = buildOptions.ClearFloder;
            togl_copytostreamingassets.value = buildOptions.CopyTOStreamingAssets;
            togl_excludetypeinfomataion.value = buildOptions.ExcludeTypeInformation;
            togl_forcerebuild.value = buildOptions.ForceRebuild;
            togl_ignoretypetreechanges.value = buildOptions.IgnoreTypeTreeChanges;
            togl_appendhash.value = buildOptions.AppendHash;
            togl_strictmode.value = buildOptions.StrictMode;
            togl_dryrunbuild.value = buildOptions.DryRunBuild;
            togl_useversioncontrol.value = buildOptions.UseVersionControl;
            togl_forcereplay.value = buildOptions.ForceReStart;
            #endregion
        }
        private void OnClickTextField(ClickEvent evt)
        {
            var txtf = evt.target as TextField; 
            if (evt.clickCount == 2)
            {
                // 弹出文件选择窗口
                string path = EditorUtility.OpenFolderPanel("Select a Folder", 
                    Application.dataPath,"");
                if (!string.IsNullOrEmpty(path))
                {
                    // 将选择的路径设置为 TextField 的值
                    txtf.value = path;
                }
            }
        }
        private void OnClickTextFieldFile(ClickEvent evt)
        {
            var txtf = evt.target as TextField; 
            if (evt.clickCount == 2)
            {
                // 弹出文件选择窗口
                string path = EditorUtility.OpenFilePanel("Select a File", 
                    Application.dataPath,"");
                if (!string.IsNullOrEmpty(path))
                {
                    // 将选择的路径设置为 TextField 的值
                    txtf.value = path.Replace(Application.dataPath, "Assets");
                }
            }
        }
        private void OnFollowBundle()
        {
            togl_win.style.display = !togl_followbundle.value ? DisplayStyle.Flex : DisplayStyle.None;
            togl_ios.style.display = !togl_followbundle.value ? DisplayStyle.Flex : DisplayStyle.None;
            togl_android.style.display = !togl_followbundle.value ? DisplayStyle.Flex : DisplayStyle.None;
        }
        private void Build()
        {
            #region 检查
            if (string.IsNullOrEmpty(txt_output.value))
            {
                EditorUtility.DisplayDialog("Error", "OutPutPath是必须填的", "OK");
                return;
            }

            if (string.IsNullOrEmpty(txt_loadinguipath.value))
            {
                EditorUtility.DisplayDialog("Error", "LoadingUIName是必须填的", "OK");
                return;
            }
            if (!togl_android.value && !togl_ios.value && !togl_win.value && !togl_followbundle.value)
            {
                EditorUtility.DisplayDialog("Error", "构建平台为必选项", "OK");
                return;
            }
            #endregion
            #region 赋值
            buildOptions.Compression = dropdown_compression.index;
            buildOptions.outputPath = txt_output.value;
            buildOptions.LoadingUIPath = txt_loadinguipath.value;
            buildOptions.buildTargets.Clear();
            if (togl_win.value)
                buildOptions.buildTargets.Add(BuildTarget.StandaloneWindows64);
            if (togl_ios.value)
                buildOptions.buildTargets.Add(BuildTarget.iOS);
            if (togl_android.value)
                buildOptions.buildTargets.Add(BuildTarget.Android);
            buildOptions.FollowBundle = togl_followbundle.value;
            buildOptions.ClearFloder = togl_clearfloder.value;
            buildOptions.CopyTOStreamingAssets = togl_copytostreamingassets.value;
            buildOptions.ExcludeTypeInformation = togl_excludetypeinfomataion.value;
            buildOptions.ForceRebuild = togl_forcerebuild.value;
            buildOptions.IgnoreTypeTreeChanges = togl_ignoretypetreechanges.value;
            buildOptions.AppendHash = togl_appendhash.value;
            buildOptions.StrictMode = togl_strictmode.value;
            buildOptions.DryRunBuild = togl_dryrunbuild.value;
            buildOptions.UseVersionControl = togl_useversioncontrol.value;
            buildOptions.ForceReStart = togl_forcereplay.value;
            EditorUtility.SetDirty(buildOptions);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            #endregion
            if (BuildAll)
                LZXBundleBuild.Build(LZXEditorResources.GetVersionController().Bundles.Values.ToList());
            else
                LZXBundleBuild.Build(bundles,false);
            Close();
        }
    }
}