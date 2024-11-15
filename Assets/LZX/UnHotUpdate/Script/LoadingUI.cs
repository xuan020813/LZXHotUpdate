using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace LZX.MScriptableObject
{
    public abstract class LoadingUI:MonoBehaviour
    {
        public abstract void UpdateDesc(string desc);
        public abstract void UpdateProgress(string desc,int count);
        public abstract void RefreshUI();
        public abstract void ShowRetryButton(Action retryAction);
        public abstract void ShowRetryButton(Action<object[]> retryAction);
        public abstract void InitDownLoad(int allfileCount);
    }
}