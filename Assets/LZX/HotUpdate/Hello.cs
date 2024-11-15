using Cysharp.Threading.Tasks;
using UnityEngine;

namespace LZX.HotUpdate
{
    public static class Hello
    {
        public static async UniTask Run()
        {
            LZXHotUpdate lzx = new LZXHotUpdate();
            await lzx.StartCheckUpdate();
        }
    }
}