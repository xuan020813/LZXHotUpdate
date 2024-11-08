using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace LZX.MEditor.MScriptableObject
{
    public class BuildOptions : ScriptableObject
    {
        public List<BuildTarget> buildTargets = new List<BuildTarget>();
        public bool FollowBundle;
        public string outputPath;
        public bool ClearFloder;
        public bool CopyTOStreamingAssets;
        public bool ExcludeTypeInformation;
        public bool ForceRebuild;
        public bool IgnoreTypeTreeChanges;
        public bool AppendHash;
        public bool StrictMode;
        public bool DryRunBuild;
        public bool UseVersionControl = true;
        [SerializeField]
        private int compression;
        public int Compression
        {
            get => compression;
            set
            {
                compression = value;
                switch (value)
                {
                    case 0:
                        compressionType = BuildAssetBundleOptions.None;
                        break;
                    case 1:
                        compressionType = BuildAssetBundleOptions.ChunkBasedCompression;
                        break;
                    case 2:
                        compressionType = BuildAssetBundleOptions.UncompressedAssetBundle;
                        break;
                }
            }
        }
        public BuildAssetBundleOptions compressionType;
    }
}


