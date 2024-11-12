using UnityEngine;

namespace LZX.MScriptableObject
{
    [System.Serializable]
    public class Asset
    {
        public string LoadPath;
        public string MD5;
        public string[] Dependencies;
    }
}