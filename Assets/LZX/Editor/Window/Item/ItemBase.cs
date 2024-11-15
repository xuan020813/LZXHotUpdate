using LZX.MEditor.LZXStatic;
using UnityEngine.UIElements;

namespace LZX.MEditor.Window.Item
{
    public class ItemBase
    {
        public VisualTreeAsset uxml;
        public StyleSheet uss;
        public VisualElement root;
        public ItemBase()
        {
            uxml = LZXEditorResources.GetItemUXML();
            uss = LZXEditorResources.GetItemUSS();
            root = uxml.CloneTree();
            root.styleSheets.Add(uss);
        }
    }
}