using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

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
        public bool ForceReStart;
        public string HotUpdateDllMD5;
        public string LoadingMD5;
        public LZX.MScriptableObject.Bundle[] Bundles;
        
    }
}