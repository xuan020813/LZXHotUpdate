using System;
using System.IO;
using LZX.MEditor.LZXStatic;
using LZX.MScriptableObject;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;

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
        // private TextField txtf_bundleoutpath;
        private TextField txtf_bundlefileex;
        private TextField txtf_loadingbundlename;
        private TextField txtf_loadingbundlepath;
        private TextField txtf_hotupdatedllname;

        public VisualElement loadingName;
        public VisualElement loadingPath;
        public ScrollView scroll_compress;
        private void CreateGUI()
        {
            var root = uxml.CloneTree();
            root.styleSheets.Add(uss);
            rootVisualElement.Add(root);

            // scroll_compress = root.Q<ScrollView>("dropdown_compress");
            // InitDropDown();
            #region AddCallBack
            txtf_bundlefileex = root.Q<TextField>("txtf_bundlefileex");
            txtf_resurl =  root.Q<TextField>("txtf_resurl");
            txtf_resurl.RegisterCallback<ClickEvent>(OnPathTextFieldClick);
            // txtf_bundleoutpath = root.Q<TextField>("txtf_bundleoutpath");
            // txtf_bundleoutpath.RegisterCallback<ClickEvent>(OnPathTextFieldClick);
            txtf_loadingbundlename = root.Q<TextField>("txtf_loadingbundlename");
            txtf_loadingbundlename.RegisterCallback<ClickEvent>(OnFileTextFieldClick);
            txtf_loadingbundlepath = root.Q<TextField>("txtf_loadingbundlepath");
            txtf_loadingbundlepath.RegisterCallback<ClickEvent>(OnPathTextFieldClick);
            txtf_hotupdatedllname = root.Q<TextField>("txtf_hotupdatedllname");
            txtf_hotupdatedllname.RegisterCallback<ClickEvent>(OnFileTextFieldClick);
            var btn_save = root.Q<Button>("btn_save");
            btn_save.clicked += () => { SaveSetting(); };
            #endregion
            InitText();
            
            loadingName = root.Q<VisualElement>("content_loadingname");
            loadingPath = root.Q<VisualElement>("content_loadingpath");
        }

        private void InitDropDown()
        {
            var options = new string[] {"None", 
                "UncompressedAssetBundle(不对AssetBundle进行压缩)",
                "DisableWriteTypeTree(在打包时禁用类型树写入)",
                "ForceRebuildAssetBundle(强制重新构建 AssetBundle，即使现有的资源未发生变化)",
                "IgnoreTypeTreeChanges(在增量构建时忽略类型树的更改)",
                "AppendHashToAssetBundleName(在 AssetBundle 名称后附加哈希值)",
                "ChunkBasedCompression(使用块压缩)",
                "StrictMode(如果构建过程中有任何错误，则不允许构建成功)",
                "DryRunBuild(执行干运行构建，即不会实际生成 AssetBundle 文件)",
                "DisableLoadAssetByFileName(禁用通过文件名加载资源的功能)",
                "DisableLoadAssetByFileNameWithExtension(禁用通过文件名及扩展名加载资源的功能)",
                "AssetBundleStripUnityVersion(在构建过程中移除 Unity 版本号信息)",
                "UseContentHash(使用资源内容的哈希值计算 AssetBundle 的哈希)",
                "RecurseDependencies(递归计算 AssetBundle 的依赖项)"
            };
            scroll_compress.mode = ScrollViewMode.Vertical;
            scroll_compress.horizontalScroller.style.display = DisplayStyle.None;
            for (int i = 0; i < options.Length; i++)
            {
                Toggle label = new Toggle(options[i]);
                //label.Q<VisualElement>("VisualElement").Add(new Label(options[i]));
                label.labelElement.style.fontSize = 10;
                label.labelElement.style.unityTextAlign = TextAnchor.MiddleLeft;
                label.labelElement.style.width = new StyleLength(new Length(96, LengthUnit.Percent));
                label.styleSheets.Add(uss);
                scroll_compress.Add(label);
            }
        }

        private void InitText()
        {
            var setting = LZXEditorResources.GetSetting();
            if(setting == null)
                return;
            txtf_resurl.value = setting.ResourcesURL;
            // txtf_bundleoutpath.value = setting.BundleOutPath;
            txtf_bundlefileex.value = setting.BundleEx;
            txtf_loadingbundlename.value = setting.LoadingBundleName;
            txtf_loadingbundlepath.value = setting.LoadingBundlePath;
            txtf_hotupdatedllname.value = setting.HotUpdateDllName;
        }

        private void SaveSetting()
        {
            if(string.IsNullOrEmpty(txtf_resurl.value))
                throw new Exception("资源服务器地址不能为空！");
            // if (string.IsNullOrEmpty(txtf_bundleoutpath.value))
            //     throw new Exception("AB包输出路径不能为空！");
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
            if (!string.IsNullOrEmpty(txtf_loadingbundlename.value))
            {
                
            }
            if (!string.IsNullOrEmpty(txtf_loadingbundlepath.value) && !txtf_loadingbundlepath.value.StartsWith("Assets/"))
                throw new Exception("AB包加载路径必须在Assets目录下！");
            if (string.IsNullOrEmpty(txtf_hotupdatedllname.value))
                throw new Exception("热更DLL名称不能为空！");
            if(!txtf_hotupdatedllname.value.EndsWith(".bytes"))
                    throw new Exception("热更DLL名称必须以.bytes结尾！");
            Setting obj = ScriptableObject.CreateInstance<Setting>();
            obj.ResourcesURL = txtf_resurl.value;
            obj.BundleEx = txtf_bundlefileex.value;
            // obj.BundleOutPath = txtf_bundleoutpath.value;
            obj.LoadingBundleName = txtf_loadingbundlename.value;
            obj.LoadingBundlePath = txtf_loadingbundlepath.value;
            obj.HotUpdateDllName = txtf_hotupdatedllname.value;
            AssetDatabase.CreateAsset(obj, "Assets/LZX/ScriptableObject/Setting.asset");
            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("提示", "设置保存成功！", "确定");
            AssetDatabase.Refresh();
            Close();
        }

        private void OnFileTextFieldClick(ClickEvent evt)
        {
            var txtf = evt.target as TextField; 
            if (evt.clickCount == 2)
            {
                // 弹出文件选择窗口
                string path = EditorUtility.OpenFilePanel("Select a Bundle", Application.dataPath, "");
                if (!string.IsNullOrEmpty(path))
                {
                    // 将选择的路径设置为 TextField 的值
                    txtf.value = Path.GetFileName(path);
                }
            }
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