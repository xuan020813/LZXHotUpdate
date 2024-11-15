using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace LZX.MScriptableObject
{
    public class LZXDownLoad
    {
        [Serializable]
        public class DownFileInfo
        {
            public string url;
            public string fileName;
            [NonSerialized]
            public string MD5;
            [NonSerialized]
            public DownloadHandler fileData;
        }
        [Serializable]
        public class DownFileList
        {
            public string version;
            public List<DownFileInfo> fileList;
        }
        /// <summary>
        /// 下载单个文件
        /// </summary>
        /// <param name="info"></param>
        /// <param name="retryCount"></param>
        /// <exception cref="Exception"></exception>
        public static async UniTask DownLoadFileAsync(DownFileInfo info,int retryCount = 0)
        {
            UnityWebRequest webRequest = UnityWebRequest.Get(info.url);
            while (retryCount < 5)
            {
                await webRequest.SendWebRequest();
                await UniTask.Delay(1000); //TODO:此处延时能省略
                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    info.fileData = webRequest.downloadHandler;
                    return;
                }
            }
            throw new Exception("下载文件出错：" + info.url);
        }
        /// <summary>
        /// 写入文件
        /// </summary>
        /// <param name="path"></param>
        /// <param name="data"></param>
        public static async UniTask WriteFile(string path, byte[] data)
        {
            //获取标准路径
            path = LZXDownLoad.GetStandarPath(path);
            //文件夹的路径
            string dir = path.Substring(0, path.LastIndexOf("/"));
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            FileInfo file = new FileInfo(path);
            if (file.Exists)
            {
                file.Delete();
            }
            try
            {
                using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write))
                {
                    await fs.WriteAsync(data, 0, data.Length);
                    fs.Close();
                }
            }
            catch (IOException e)
            {
                Debug.LogError(e.Message);
            }
        }
        public static string GetUnityPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return string.Empty;
            }
            return path.Substring(path.IndexOf("Assets"));
        }
        /// <summary>
        /// 获取标准路径
        /// </summary>
        /// <param name="path">路径</param>
        /// <returns>路径</returns>
        public static string GetStandarPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return string.Empty;
            return path.Trim().Replace("\\", "/");
        }

        /// <summary>
        /// 通过路径下载资源
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static async UniTask<byte[]> GetFileBytes(string path)
        {
            UnityWebRequest webRequest = UnityWebRequest.Get(path);
            await webRequest.SendWebRequest();
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                return webRequest.downloadHandler.data;
            }
            else
                throw new Exception("获取文件出错：" + path);
        }
        public static string GetMD5(string path)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] hash = md5.ComputeHash(File.ReadAllBytes(path));
                return BitConverter.ToString(hash).Replace("-", "").ToLower();;
            }
        }
    }
}