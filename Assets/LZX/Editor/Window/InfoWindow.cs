using LZX.MEditor.MScriptableObject;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace LZX.MEditor.Window
{
    public class InfoWindow:LZXEditorWindowBase
    {
        public ScrollView scv_info;
        public override void CreateGUI()
        {
            base.CreateGUI();
            scv_info = root.Q<ScrollView>("scv_inforoot");
        }
        public void Init(Bundle bundle)
        {
            scv_info.Clear();
            var label_name = new Label("名称："+ bundle.Name);
            scv_info.Add(label_name);
            var label_guid = new Label("GUID："+ bundle.GUID);
            scv_info.Add(label_guid);
            var label_group = new Label("分组："+ string.Join(",", bundle.Group));
            scv_info.Add(label_group);
            var label_isbuild = new Label("是否已构建："+ bundle.IsBuild);
            scv_info.Add(label_isbuild);
            var label_plat = new Label("平台：" + string.Join(",", bundle.Platform));
            scv_info.Add(label_plat);
            var label_assetcount = new Label("资源数量：" + bundle.AssetGUIDs.Count);
            scv_info.Add(label_assetcount);
        }
        public void Init(Asset asset)
        {
            scv_info.Clear();
            var label_name = new Label("名称："+ asset.Name);
            scv_info.Add(label_name);
            var label_guid = new Label("GUID："+ asset.GUID);
            scv_info.Add(label_guid);
            var label_size = new Label("大小：" + asset.Size);
            scv_info.Add(label_size);
            int index = 0;
            foreach (var dep in asset.Dependences)
            {
                index++;
                var label_dependences = new Label($"依赖{index}:{dep}");
                scv_info.Add(label_dependences);
            }
        }
    }
}