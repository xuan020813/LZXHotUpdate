using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    }
    [System.Serializable]
    public struct KeyPairValueSerializable<TKey, TValue>
    {
        public TKey Key;
        public TValue Value;
    }
}