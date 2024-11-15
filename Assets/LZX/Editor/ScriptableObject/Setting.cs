using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Serialization;

namespace LZX.MEditor.MScriptableObject
{
    public class Setting: ScriptableObject
    {
        public string ResourcesURL;
        public string BundleEx;
        public List<AssemblyDefinitionAsset> asmdefs;
    }
}