using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using LZX.MEditor.Enum;
using LZX.MEditor.LZXStatic;
using LZX.MEditor.MScriptableObject;
using LZX.MEditor.Window.Item;
using LZX.MEditor.WindowLogic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace LZX.MEditor.Window
{
    public class BundleInfoWindow : EditorWindow
    {
        [MenuItem("LZX/Windows/打包粒度控制窗口")]
        public static void ShowWindow()
        {
            var win = GetWindow<BundleInfoWindow>();
            win.minSize = new Vector2(600, 600);
        }
        [MenuItem("LZX/Windows/合成窗口")]
        public static void ShowCompositionWindow()
        {
            Type rightrootType = typeof(RightRootWindow);
            Type leftrootType = typeof(LeftRootWindow);
            Type inforootType = typeof(InfoWindow);
            //创建最外层Container
            object containerInstance = EditorContainerWindow.CreateInstance();
            //创建分屏容器
            object splitViewInstance = EditorSplitView.CreateInstance();
            //跟容器
            EditorContainerWindow.SetRootView(containerInstance, splitViewInstance);
            #region 左侧合成窗口
            //leftroot与inforoot
            object leftSpliteViewInstance = EditorSplitView.CreateInstance();
            EditorSplitView.SetPosition(leftSpliteViewInstance,new Rect(0,0,600,800));
            //设置垂直状态
            EditorSplitView.SetVertical(leftSpliteViewInstance,true);
            object leftrootDockArea = EditorDockArea.CreateInstance();
            EditorDockArea.SetPosition(leftrootDockArea,new Rect(0,0,600,600));
            EditorWindow leftrootWindow = (EditorWindow) CreateInstance(leftrootType);
            leftrootWindow.titleContent.text = "Bundles";
            EditorDockArea.AddTab(leftrootDockArea,leftrootWindow);
            EditorSplitView.AddChild(leftSpliteViewInstance,leftrootDockArea);
            //inforoot
            object inforootDoceArea = EditorDockArea.CreateInstance();
            EditorDockArea.SetPosition(inforootDoceArea,new Rect(0,0,600,200));
            EditorWindow inforootWindow = (EditorWindow) CreateInstance(inforootType);
            inforootWindow.titleContent.text = "Info";
            EditorDockArea.AddTab(inforootDoceArea,inforootWindow);
            EditorSplitView.AddChild(leftSpliteViewInstance,inforootDoceArea);
            EditorSplitView.AddChild(splitViewInstance,leftSpliteViewInstance);
            #endregion
            #region 右侧合成窗口
            //添加右侧DockArea
            object rightDockAreaInstance = EditorDockArea.CreateInstance();
            EditorDockArea.SetPosition(rightDockAreaInstance, new Rect(0, 0, 200, 800));
            EditorWindow rightrootWindow = (EditorWindow)CreateInstance(rightrootType);
            rightrootWindow.titleContent.text = "Assets";
            EditorDockArea.AddTab(rightDockAreaInstance, rightrootWindow);
            EditorSplitView.AddChild(splitViewInstance, rightDockAreaInstance);
            #endregion
            EditorEditorWindow.MakeParentsSettingsMatchMe(rightrootWindow);
            EditorEditorWindow.MakeParentsSettingsMatchMe(leftrootWindow);
            EditorEditorWindow.MakeParentsSettingsMatchMe(inforootWindow);
            EditorContainerWindow.SetPosition(containerInstance,new Rect(100,100,800,800));
            EditorSplitView.SetPosition(splitViewInstance,new Rect(0,0,800,800));
            EditorContainerWindow.Show(containerInstance,0,true,false,true);
            EditorContainerWindow.OnResize(containerInstance);

            // var root = this.rootVisualElement;
            // var container = infoItem.CloneTree();
            // container.styleSheets.Add(uss);
            //
            // BundleRoot = container.Q<ScrollView>("litemroot");
            // AssetRoot = container.Q<ScrollView>("ritemroot");
            // AssetRoot.RegisterCallback<DragEnterEvent>(OnDragEnter);
            // AssetRoot.RegisterCallback<DragUpdatedEvent>(OnDragUpdated);
            // AssetRoot.RegisterCallback<DragLeaveEvent>(OnDragLeave);
            // AssetRoot.RegisterCallback<DragPerformEvent>(OnDragPerform);
            //
            // btnroot = container.Q<VisualElement>("btnroot");
            // multiselectroot = container.Q<VisualElement>("multiselectroot");
            // #region Label
            // label_group = container.Q<Label>("label_group");
            // label_dependience = container.Q<Label>("label_dependience");
            // label_createdate = container.Q<Label>("label_createdate");
            // label_platform = container.Q<Label>("label_platform");
            // label_isbuild = container.Q<Label>("label_isbuild");
            // label_guid = container.Q<Label>("label_guid");
            // label_assetcount = container.Q<Label>("label_assetcount");
            // label_name = container.Q<Label>("label_name");
            // #endregion
            // #region BUttonRoot
            // var btn_addgroup = container.Q<Button>("btn_addgroup");
            // btn_addgroup.clicked += AddBundle;
            // var btn_select = container.Q<Button>("btn_select");
            // btn_select.clicked += OnSelectClick;
            // var btn_buildAll = container.Q<Button>("btn_buildAll");
            // btn_buildAll.clicked += OnBuildAllClick;
            // var btn_multiselect = container.Q<Button>("btn_multiselect");
            // btn_multiselect.clicked += () => { OnMultiSelectClick(); };
            // var btn_emptyasset = container.Q<Button>("btn_emptyasset");
            // btn_emptyasset.clicked += OnEmptyAssetClick;
            // #endregion
            // #region 多选Button
            // var btn_cancel = container.Q<Button>("btn_cancel");
            // btn_cancel.clicked += () => { OnMultiSelectClick(); };
            // btn_allselect = container.Q<Button>("btn_allselect");
            // btn_allselect.clicked += OnAllSelectClick;
            // var btn_inverse = container.Q<Button>("btn_inverse");
            // btn_inverse.clicked += OnInvertSelectClick;
            // var btn_buildselect = container.Q<Button>("btn_buildselect");
            // btn_buildselect.clicked += OnBuildSelectClick;
            // var btn_deleteselect = container.Q<Button>("btn_deleteselect");
            // btn_deleteselect.clicked += OnDeleteSelectClick;
            // #endregion
            // #region Search
            // container.Q<TextField>("txtf_bundlesearch").RegisterValueChangedCallback(OnSearchBundleChange);
            // container.Q<TextField>("txtf_bundlesearch1").RegisterValueChangedCallback(OnSearchBundleChange);
            // container.Q<TextField>("txtf_assetsearch").RegisterValueChangedCallback(OnSearchAssetChange);
            // #endregion
            // root.Add(container);
            // RefreshBundle();
        }
        // #region 拖拽
        // private void OnDragEnter(DragEnterEvent evt)
        // {
        //     if (DragAndDrop.objectReferences.Length > 0)
        //     {
        //         // 设置拖拽操作的效果
        //         DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
        //     }
        //     else
        //     {
        //         DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
        //     }
        // }
        // private void OnDragLeave(DragLeaveEvent evt)
        // {
        //     
        // }
        // private void OnDragUpdated(DragUpdatedEvent evt)
        // {
        //     if (DragAndDrop.objectReferences.Length > 0)
        //     {
        //         DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
        //     }
        //     else
        //     {
        //         DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
        //     }
        // }
        // private void OnDragPerform(DragPerformEvent evt)
        // {
        //     if (curSelectBundle == null)
        //         throw new Exception("未选择Bundle,您往哪拖？");
        //     foreach (var obj in DragAndDrop.objectReferences)
        //     {
        //         string assetPath = AssetDatabase.GetAssetPath(obj);
        //         if(assetPath.EndsWith(".meta") || assetPath.EndsWith(".asset"))
        //             continue;
        //         string absolutePath = assetPath.Replace("Assets", Application.dataPath);
        //         List<string> assets = new List<string>();
        //         if (IsDir(absolutePath))
        //         {
        //             assets.AddRange(Directory.GetFiles(absolutePath, "*", SearchOption.AllDirectories)
        //                 .Where(v => !v.EndsWith(".meta") && !v.EndsWith(".asset"))
        //                 .Select(v => v.Replace(Application.dataPath, "Assets"))
        //                 .ToList());
        //         }
        //         else
        //         {
        //             assets.Add(assetPath);
        //         }
        //         AddAsset(assets);
        //     }
        // }
        // private void ProcessingDrag(string assetPath, string absolutePath)
        // {
        //     if (IsDir(absolutePath))
        //     {
        //         //Debug.Log("拖拽文件夹");
        //         string[] files = Directory.GetFiles(absolutePath);
        //         for (int i = 0; i < files.Length; i++)
        //         {
        //             if (files[i].EndsWith(".meta") || files[i].EndsWith(".asset"))
        //                 continue;
        //             string fileAssetPath = files[i].Replace(Application.dataPath, "Assets"); //貌似不能在这里GetFileName
        //             ProcessingDrag(fileAssetPath, files[i]);
        //         }
        //
        //         string[] dirs = Directory.GetDirectories(absolutePath);
        //         for (int i = 0; i < dirs.Length; i++)
        //         {
        //             ProcessingDrag("", dirs[i]);
        //         }
        //     }
        //     else
        //         AddAsset(assetPath);
        //
        // }
        // private bool IsDir(string absolutePath)
        // {
        //     if (Directory.Exists(absolutePath))
        //         return true;
        //     return false;
        // }
        // #endregion
        // #region Refresh
        // public void RefreshBundle()
        // {
        //     BundleRoot.Clear();
        //     List<Bundle> bundles = GetBundles();
        //     BG_toggles.Clear();
        //     selectBundles.Clear();
        //     int count = 0;
        //     foreach (var bundle in bundles)
        //     {
        //         count++;
        //         var itemroot = item.CloneTree();
        //         itemroot.styleSheets.Add(itemuss);
        //         BundleItem bundleItem = new BundleItem(bundle,itemroot);
        //         var labelroot = itemroot.Q<VisualElement>("BG");
        //         //if (!IsMultiSelect)
        //         //{
        //         Label label = new Label(count.ToString());
        //         labelroot.Add(label);
        //         //}
        //         // else
        //         // {
        //         //     Toggle toggle = new Toggle();
        //         //     toggle.RegisterValueChangedCallback((evt) => { OnSelectToggleValueChanged(bundle, toggle.value); });
        //         //     labelroot.Add(toggle);
        //         //     BG_toggles.Add(toggle);
        //         // }
        //         InitButton(itemroot, bundle);
        //         BundleRoot.Add(itemroot);
        //     }
        // }
        // private void InitButton(TemplateContainer itemroot, Bundle bundle)
        // {
        //     var btn_setting = itemroot.Q<Button>("btn_remove");
        //     btn_setting.clicked += () => { OnSettingClick(bundle); };
        //     btn_setting.style.backgroundImage = new StyleBackground(LZXEditorResources.GetIcon(LZXIconType.setting));
        //     var btn_delete = itemroot.Q<Button>("btn_expansion");
        //     btn_delete.clicked += () => { OnDeleteClick(bundle); };
        //     btn_delete.style.backgroundImage = new StyleBackground(LZXEditorResources.GetIcon(LZXIconType.delete));
        //     var btn_build = itemroot.Q<Button>("btn_build");
        //     btn_build.clicked += () => { OnBuildClick(bundle); };
        //     btn_build.style.backgroundImage = new StyleBackground(LZXEditorResources.GetIcon(LZXIconType.build));
        // }
        // public void RefreshBundle(IEnumerable<Bundle> bundles)
        // {
        //     BundleRoot.Clear();
        //     int count = 0;
        //     foreach (var bundle in bundles)
        //     {
        //         count++;
        //         var itemroot = item.CloneTree();
        //         itemroot.styleSheets.Add(itemuss);
        //         BundleItem bundleItem = new BundleItem(bundle,itemroot);
        //         Label label = new Label(count.ToString());
        //         var labelroot = itemroot.Q<VisualElement>("BG");
        //         labelroot.Add(label);
        //         InitButton(itemroot, bundle);
        //         BundleRoot.Add(itemroot);
        //         
        //     }
        // }
        // public void RefreshAsset(Bundle bundle)
        // {
        //     AssetRoot.Clear();
        //     // string dir = Path.Combine(Application.dataPath, "LZX/Bundles/" + bundle.name);
        //     // var files = Directory.GetFiles(dir);
        //     foreach (var GUID in bundle.AssetGUIDs)
        //     {
        //         // if (file.EndsWith(".meta"))
        //         //     continue;
        //         var asset = new Asset(GUID);
        //         var itemroot = item.CloneTree();
        //         itemroot.styleSheets.Add(itemuss);
        //         AssetItem assetItem = new AssetItem(asset,itemroot);
        //         AssetRoot.Add(itemroot);
        //     }
        //     RefreshLabel(bundle);
        // }
        // public void RefreshLabel(Bundle bundle)
        // {
        //     curSelectBundle = bundle;
        //     label_group.text = "分组:";
        //     foreach (var group in bundle.Group)
        //     {
        //         label_group.text += group + ",";
        //     }
        //     label_createdate.text = "创建时间：" + bundle.CreateDate;
        //     label_platform.text = "平台：";
        //     foreach (var plat in bundle.Platform)
        //     {
        //         label_platform.text += plat.ToString()+",";
        //     }
        //     label_isbuild.text = "是否已构建：" + bundle.IsBuild;
        //     label_guid.text = "GUID：" + bundle.GUID;
        //     label_assetcount.text = "资源数量：" + bundle.AssetGUIDs.Count;
        //     label_name.text = "名称：" + bundle.name;
        //     //TODO:dependience添加
        // }
        // public void RefreshLabel(Asset asset)
        // {
        //     label_group.text = "Size:" + asset.Size;
        //     label_createdate.text = "TODO...";
        //     label_platform.text = "TODO...";
        //     label_isbuild.text = "LoadPath：" + asset.LoadPath;
        //     label_guid.text = "GUID：" + asset.GUID;
        //     label_assetcount.text = "TODO...";
        //     label_name.text = "名称：" + asset.Name;
        // }
        // public void RefreshWithDate(string left, string right)
        // {
        //     var bundles = GetBundles();
        //     List<Bundle> selectedBundles = new List<Bundle>();
        //     foreach (var bundle in bundles)
        //     {
        //         DateTime before = DateTime.Parse(left);
        //         DateTime after = DateTime.Parse(right);
        //         if (before <= DateTime.Parse(bundle.CreateDate) && DateTime.Parse(bundle.CreateDate) <= after)
        //         {
        //             selectedBundles.Add(bundle);
        //         }
        //     }
        //     RefreshBundle(selectedBundles);
        // }
        // public void RefreshWithGroup(List<Toggle> tempGroupNameList)
        // {
        //     var bundles = GetBundles();
        //     HashSet<Bundle> selectedBundles = new HashSet<Bundle>();
        //     foreach (var groupName in tempGroupNameList)
        //     {
        //         if(!groupName.value)
        //             continue;
        //         for (int i = bundles.Count - 1; i >= 0; i--)
        //         {
        //             if (bundles[i].Group.Contains(groupName.text))
        //             {
        //                 selectedBundles.Add(bundles[i]);
        //                 bundles.RemoveAt(i);
        //             }
        //         }
        //     }
        //     RefreshBundle(selectedBundles);
        // }
        // private void RefreshBundleWithSearch(string evtNewValue)
        // {
        //     var bundles = GetBundles();
        //     List<Bundle> selectedBundles = new List<Bundle>();
        //     foreach (var bundle in bundles)
        //     {
        //         if (bundle.name.Contains(evtNewValue))
        //         {
        //             selectedBundles.Add(bundle);
        //         }
        //     }
        //     RefreshBundle(selectedBundles);
        //     if(IsMultiSelect)
        //         OnMultiSelectClick(true);
        // }
        // private void RefreshAssetWithSearch(string evtNewValue)
        // {
        //     var asset = curSelectBundle.GetAssetPaths();
        //     AssetRoot.Clear();
        //     foreach (var path in asset)
        //     {
        //         if(!Path.GetFileNameWithoutExtension(path).ContainsInvariantCultureIgnoreCase(evtNewValue))
        //             continue;
        //         string guid = AssetDatabase.AssetPathToGUID(path);
        //         var ass = new Asset(guid);
        //         var itemroot = item.CloneTree();
        //         itemroot.styleSheets.Add(itemuss);
        //         AssetItem assetItem = new AssetItem(ass,itemroot);
        //         AssetRoot.Add(itemroot);
        //     }
        // }
        // #endregion
        // #region Click
        // private void OnSelectClick()
        // {
        //     GetWindow<SelectWindow>().minSize = new Vector2(400, 200);
        // }
        // private void OnDeleteClick(Bundle bundle)
        // {
        //     MessageBoxWindow.Show("确定要删除嘛？", () => { DeleteBundle(bundle);});
        // }
        // private void OnSettingClick(Bundle bundle)
        // {
        //     var window = GetWindow<AddGroupWindow>();
        //     window.minSize = new Vector2(400, 200);
        //     window.maxSize = new Vector2(400, 200);
        //     window.Name.value = bundle.Name;
        //     window.Win.value = bundle.Platform.Contains(BuildTarget.StandaloneWindows64);
        //     //window.Switch.value = bundle.Platform.Contains(BuildTarget.Switch);
        //     window.IOS.value = bundle.Platform.Contains(BuildTarget.iOS);
        //     window.Android.value = bundle.Platform.Contains(BuildTarget.Android);
        //     foreach (var element in window.Group.Children())
        //     {
        //         if (element is ScrollView)
        //         {
        //             foreach (var toggle in element.Children())
        //             {
        //                 if (toggle is Toggle t)
        //                 {
        //                     if(bundle.Group.Contains(t.text))
        //                         t.value = true;
        //                     else
        //                         t.value = false;
        //                 }
        //             }
        //         }
        //
        //     }
        //     //window.OnYesButtonClick = () => ResetBundle(window, bundle);
        // }
        // private void OnBuildClick(Bundle bundle)
        // {
        //     List<Bundle> bundles = new List<Bundle>();
        //     bundles.Add(bundle);
        //     BuidlOptionsWindow window = GetWindow<BuidlOptionsWindow>();
        //     window.minSize = new Vector2(600, 370);
        //     window.bundles = bundles;
        //     window.togl_clearfloder.style.display = DisplayStyle.None;
        //     //BuildAll无需赋值
        // }
        // private void OnBuildAllClick()
        // {
        //     List<Bundle> bundles = GetBundles();
        //     BuidlOptionsWindow window = GetWindow<BuidlOptionsWindow>();
        //     window.minSize = new Vector2(600, 370);
        //     window.bundles = bundles;
        //     window.BuildAll = true;
        // }
        // private void OnMultiSelectClick(bool IsSearch = false)
        // {
        //     if(!IsSearch)
        //     {
        //         IsMultiSelect = !IsMultiSelect;
        //         btnroot.style.display = IsMultiSelect ? DisplayStyle.None : DisplayStyle.Flex;
        //         multiselectroot.style.display = IsMultiSelect ? DisplayStyle.Flex : DisplayStyle.None;
        //     }
        //     var items = BundleRoot.Children();
        //     int count = 0;
        //     foreach (var element in items)
        //     {
        //         if (element is TemplateContainer container)
        //         {
        //             count++;
        //             var labelroot = container.Q<VisualElement>("BG");
        //             labelroot.Clear();
        //             if (IsMultiSelect)
        //             {
        //                 Toggle toggle = new Toggle();
        //                 toggle.RegisterValueChangedCallback((evt) => { OnSelectToggleValueChanged(container.Q<Label>("label_name").text, toggle.value); });
        //                 labelroot.Add(toggle);
        //                 BG_toggles.Add(toggle);
        //             }
        //             else
        //             {
        //                 Label label = new Label(count.ToString());
        //                 labelroot.Add(label);
        //             }
        //         }
        //     }
        // }
        // private void OnSelectToggleValueChanged(string bundleName,bool value)
        // {
        //     var bundle = LZXEditorResources.GetBundleWithBundleName(bundleName);
        //     if(bundle == null)
        //         throw new Exception("Bundle不存在");
        //     if(value)
        //         selectBundles.Add(bundle);
        //     else
        //         selectBundles.Remove(bundle);
        // }
        // private void OnDeleteSelectClick()
        // {
        //     for (int i = selectBundles.Count - 1; i >= 0; i--)
        //     {
        //         var bundle = selectBundles.ElementAt(i);
        //         selectBundles.Remove(bundle);
        //         DeleteBundle(bundle);
        //     }
        // }
        // private void OnBuildSelectClick()
        // {
        //     BuidlOptionsWindow window = GetWindow<BuidlOptionsWindow>();
        //     window.minSize = new Vector2(600, 370);
        //     window.bundles = selectBundles.ToList();
        //     window.togl_clearfloder.style.display = DisplayStyle.None;
        // }
        // private void OnInvertSelectClick()
        // {
        //     foreach (var toggle in BG_toggles)
        //     {
        //         toggle.value = !toggle.value;
        //     }
        // }
        // private void OnAllSelectClick()
        // {
        //     IsAllSelect = !IsAllSelect;
        //     btn_allselect.text = IsAllSelect ? "全不选" : "全选";
        //     foreach (var toggle in BG_toggles)
        //     {
        //         toggle.value = IsAllSelect;
        //     }
        // }
        // private void OnEmptyAssetClick()
        // {
        //     MessageBoxWindow.Show("确定要清空资源嘛？", 
        //         () => {
        //         curSelectBundle.AssetGUIDs.Clear();
        //         EditorUtility.SetDirty(curSelectBundle);
        //         AssetDatabase.SaveAssets();
        //         RefreshAsset(curSelectBundle);});
        // }
        // #endregion
        // #region Search
        // private void OnSearchBundleChange(ChangeEvent<string> evt)
        // {
        //     RefreshBundleWithSearch(evt.newValue);
        // }
        // private void OnSearchAssetChange(ChangeEvent<string> evt)
        // {
        //     RefreshAssetWithSearch(evt.newValue);
        // }
        // #endregion
        // private void AddBundle()
        // {
        //     var window = GetWindow<AddGroupWindow>();
        //     window.minSize = new Vector2(400, 200);
        //     window.maxSize = new Vector2(400, 200);
        //     //window.OnYesButtonClick = () => CreateBundle(window);
        // }
        // private void CreateBundle(AddGroupWindow window)
        // {
        //     Bundle bundle = ScriptableObject.CreateInstance<Bundle>();
        //     bundle.Name = window.Name.value;
        //     if (window.Win.value)
        //         bundle.Platform.Add(BuildTarget.StandaloneWindows64);
        //     //if (window.Switch.value)
        //         //bundle.Platform.Add(BuildTarget.Switch);
        //     if (window.IOS.value)
        //         bundle.Platform.Add(BuildTarget.iOS);
        //     if (window.Android.value)
        //         bundle.Platform.Add(BuildTarget.Android);
        //     bundle.GUID = UnityEditor.GUID.Generate().ToString();
        //     bundle.CreateDate = DateTime.Now.ToString();
        //     bundle.IsBuild = "FALSE";
        //     foreach (var item in window.Group.Children())
        //     {
        //         if (item is ScrollView)
        //         {
        //             foreach (var toggle in item.Children())
        //             {
        //                 if (toggle is Toggle { value: true } t)
        //                     bundle.Group.Add(t.text);
        //             }
        //         }
        //
        //     }
        //     if (!Directory.Exists(Path.Combine(Application.dataPath, "LZX/Bundles")))
        //         Directory.CreateDirectory(Path.Combine(Application.dataPath, "LZX/Bundles"));
        //     AssetDatabase.CreateAsset(bundle, "Assets/LZX/Bundles/" + bundle.Name + ".asset");
        //     var version = LZXEditorResources.GetVersionController();
        //     version.Add(bundle.GUID,bundle);
        //     AssetDatabase.SaveAssets();
        //     // Directory.CreateDirectory(Path.Combine(Application.dataPath, "LZX/Bundles/" + bundle.Name));
        //     AssetDatabase.Refresh();
        //     RefreshBundle();
        //     window.Close();
        // }
        // private void ResetBundle(AddGroupWindow window, Bundle bundle)
        // {
        //     var oldName = bundle.Name;
        //     bundle.Name = window.Name.value;
        //     bundle.Platform.Clear();
        //     if (window.Win.value)
        //         bundle.Platform.Add(BuildTarget.StandaloneWindows64);
        //     // if (window.Switch.value)
        //     //     bundle.Platform.Add(BuildTarget.Switch);
        //     if (window.IOS.value)
        //         bundle.Platform.Add(BuildTarget.iOS);
        //     if (window.Android.value)
        //         bundle.Platform.Add(BuildTarget.Android);
        //     bundle.CreateDate = DateTime.Now.ToString();
        //     bundle.IsBuild = "FALSE";
        //     bundle.Group.Clear();
        //     foreach (var item in window.Group.Children())
        //     {
        //         if (item is ScrollView)
        //         {
        //             foreach (var toggle in item.Children())
        //             {
        //                 if (toggle is Toggle { value: true } t)
        //                     bundle.Group.Add(t.text);
        //             }
        //         }
        //     }
        //     if (!Directory.Exists(Path.Combine(Application.dataPath, "LZX/Bundles")))
        //         Directory.CreateDirectory(Path.Combine(Application.dataPath, "LZX/Bundles"));
        //     EditorUtility.SetDirty(bundle);
        //     AssetDatabase.SaveAssets();
        //     if (oldName != bundle.Name)
        //     {
        //         Directory.Move(Path.Combine(Application.dataPath, "LZX/Bundles/" + oldName),
        //                 Path.Combine(Application.dataPath, "LZX/Bundles/" +bundle.Name));
        //         File.Move(Path.Combine(Application.dataPath, "LZX/Bundles/" + oldName + ".asset"),
        //                     Path.Combine(Application.dataPath, "LZX/Bundles/" + bundle.Name + ".asset"));
        //         File.Delete(Path.Combine(Application.dataPath, "LZX/Bundles/" + oldName + ".meta"));
        //         //Directory.Delete(Path.Combine(Application.dataPath, "LZX/Bundles/" + oldName));
        //     }
        //     AssetDatabase.Refresh();
        //     RefreshBundle();
        //     window.Close();
        // }
        // private void DeleteBundle(Bundle bundle)
        // {
        //     Directory.Delete("Assets/LZX/Bundles/" + bundle.Name, true);
        //     File.Delete("Assets/LZX/Bundles/" + bundle.Name + ".meta");
        //     File.Delete("Assets/LZX/Bundles/" + bundle.Name + ".asset");
        //     File.Delete("Assets/LZX/Bundles/" + bundle.Name + ".asset.meta");
        //     var version = LZXEditorResources.GetVersionController();
        //     version.Remove(bundle.GUID);
        //     AssetDatabase.SaveAssets();
        //     AssetDatabase.Refresh();
        //     RefreshBundle();
        // }
        // private void AddAsset(string assetPath)
        // {
        //     var bundles = GetBundles();
        //     List<Bundle> repeted = new List<Bundle>();
        //     foreach (var bundle in bundles)
        //     {
        //         if (bundle.AssetGUIDs.Contains(AssetDatabase.AssetPathToGUID(assetPath)))
        //         {
        //             if (bundle != curSelectBundle)
        //             {
        //                 repeted.Add(bundle);
        //             }
        //         }
        //     }
        //     if (repeted.Count > 0)
        //     {
        //         MessageBoxWindow.Show($"资产已存在于:{string.Join(",", repeted.Select(b => b.Name))}中",
        //             () => {
        //                 if(!curSelectBundle.AssetGUIDs.Contains(AssetDatabase.AssetPathToGUID(assetPath)))
        //                     curSelectBundle.AssetGUIDs.Add(AssetDatabase.AssetPathToGUID(assetPath));
        //             });
        //     }
        //     else
        //     {
        //         if(!curSelectBundle.AssetGUIDs.Contains(AssetDatabase.AssetPathToGUID(assetPath)))
        //             curSelectBundle.AssetGUIDs.Add(AssetDatabase.AssetPathToGUID(assetPath));
        //     }
        //     EditorUtility.SetDirty(curSelectBundle);
        //     AssetDatabase.SaveAssets();
        //     RefreshAsset(curSelectBundle);
        // }
        // private void AddAsset(List<string> assetsPath)
        // {
        //     Dictionary<string, List<string>> repeated = new Dictionary<string, List<string>>();
        //     foreach (var path in assetsPath)
        //     {
        //         string guid = AssetDatabase.AssetPathToGUID(path);
        //         foreach (var bundle in GetBundles())
        //         {
        //             if (bundle.AssetGUIDs.Contains(guid))
        //             {
        //                 if (!repeated.ContainsKey(path))
        //                     repeated[path] = new List<string>();
        //                 repeated[path].Add(bundle.Name);
        //             }
        //         }
        //     }
        //     
        //     string warning = "";
        //     foreach (var kv in repeated)
        //     {
        //         warning += $"资产：{kv.Key} 已经存在于Bundle：{string.Join(",", kv.Value)}中\n";
        //     }
        //     if (warning != "")
        //     {
        //         MessageBoxWindow.Show(warning, () => {
        //             foreach (var path in assetsPath)
        //             {
        //                 string guid = AssetDatabase.AssetPathToGUID(path);
        //                 if (!curSelectBundle.AssetGUIDs.Contains(guid))
        //                     curSelectBundle.AssetGUIDs.Add(guid);
        //             }
        //             EditorUtility.SetDirty(curSelectBundle);
        //             AssetDatabase.SaveAssets();
        //             RefreshAsset(curSelectBundle);
        //         });
        //     }
        //     else
        //     {
        //         foreach (var path in assetsPath)
        //         {
        //             string guid = AssetDatabase.AssetPathToGUID(path);
        //             if (!curSelectBundle.AssetGUIDs.Contains(guid))
        //                 curSelectBundle.AssetGUIDs.Add(guid);
        //         }
        //         EditorUtility.SetDirty(curSelectBundle);
        //         AssetDatabase.SaveAssets();
        //         RefreshAsset(curSelectBundle);
        //     }
        // }
        // private List<Bundle> GetBundles()
        // {
        //     List<Bundle> bundles = new List<Bundle>();
        //     var version = LZXEditorResources.GetVersionController();
        //     foreach (var kv in version._bundles)
        //     {
        //         bundles.Add(kv.Value);
        //     }
        //     return bundles;
        // }
    }
}
