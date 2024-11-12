using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using LZX.MScriptableObject;
using UnityEngine;

namespace LZX.HotUpdate
{
    public class LZXHotUpdate:MonoBehaviour
    {
        private LoadingUI loadingUI;
        private VersionObject version;
        private string resourcesURL;
        private VersionObject server_version;
        private async void Start()
        {
            version = ScriptableObject.CreateInstance<VersionObject>();
            string json = File.ReadAllText(Application.persistentDataPath + "/version.json");
            JsonUtility.FromJsonOverwrite(json, version);
            resourcesURL = Path.Combine(version.setting.ResourcesURL, Application.platform.ToString());
            Loadloading();
            loadingUI.UpdateDesc("正在检查更新...");
            await GetServerVersionObj();
            if(server_version.version != version.version)
                CheckUpdate();
            EnterGame();
        }
        private void EnterGame()
        {
            throw new NotImplementedException();
        }
        private void CheckUpdate()
        {
            LZXDownLoad.DownFileList fileList = null;
            if (CheckBreakPoint())
            {
                loadingUI.UpdateDesc("正在恢复热更...");
                string json =
                    File.ReadAllText(Path.Combine(Application.persistentDataPath, "Temp", "downfilelist.json"));
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
                    if (!File.Exists(Path.Combine(Application.persistentDataPath, version.version, bundle.Name)))
                    {
                        fileInfo.fileName = bundle.Name;
                        fileInfo.url = Path.Combine(resourcesURL, bundle.Name);
                        fileInfos.Add(fileInfo);
                    }
                    else
                    {
                        string md5 = LZXDownLoad.GetMD5(Path.Combine(Application.persistentDataPath, version.version,
                            bundle.Name));
                        loadingUI.UpdateDesc($"比对MD5:[{md5}]:[{bundle.MD5}]");
                        if (md5 != bundle.MD5)
                        {
                            fileInfo.fileName = bundle.Name;
                            fileInfo.url = Path.Combine(resourcesURL, bundle.Name);
                            fileInfos.Add(fileInfo);
                        }
                    }
                }
                fileList.fileList = fileInfos;
                string json = JsonUtility.ToJson(fileList);
                File.WriteAllText(Path.Combine(Application.persistentDataPath, "Temp", "downfilelist.json"), json);
            }
            else
            {
                loadingUI.UpdateDesc("断点有效，恢复下载...");
                loadingUI.InitDownLoad(fileList.fileList.Count);
            }
            DownLoad(fileList);
        }
        private async Task GetServerVersionObj()
        {
            server_version = ScriptableObject.CreateInstance<VersionObject>();
            LZXDownLoad.DownFileInfo serverfile = new LZXDownLoad.DownFileInfo();
            serverfile.url = Path.Combine(resourcesURL, "version.json");
            loadingUI.UpdateDesc("正在向服务器请求清单文件");
            await LZXDownLoad.DownLoadFileAsync(serverfile);
            JsonUtility.FromJsonOverwrite(serverfile.fileData.text, server_version);
        }
        private async void DownLoad(LZXDownLoad.DownFileList fileList)
        {
            int count = 0;
            foreach (var fileInfo in fileList.fileList)
            {
                if (File.Exists(Path.Combine(Application.persistentDataPath, "Temp", fileInfo.fileName)))
                {
                    count++;
                    loadingUI.UpdateProgress($"下载文件:{fileInfo.fileName},数量：{count}");
                }
                else
                {
                    await LZXDownLoad.DownLoadFileAsync(fileInfo);
                    await LZXDownLoad.WriteFile(
                        Path.Combine(Application.persistentDataPath, "Temp", fileInfo.fileName),
                        fileInfo.fileData.data);
                    count++;
                    loadingUI.UpdateProgress($"下载文件:{fileInfo.fileName},数量：{count}");
                }
            }
            Directory.Delete(Path.Combine(Application.persistentDataPath, version.version), true);
            Directory.Move(Path.Combine(Application.persistentDataPath, "Temp"),
                Path.Combine(Application.persistentDataPath, server_version.version));
            File.Delete(Path.Combine(Application.persistentDataPath, "version.json"));
            File.WriteAllText(Path.Combine(Application.persistentDataPath, "version.json"),
                JsonUtility.ToJson(server_version));
            await CheckDll();
        }
        private async Task CheckDll()
        {
            if (server_version.HotUpdateDllMD5 != version.HotUpdateDllMD5)
            {
                LZXDownLoad.DownFileInfo loadingBundle = new LZXDownLoad.DownFileInfo();
                loadingBundle.url = Path.Combine(resourcesURL,"HotUpdate.dll.bytes");
                await LZXDownLoad.DownLoadFileAsync(loadingBundle);
                await LZXDownLoad.WriteFile(Path.Combine(Application.persistentDataPath, "HotUpdate.dll.bytes.temp"),
                    loadingBundle.fileData.data);
                if (server_version.ForeceReplay)
                {
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
#else
                //TODO:退出前给个提示
                    Application.Quit();
#endif
                }
            }
        }
        private bool CheckBreakPoint()
        {
            if (!Directory.Exists(Path.Combine(Application.persistentDataPath, "Temp")))
            {
                Directory.CreateDirectory(Path.Combine(Application.persistentDataPath, "Temp"));
                return false;
            }
            if (Directory.Exists(Path.Combine(Application.persistentDataPath, "Temp", "downfilelist.json")))
                return true;
            return false;
        }
        private async void Loadloading()
        {
            var bundle =
                await AssetBundle.LoadFromFileAsync(
                    Path.Combine(Application.persistentDataPath, version.setting.LoadingBundleName));
            var go = bundle.LoadAsset<GameObject>(version.setting.LoadingUIPath);
            Instantiate(go);
            bundle.Unload(false);
            var type = Assembly.GetAssembly(typeof(LoadingUI))
                .GetTypes()
                .Where(type => type.IsSubclassOf(typeof(LoadingUI)) && !type.IsAbstract)
                .ToList();
            if(type.Count > 0 && type.Count == 1)
                loadingUI = gameObject.AddComponent(type[0]) as LoadingUI;
            else
                throw new Exception("必须有且仅有一个类继承自LoadingUI");
        }
    }
}