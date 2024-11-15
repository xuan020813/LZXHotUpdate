using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using LZX.MScriptableObject;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LZX.HotUpdate
{
    public class LZXHotUpdate
    {
        private readonly string versionfileName = "Version.json";
        private readonly string downfileListName = "downfilelist.json";
        private readonly string hotUpdateDllName = "HotUpdate.dll.bytes";
        private readonly string tempDir = "Temp";
        private readonly string tempEx = ".temp";
        private LoadingUI loadingUI;
        private VersionObject version;
        private VersionObject server_version;
        public async UniTask StartCheckUpdate()
        {
            version = ScriptableObject.CreateInstance<VersionObject>();
            string json = File.ReadAllText(Path.Combine(Application.persistentDataPath, versionfileName));
            JsonUtility.FromJsonOverwrite(json, version);
            await Loadloading();
            loadingUI.UpdateDesc("正在检查更新...");
            await GetServerVersionObj();
            if(server_version.version != version.version)
                await CheckUpdate();
            else
                loadingUI.UpdateDesc("无需更新，进入游戏...");
        }
        private async UniTask EnterGame()
        {
            //TODO:进入游戏
            loadingUI.UpdateDesc("进入游戏...");
            LZXResources.ParseVersionObject(version);
            // var scene = await LZXResources.LoadAssetAsync<SceneAsset>("testscene");
            // var asyncOperation = SceneManager.LoadSceneAsync(scene.name, LoadSceneMode.Single);
            // asyncOperation.allowSceneActivation = true;
            var ass = await LZXResources.LoadDllAsync("DemoAssembly");
            var type = ass.GetType("Test");
            type.GetMethod("Start").Invoke(null, null);
        }
        private async UniTask CheckUpdate()
        {
            LZXDownLoad.DownFileList fileList = null;
            if (CheckBreakPoint())
            {
                loadingUI.UpdateDesc("正在恢复热更...");
                string json =
                    File.ReadAllText(Path.Combine(Application.persistentDataPath, tempDir,downfileListName ));
                fileList = JsonUtility.FromJson<LZXDownLoad.DownFileList>(json);
            }
            if (fileList == null || fileList.version != server_version.version)
            {
                //计算下载文件
                loadingUI.UpdateDesc("正在计算下载文件...");
                fileList = new LZXDownLoad.DownFileList();
                fileList.version = server_version.version;
                List<LZXDownLoad.DownFileInfo> fileInfos = new List<LZXDownLoad.DownFileInfo>();
                foreach (var bundle in server_version.Bundles)
                {
                    LZXDownLoad.DownFileInfo fileInfo = new LZXDownLoad.DownFileInfo();
                    string bundlePath = "";
                    string bundleUrl = "";
                    if (bundle.Name.EndsWith(".bytes"))
                    {
                        bundlePath = Path.Combine(server_version.ResourcesURL, version.version, bundle.Name);
                        bundleUrl = Path.Combine(server_version.ResourcesURL, bundle.Name);
                    }
                    else
                    {
                        bundlePath = Path.Combine(Application.persistentDataPath, version.version,
                            bundle.Name + version.BundleEx);
                        bundleUrl = Path.Combine(server_version.ResourcesURL, bundle.Name+server_version.BundleEx);
                    }
                    if (!File.Exists(bundlePath))
                    {
                        fileInfo.fileName = bundle.Name;
                        fileInfo.url = bundleUrl;
                        fileInfos.Add(fileInfo);
                    }
                    else
                    {
                        string md5 = LZXDownLoad.GetMD5(bundlePath);
                        loadingUI.UpdateDesc($"比对MD5:[{md5}]:[{bundle.MD5}]");
                        if (md5 != bundle.MD5)
                        {
                            fileInfo.fileName = bundle.Name;
                            fileInfo.url = bundleUrl;
                            fileInfos.Add(fileInfo);
                        }
                    }
                }
                fileList.fileList = fileInfos;
                string json = JsonUtility.ToJson(fileList);
                File.WriteAllText(Path.Combine(Application.persistentDataPath, tempDir, downfileListName), json);
            }
            else
            {
                loadingUI.UpdateDesc("断点有效，恢复下载...");
                loadingUI.InitDownLoad(fileList.fileList.Count);
            }
            await DownLoad(fileList);
        }
        private async UniTask GetServerVersionObj()
        {
            server_version = ScriptableObject.CreateInstance<VersionObject>();
            LZXDownLoad.DownFileInfo serverfile = new LZXDownLoad.DownFileInfo();
            serverfile.url = Path.Combine(version.ResourcesURL, versionfileName);
            loadingUI.UpdateDesc("正在向服务器请求清单文件");
            await LZXDownLoad.DownLoadFileAsync(serverfile);
            JsonUtility.FromJsonOverwrite(serverfile.fileData.text, server_version);
        }
        private async UniTask DownLoad(LZXDownLoad.DownFileList fileList)
        {
            int count = 0;
            loadingUI.InitDownLoad(fileList.fileList.Count);
            foreach (var fileInfo in fileList.fileList)
            {
                string filePath = "";
                if (!fileInfo.fileName.EndsWith(".bytes"))
                    filePath = Path.Combine(Application.persistentDataPath, tempDir,
                        fileInfo.fileName + server_version.BundleEx);
                else
                    filePath = Path.Combine(Application.persistentDataPath, tempDir, fileInfo.fileName);
                if (File.Exists(filePath))
                {
                    count++;
                    loadingUI.UpdateProgress($"下载文件:{fileInfo.fileName},数量：{count}",count);
                }
                else
                {
                    await LZXDownLoad.DownLoadFileAsync(fileInfo);
                    await LZXDownLoad.WriteFile(filePath, fileInfo.fileData.data);
                    count++;
                    loadingUI.UpdateProgress($"下载文件:{fileInfo.fileName},数量：{count}",count);
                }
            }
            Directory.Delete(Path.Combine(Application.persistentDataPath, version.version), true);
            Directory.Move(Path.Combine(Application.persistentDataPath, tempDir),
                Path.Combine(Application.persistentDataPath, server_version.version));
            File.Delete(Path.Combine(Application.persistentDataPath, versionfileName));
            File.WriteAllText(Path.Combine(Application.persistentDataPath, versionfileName),
                JsonUtility.ToJson(server_version));
            await CheckDll();
        }
        private async UniTask CheckDll()
        {
            if (server_version.HotUpdateDllMD5 != version.HotUpdateDllMD5)
            {
                LZXDownLoad.DownFileInfo hotupdatedll = new LZXDownLoad.DownFileInfo();
                hotupdatedll.url = Path.Combine(version.ResourcesURL,hotUpdateDllName);
                await LZXDownLoad.DownLoadFileAsync(hotupdatedll);
                await LZXDownLoad.WriteFile(Path.Combine(Application.persistentDataPath,hotUpdateDllName+tempEx ),
                    hotupdatedll.fileData.data);
                if (server_version.ForceReStart)
                {
#if !UNITY_EDITOR
                    Debug.LogError("下载完成，重启客户端");
                    UnityEditor.EditorApplication.isPlaying = false;
#else
                    loadingUI.ShowRetryButton("下载完成，请重启客户端","重启",Application.Quit);
#endif
                }
            }
        }
        private bool CheckBreakPoint()
        {
            if (!Directory.Exists(Path.Combine(Application.persistentDataPath, tempDir)))
            {
                Directory.CreateDirectory(Path.Combine(Application.persistentDataPath, tempDir));
                return false;
            }
            if (Directory.Exists(Path.Combine(Application.persistentDataPath, tempDir, downfileListName)))
                return true;
            return false;
        }
        private async UniTask Loadloading()
        {
            var bundle =
                await AssetBundle.LoadFromFileAsync(
                    Path.Combine(Application.persistentDataPath,version.version ,version.LoadingBundleName+version.BundleEx));
            var go = bundle.LoadAsset<GameObject>(version.LoadingUIPath);
            var loadinggo = GameObject.Instantiate(go);
            bundle.Unload(false);
            var type = Assembly.GetAssembly(typeof(LoadingUI))
                .GetTypes()
                .Where(type => type.IsSubclassOf(typeof(LoadingUI)) && !type.IsAbstract)
                .ToList();
            if(type.Count > 0 && type.Count == 1)
                loadingUI = loadinggo.AddComponent(type[0]) as LoadingUI;
            else
                throw new Exception("必须有且仅有一个类继承自LoadingUI");
        }
    }
}