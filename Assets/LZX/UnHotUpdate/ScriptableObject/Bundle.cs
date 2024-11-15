using System;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LZX.MScriptableObject
{
    [System.Serializable]
    public class Bundle
    {
        public string Name;
        public string MD5;
        public Asset[] Assets;
    }
}