using System.IO;
using LZX.MEditor.LZXStatic;
using LZX.MEditor.MScriptableObject;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace LZX.MEditor.Window.Item
{
    public class AssetItem:ItemBase
    {
        public Label Name;
        public bool IsName = true;
        public Asset Asset;
        public VisualElement Root;
        private VisualElement BG;
        public AssetItem(Asset asset,VisualElement root)
        {
            Name = root.Q<Label>("label_name");
            Name.text = asset.name;
            var btn_delete = root.Q<Button>("btn_expansion");
            btn_delete.clicked += () => { OnDeleteClick();};
            btn_delete.style.backgroundImage = LZXEditorResources.GetIcon(LZXIconType.delete);
            var btn_trans = root.Q<Button>("btn_remove");
            btn_trans.clicked += () => { OnTransClick(); };
            btn_trans.style.backgroundImage = LZXEditorResources.GetIcon(LZXIconType.trans);
            BG = root.Q<VisualElement>("BG");
            Root = root;
            Asset = asset;
            var label_name = root.Q<Label>("label_name");
            label_name.RegisterCallback<ClickEvent>(OnClick);
            ParseBG();
        }
        private void OnClick(ClickEvent evt)
        {
            BundleInfoWindow window = EditorWindow.GetWindow<BundleInfoWindow>();
            window.RefreshLabel(Asset);
            //TODO:高亮资产
        }
        private void OnDeleteClick()
        {
            throw new System.NotImplementedException();
        }
        private void OnTransClick()
        {
            if (IsName)
            { 
                Name.text = Asset.LoadPath;   
            }
            else
            {
                Name.text = Asset.Name;
            }
            IsName =!IsName;
        }
        private void ParseBG()
        {
            string ext = Path.GetExtension(Asset.LoadPath);
            switch (ext)
            {
                case ".prefab":
                    BG.style.backgroundImage = LZXEditorResources.GetIcon(LZXIconType.prefab);
                    break;
                case ".mp3":
                case ".wav":
                    BG.style.backgroundImage = LZXEditorResources.GetIcon(LZXIconType.music);
                    break;
                case ".png":
                case ".jpg":
                    BG.style.backgroundImage = AssetDatabase.LoadAssetAtPath<Texture2D>(Asset.LoadPath);
                    break;
                case ".asset":
                case ".json":
                case ".txt":
                case ".bytes":
                case ".cs":
                    BG.style.backgroundImage = LZXEditorResources.GetIcon(LZXIconType.cs);
                    break;
                case ".unity":
                    BG.style.backgroundImage = LZXEditorResources.GetIcon(LZXIconType.scene);
                    break;
            }
        }
    }
}