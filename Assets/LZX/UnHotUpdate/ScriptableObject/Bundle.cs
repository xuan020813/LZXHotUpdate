using System;
using UnityEngine;

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