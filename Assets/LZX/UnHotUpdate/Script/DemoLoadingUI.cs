using System;
using System.Collections;
using System.Collections.Generic;
using LZX.MScriptableObject;
using UnityEngine;
using UnityEngine.UI;

public class DemoLoadingUI : LoadingUI
{
    public Text desc;
    public Text progressdesc;
    public Slider progressBar;
    public GameObject errorUI;
    public Button retryBtn;
    public float allfileCount;

    private void Awake()
    {
        desc = transform.Find("Desc").GetComponent<Text>();
        progressdesc = transform.Find("ProgressDesc").GetComponent<Text>();
        progressBar = transform.Find("ProgressBar").GetComponent<Slider>();
        errorUI = transform.Find("ErrorUI").gameObject;
        errorUI.SetActive(false);
        retryBtn = errorUI.transform.Find("RetryBtn").GetComponent<Button>();
    }

    public override void UpdateDesc(string desc)
    {
        this.desc.text = desc;
    }
    public override void UpdateProgress(string desc, int count)
    {
        progressdesc.text = desc;
        progressBar.value = count / allfileCount;
    }
    public override void RefreshUI()
    {
        desc.text = "";
        progressdesc.text = "";
        progressBar.value = 0;
        errorUI.SetActive(false);
        allfileCount = 0;
    }
    public override void ShowRetryButton(Action retryAction)
    {
        errorUI.SetActive(true);
        retryBtn.onClick.RemoveAllListeners();
        retryBtn.onClick.AddListener(() =>
        {
            retryAction?.Invoke();
            errorUI.SetActive(false);
        });
    }
    public override void ShowRetryButton(Action<object[]> retryAction)
    {
        errorUI.SetActive(true);
        retryBtn.onClick.RemoveAllListeners();
        retryBtn.onClick.AddListener(() =>
        {
            retryAction?.Invoke(null);
            errorUI.SetActive(false);
        });
    }
    public override void InitDownLoad(int allfileCount)
    {
        this.allfileCount = allfileCount;
    }
}