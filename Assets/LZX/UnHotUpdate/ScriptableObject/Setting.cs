using UnityEngine;
using UnityEngine.Serialization;

namespace LZX.MScriptableObject
{
    public class Setting: ScriptableObject
    {
        public string ResourcesURL;
        public string BundleEx; 
        public string LoadingUIPath;
        public string LoadingBundleName;
    }
}