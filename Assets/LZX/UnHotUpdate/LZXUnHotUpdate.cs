using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using HybridCLR;
using LZX.MScriptableObject;
using UnityEngine;

public class LZXUnHotUpdate : MonoBehaviour
{
    private VersionObject version;
    private LoadingUI loadingUI;
    private async void Start()
    {
        if (File.Exists(Application.persistentDataPath + "/HotUpdate.dll.bytes.temp"))
        {
            //非第一次进入游戏且进行过热更新
            if(File.Exists(Application.persistentDataPath + "/HotUpdate.dll.bytes"))
                File.Delete(Application.persistentDataPath + "/HotUpdate.dll.bytes");
            File.Move(Application.persistentDataPath + "/HotUpdate.dll.bytes.temp", Application.persistentDataPath + "/HotUpdate.dll.bytes");
        }
        if (!File.Exists(Application.persistentDataPath + "/version.json"))
        {
            await LoadVersion(true);
            loadingUI.UpdateDesc("正在释放资源,此过程不消耗流量");
            await ReleaseResources();
            loadingUI.UpdateDesc("资源释放完毕,正在加载程序集");
        }
        else
        {
            await LoadVersion(false);
            loadingUI.UpdateDesc("正在加载程序集");
        }
        LoadDll();
    }

    private void LoadDll()
    {
        LoadMetadataForAOTAssemblies();//HybirdCLR框架下的补充元数据方法，用于在热更新代码中使用AOT泛型
                                        // Editor环境下，HotUpdate.dll.bytes已经被自动加载，不需要加载，重复加载反而会出问题。
#if !UNITY_EDITOR
        //非编辑器下加载程序集
        Assembly hotUpdateAss = Assembly.Load(File.ReadAllBytes(Path.Combine(Application.persistentDataPath,"HotUpdate.dll.bytes")));
#else
        Assembly hotUpdateAss = System.AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name == "HotUpdate");
#endif
        GameObject.Destroy(loadingUI.gameObject);
        Type type = hotUpdateAss.GetType("Hello");
        type.GetMethod("Run").Invoke(null, null);
    }
    private async void LoadMetadataForAOTAssemblies()
    {
        List<string> files = new List<string>();
#if UNITY_ANDROID
        try
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                using (AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    using (AndroidJavaObject assets = activity.Call<AndroidJavaObject>("getAssets"))
                    {
                        // 列出 StreamingAssets 目录下的所有文件
                        string[] fileNames = assets.Call<string[]>("list", "AssembliesPostIl2CppStrip"); // "" 表示根目录
                        files.AddRange(fileNames);
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to get files from StreamingAssets: " + e.Message);
        }
#else
        files.AddRange(Directory.GetFiles(Application.streamingAssetsPath, "AssembliesPostIl2CppStrip", SearchOption.AllDirectories));
#endif
        foreach (var aotDllName in files)
        {
            byte[] dllBytes = await LZXDownLoad.GetFileBytes(aotDllName);
            int err = (int)RuntimeApi.LoadMetadataForAOTAssembly(dllBytes, HomologousImageMode.SuperSet);//补充元数据方法
        }
    }
    private async UniTask LoadVersion(bool IsFirstIn)
    {
        if (IsFirstIn)
        {
            LZXDownLoad.DownFileInfo versionFile = new LZXDownLoad.DownFileInfo();
            versionFile.url = Path.Combine(Application.streamingAssetsPath, "version.json");
            await LZXDownLoad.DownLoadFileAsync(versionFile, 0);
            version = ScriptableObject.CreateInstance<VersionObject>();
            JsonUtility.FromJsonOverwrite(versionFile.fileData.text, version);
        }
        else
        {
            version = ScriptableObject.CreateInstance<VersionObject>();
            JsonUtility.FromJsonOverwrite(
                File.ReadAllText(Path.Combine(Application.persistentDataPath, "version.json"))
                , version);
        }
        await Loadloading();
    }

    private async Task Loadloading()
    {
        var loading = version.setting.LoadingBundleName;
        LZXDownLoad.DownFileInfo loadingFile = new LZXDownLoad.DownFileInfo();
        loadingFile.url = Path.Combine(Application.streamingAssetsPath, loading);
        await LZXDownLoad.DownLoadFileAsync(loadingFile, 0);
        var bundle = AssetBundle.LoadFromMemory(loadingFile.fileData.data);
        var ui = bundle.LoadAsset(version.setting.LoadingUIPath);
        GameObject.Instantiate(ui);
        //AddComponent
        var type = Assembly.GetAssembly(typeof(LoadingUI))
            .GetTypes()
            .Where(type => type.IsSubclassOf(typeof(LoadingUI)) && !type.IsAbstract)
            .ToList();
        if(type.Count > 0 && type.Count == 1)
            loadingUI = gameObject.AddComponent(type[0]) as LoadingUI;
        else
            throw new Exception("必须有且仅有一个类继承自LoadingUI");
        bundle.Unload(false);
    }

    private async UniTask ReleaseResources()
    {
        int count = 0;
        foreach (var bundle in version.Bundles)
        {
            count++;
            LZXDownLoad.DownFileInfo bundleFile = new LZXDownLoad.DownFileInfo();
            bundleFile.url = Path.Combine(Application.streamingAssetsPath, bundle.Name+version.setting.BundleEx);
            loadingUI.UpdateProgress($"数量:{count}/{version.Bundles.Length}");
            while (true)//循环重试，直到成功下载
            {
                try
                {
                    await LZXDownLoad.DownLoadFileAsync(bundleFile, 0);
                    await LZXDownLoad.WriteFile(
                        Path.Combine(Application.persistentDataPath,
                            version.version,
                            bundle.Name + version.setting.BundleEx), 
                        bundleFile.fileData.data);
                    break;
                }
                catch (Exception e)
                {
                    var retryCompletionSource = new UniTaskCompletionSource();
                    loadingUI.UpdateDesc($"下载文件{bundle.Name+version.setting.BundleEx}失败\r\n,错误信息:{e.Message}");
                    loadingUI.ShowRetryButton(async () =>
                    {
                        retryCompletionSource.TrySetResult();
                    });
                    await retryCompletionSource.Task;
                }
            }
        }
        loadingUI.UpdateDesc("资源释放完毕");
        await File.WriteAllTextAsync(Path.Combine(Application.persistentDataPath, "version.json"), JsonUtility.ToJson(version));
    }
}
