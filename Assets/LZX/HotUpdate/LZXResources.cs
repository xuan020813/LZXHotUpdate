using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
using LZX.MScriptableObject;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LZX.HotUpdate
{
    public class LZXResources
    {
        public class BundleData
        {
            public int Count;
            public AssetBundle Bundle;
            public BundleData(AssetBundle bundle)
            {
                Bundle = bundle;
            }
            public async UniTask<T> LoadAssetAsync<T>(string assetName) where T : UnityEngine.Object
            {
                Count++;
                var bundleReq = Bundle.LoadAssetAsync(assetName);
                await bundleReq;
                var asset = bundleReq.asset as T;
                asset.name = assetName;
                return asset;
            }
            public async UniTask UnLoadAssetAsync(UnityEngine.Object obj)
            {
                Count--;
                UnityEngine.Object.Destroy(obj);
                if (Count == 0)
                { 
                    Debug.Log("Unload bundle " + Bundle.name);
                    await Bundle.UnloadAsync(true);
                    Object.Destroy(Bundle);
                    Instance.Bundles.Remove(Bundle.name);
                    Bundle = null;
                }
            }
        }
        private static LZXResources _instance;
        public static LZXResources Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new LZXResources();
                return _instance;
            }
        }
        public VersionObject Version;
        public static void ParseVersionObject(VersionObject version)
        {
            Instance.Parse(version);
        }
        private readonly Dictionary<string,Bundle> Assets = new Dictionary<string, Bundle>();
        private readonly Dictionary<string, BundleData> Bundles = new Dictionary<string, BundleData>();
        private void Parse(VersionObject version)
        {
            Version = version;
            foreach (var bundle in version.Bundles)
            {
                foreach (var asset in bundle.Assets)
                {
                    Assets.Add(Path.GetFileNameWithoutExtension(asset.LoadPath), bundle);
                }
            }
        }
        public static async UniTask<T> LoadAssetAsync<T>(string assetName) where T : UnityEngine.Object
        {
            if (Instance.Version == null)
                throw new Exception("Please call LZXResources.ParseVersionObject first");
            if (Instance.Assets.TryGetValue(assetName,out var bundle))
            {
                if(!Instance.Bundles.TryGetValue(bundle.Name,out var BundleData))
                {
                    string path = Path.Combine(Application.persistentDataPath,
                        Instance.Version.version, 
                        bundle.Name + Instance.Version.BundleEx);
                    var req = AssetBundle.LoadFromFileAsync(path);
                    await req;
                    req.assetBundle.name = bundle.Name;
                    BundleData = new BundleData(req.assetBundle);
                    Instance.Bundles.Add(bundle.Name, BundleData);
                }
                return await BundleData.LoadAssetAsync<T>(assetName);
            }
            return null;
        }
        public static async UniTask UnLoadAssetAsync(UnityEngine.Object obj)
        {
            if (Instance.Version == null)
                throw new Exception("Please call LZXResources.ParseVersionObject first");
            if(Instance.Bundles.TryGetValue(obj.name,out var bundleData))
                await bundleData.UnLoadAssetAsync(obj);
        }
        public static async UniTask<Assembly> LoadDllAsync(string dllName)
        {
            if (Instance.Version == null)
                throw new Exception("Please call LZXResources.ParseVersionObject first");
            if (Instance.Assets.TryGetValue(dllName + ".dll.bytes", out var bundle))
            {
                string path = Path.Combine(Application.persistentDataPath,
                    Instance.Version.version, bundle.Name);
                if (!File.Exists(path))
                    throw new Exception($"Dll:{path} not found");
                var bytes = await File.ReadAllBytesAsync(path);
#if !UNITY_EDITOR
                //非编辑器下加载程序集
                Assembly assembly = Assembly.Load(bytes);
#else
                Assembly assembly = System.AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name == "dllName");
#endif
                return assembly;
            }
            Debug.LogError($"Dll:{dllName} not found in bundle");
            return null;
        }
    }
}