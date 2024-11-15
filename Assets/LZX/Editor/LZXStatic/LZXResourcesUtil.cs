namespace LZX.MEditor.LZXStatic
{
    public class LZXResourcesUtil
    {
        public static string ScriptableObjectPath = "Assets/LZX/Editor/ScriptableObject/";
        public static string GetBuildOptionsPath()
        {
            return ScriptableObjectPath + "BuildOptions.asset";
        }
        public static string GetBundleGroupPath()
        {
            return ScriptableObjectPath + "BundleGroup.asset";
        }
        public static string GetSettingsPath()
        {
            return ScriptableObjectPath + "Setting.asset";
        }
        public static string GetVersionControllerPath()
        {
            return ScriptableObjectPath + "VersionController.asset";
        }
        public static string GetBundlesPath()
        {
            return "Assets/LZX/Bundles/";
        }
        public static string GetItemUxmlPath()
        {
            return "Assets/LZX/Editor/UXML/LZXInfoItem.uxml";
        }
        public static string GetItemUssPath()
        {
            return "Assets/LZX/Editor/USS/LZXInfoItem.uss";
        }

        public static string GetHotUpdateAssembleDefinePath()
        {
            return "Assets/LZX/HotUpdate/HotUpdate.asmdef";
        }
    }
}