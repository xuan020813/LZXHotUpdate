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

public class LZXUnHotUpdate
{
    private readonly string hotUpdateDllName = "HotUpdate.dll.bytes";
    private readonly string tempEx = ".temp";
    private readonly string versionfileName = "Version.json";
    private readonly string metaDataBundleName = "MetaDataDlls";
    private VersionObject version;
    private LoadingUI loadingUI;
    public async UniTask UseVersionController()
    {
        if (File.Exists(Path.Combine(Application.persistentDataPath , hotUpdateDllName+tempEx)))
        {
            //非第一次进入游戏且进行过热更新
            if(File.Exists(Path.Combine(Application.persistentDataPath ,hotUpdateDllName)))
                File.Delete(Path.Combine(Application.persistentDataPath ,hotUpdateDllName));
            File.Move(Path.Combine(Application.persistentDataPath ,hotUpdateDllName+tempEx), Path.Combine(Application.persistentDataPath ,hotUpdateDllName));
        }
        if (!File.Exists(Path.Combine(Application.persistentDataPath ,versionfileName)))
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
        await LoadDll();
    }
    private async UniTask LoadDll()
    {
        await LoadMetadataForAOTAssemblies();//HybirdCLR框架下的补充元数据方法，用于在热更新代码中使用AOT泛型
                                        // Editor环境下，HotUpdate.dll.bytes已经被自动加载，不需要加载，重复加载反而会出问题。
#if !UNITY_EDITOR
        //非编辑器下加载程序集
        Assembly hotUpdateAss = Assembly.Load(File.ReadAllBytes(Path.Combine(Application.persistentDataPath,hotUpdateDllName)));
#else
        Assembly hotUpdateAss = System.AppDomain.CurrentDomain.
            GetAssemblies().
            First(a => a.GetName().Name == Path.GetFileNameWithoutExtension(hotUpdateDllName));
#endif
        GameObject.Destroy(loadingUI.gameObject);
        Type type = hotUpdateAss.GetType("LZX.HotUpdate.Hello");
        var mathod = type.GetMethod("Run");
        UniTask task = (UniTask)mathod.Invoke(null, null);
        await task;
    }
    private async UniTask LoadMetadataForAOTAssemblies()
    {
        loadingUI.UpdateDesc("正在加载程序集文件");
        string url = Path.Combine(Application.persistentDataPath,version.version ,metaDataBundleName+version.BundleEx);
        AssetBundleCreateRequest req = AssetBundle.LoadFromFileAsync(url);
        await req;
        AssetBundle bundle = req.assetBundle;
        var assets = version.Bundles.FirstOrDefault(b => b.Name == metaDataBundleName).Assets;
        if (assets == null)
            throw new Exception("没有找到MetaDataDlls资源");
        int count = 0;
        loadingUI.InitDownLoad(assets.Length);
        foreach (var aotDllName in assets)
        {
            count++;
            var asset = bundle.LoadAssetAsync(aotDllName.LoadPath);
            loadingUI.UpdateProgress($"加载{Path.GetFileName(aotDllName.LoadPath)}元数据:{count}/{assets.Length}",count);
            await asset;
            TextAsset textAsset = asset.asset as TextAsset;
            if (textAsset == null)
                throw new Exception($"资源{aotDllName}不是TextAsset");
            int err = (int)RuntimeApi.LoadMetadataForAOTAssembly(textAsset.bytes, HomologousImageMode.SuperSet);//补充元数据方法
        }
        bundle.Unload(false);
    }
    private async UniTask LoadVersion(bool IsFirstIn)
    {
        if (IsFirstIn)
        {
            LZXDownLoad.DownFileInfo versionFile = new LZXDownLoad.DownFileInfo();
            versionFile.url = Path.Combine(Application.streamingAssetsPath, versionfileName);
            await LZXDownLoad.DownLoadFileAsync(versionFile, 0);
            version = ScriptableObject.CreateInstance<VersionObject>();
            JsonUtility.FromJsonOverwrite(versionFile.fileData.text, version);
        }
        else
        {
            version = ScriptableObject.CreateInstance<VersionObject>();
            JsonUtility.FromJsonOverwrite(
                File.ReadAllText(Path.Combine(Application.persistentDataPath,versionfileName))
                , version);
        }
        await Loadloading();
    }
    private async Task Loadloading()
    {
        var loading = version.LoadingBundleName+version.BundleEx;
        LZXDownLoad.DownFileInfo loadingFile = new LZXDownLoad.DownFileInfo();
        loadingFile.url = Path.Combine(Application.streamingAssetsPath, loading);
        await LZXDownLoad.DownLoadFileAsync(loadingFile, 0);
        var bundle = AssetBundle.LoadFromMemory(loadingFile.fileData.data);
        var ui = bundle.LoadAsset(version.LoadingUIPath);
        GameObject loadingGo = (GameObject)GameObject.Instantiate(ui);
        //AddComponent
        var type = Assembly.GetAssembly(typeof(LoadingUI))
            .GetTypes()
            .Where(type => type.IsSubclassOf(typeof(LoadingUI)) && !type.IsAbstract)
            .ToList();
        if(type.Count > 0 && type.Count == 1)
            loadingUI = loadingGo.AddComponent(type[0]) as LoadingUI;
        else
            throw new Exception("必须有且仅有一个类继承自LoadingUI");
        bundle.Unload(false);
    }
    private async UniTask ReleaseResources()
    {
        int count = 1;
        loadingUI.InitDownLoad(version.Bundles.Length+1);
        await ReleaseHotUpdateDll();
        foreach (var bundle in version.Bundles)
        {
            count++;
            LZXDownLoad.DownFileInfo bundleFile = new LZXDownLoad.DownFileInfo();
            bundleFile.url = Path.Combine(Application.streamingAssetsPath, bundle.Name+version.BundleEx);
            loadingUI.UpdateProgress($"数量:{count}/{version.Bundles.Length}",count);
            while (true)//循环重试，直到成功下载
            {
                try
                {
                    await LZXDownLoad.DownLoadFileAsync(bundleFile, 0);
                    await LZXDownLoad.WriteFile(
                        Path.Combine(Application.persistentDataPath,
                            version.version,
                            bundle.Name + version.BundleEx), 
                        bundleFile.fileData.data);
                    break;
                }
                catch (Exception e)
                {
                    var retryCompletionSource = new UniTaskCompletionSource();
                    loadingUI.ShowRetryButton($"释放文件{bundle.Name+version.BundleEx}失败\\r\\n,错误信息:{e.Message}","重试",async () =>
                    {
                        retryCompletionSource.TrySetResult();
                    });
                    await retryCompletionSource.Task;
                }
            }
        }
        loadingUI.UpdateDesc("资源释放完毕");
        await File.WriteAllTextAsync(Path.Combine(Application.persistentDataPath,versionfileName), JsonUtility.ToJson(version));
    }
    private async Task ReleaseHotUpdateDll()
    {
        LZXDownLoad.DownFileInfo dll = new LZXDownLoad.DownFileInfo();
        dll.url = Path.Combine(Application.streamingAssetsPath,hotUpdateDllName);
        loadingUI.UpdateProgress($"数量:{1}/{version.Bundles.Length}",1);
        while (true)
        {
            try
            {
                await LZXDownLoad.DownLoadFileAsync(dll);
                await LZXDownLoad.WriteFile(Path.Combine(Application.persistentDataPath,hotUpdateDllName), dll.fileData.data);
                break;
            }
            catch (Exception e)
            {
                var retryCompletionSource = new UniTaskCompletionSource();
                loadingUI.ShowRetryButton($"释放热更新程序集失败\r\n,错误信息:{e.Message}","重试",async () =>
                {
                    retryCompletionSource.TrySetResult();
                });
                await retryCompletionSource.Task;
            }
        }
    }
}
