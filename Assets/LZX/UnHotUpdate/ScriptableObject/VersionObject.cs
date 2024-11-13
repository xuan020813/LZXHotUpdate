using UnityEngine;

namespace LZX.MScriptableObject
{
    [System.Serializable]
    public class VersionObject:ScriptableObject
    {
        public string version;
        public string ResourcesURL;
        public string BundleEx; 
        public string LoadingUIPath;
        public string LoadingBundleName;
        public bool ForeceReplay;
        public string HotUpdateDllMD5;
        public string LoadingMD5;
        public LZX.MScriptableObject.Bundle[] Bundles;
        public string[] MetaDataDlls;
    }
}