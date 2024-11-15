using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HybridCLR.Editor;
using HybridCLR.Editor.Settings;
using LZX.MEditor.LZXStatic;
using LZX.MScriptableObject;
using UnityEngine;

namespace LZX.MEditor.MScriptableObject
{
    public class VersionController : ScriptableObject
    {
        public int[] Version = new int[5];

        public List<KeyPairValueSerializable<string, Bundle>> _bundles =
            new List<KeyPairValueSerializable<string, Bundle>>();
        private HashSet<string> keys = new HashSet<string>();
        public Dictionary<string, Bundle> Bundles
        {
            get
            {
                return _bundles.ToDictionary(x => x.Key, x => x.Value);
            }
        }
        private void OnEnable()
        {
            foreach (var kv in _bundles)
            {
                keys.Add(kv.Key);
            }
        }
        public void Add(string key, Bundle bundle)
        {
            if(keys.Contains(key))
                throw new System.ArgumentException("Key already exists in the dictionary");
            _bundles.Add(new KeyPairValueSerializable<string, Bundle> { Key = key, Value = bundle });
            keys.Add(key);
        }
        public bool Remove(string key)
        {
            if (!keys.Contains(key))
                return false;
            _bundles.RemoveAll(x => x.Key == key);
            keys.Remove(key);
            return true;
        }
        public bool ContainsKey(string key)
        {
            return keys.Contains(key);
        }
        public VersionObject GeneratorVersionObject(LZXBundleBuild.LZXBundleBuildParameter parameter)
        {
            var versionObject = CreateInstance<VersionObject>();
            LZXDllController.CopyDll2OutPut(parameter);
            string hotupdatePath = parameter.OutputPath + "/HotUpdate.dll.bytes";
            if (!File.Exists(hotupdatePath))
                throw new Exception("HotUpdate.dll.bytes not found in output path");
            versionObject.HotUpdateDllMD5 = LZXEditorResources.GetMD5(hotupdatePath);
            if(!File.Exists(Path.Combine(parameter.OutputPath,parameter.LoadingBundleName+LZXEditorResources.GetSetting().BundleEx)))
                throw new Exception("Loading bundle not found in output path");
            versionObject.LoadingMD5 = LZXEditorResources.GetMD5(Path.Combine(parameter.OutputPath,parameter.LoadingBundleName+LZXEditorResources.GetSetting().BundleEx));
            versionObject.version = string.Join(".", Version);
            versionObject.BundleEx = LZXEditorResources.GetSetting().BundleEx;
            versionObject.LoadingUIPath = parameter.LoadingUIPath;
            versionObject.LoadingBundleName = parameter.LoadingBundleName;
            versionObject.ResourcesURL = Path.Combine(LZXEditorResources.GetSetting().ResourcesURL, parameter.Target.ToString());
            versionObject.ForceReStart = parameter.ForceReStart;
            return versionObject;
        }
    }
    [System.Serializable]
    public struct KeyPairValueSerializable<TKey, TValue>
    {
        public TKey Key;
        public TValue Value;
    }
}