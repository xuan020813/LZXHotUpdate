using UnityEngine;

namespace LZX.HotUpdate
{
    public static class Hello
    {
        public static void Run()
        {
            GameObject go = new GameObject("HotUpdate");
            go.AddComponent<LZXHotUpdate>();
        }
    }
}