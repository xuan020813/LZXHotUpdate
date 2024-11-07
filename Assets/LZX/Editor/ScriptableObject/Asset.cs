using UnityEngine;

namespace LZX.MEditor.MScriptableObject
{
    public class Asset:ScriptableObject
    {
        public string Name;
        public string GUID;
        public string LoadPath;
        public string[] Dependences;
        public string Size;
    }
}