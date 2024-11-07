using UnityEngine;

namespace LZX.MScriptableObject
{
    public class Setting: ScriptableObject
    {
        public string ResourcesURL;
        public string BundleOutPath;
        public string BundleEx;
        public string LoadingBundleName;
        public string LoadingBundlePath;
        public string HotUpdateDllName;
    }
}