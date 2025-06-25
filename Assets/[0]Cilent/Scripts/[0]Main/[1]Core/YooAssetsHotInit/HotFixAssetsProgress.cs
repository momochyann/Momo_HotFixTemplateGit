using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.ProceduralImage;
using DG.Tweening;
using Cysharp.Threading.Tasks;
public class HotFixAssetsProgress : MonoBehaviour
{
    // Start is called before the first frame update 
    public ProceduralImage progressImage;
    Text progressText;
    public Queue<Tweener> _tweenerQueue = new Queue<Tweener>();
    Tweener _currentTweener;
    float _lastSetProgress = 0;
    void Start()
    {
        progressImage = transform.Find("LoadProgressBar").GetComponent<ProceduralImage>();
        progressText = transform.Find("LoadNameText").GetComponent<Text>();
        var LoadDll = FindObjectOfType<LoadDll>();
        LoadDll.OnHotFixAssetsProgress += OnHotFixAssetsProgress;
        LoadDll.OnHotFixPackageDownload += OnHotFixPackageDownloadTextDisplay;
    }
    public void PlaySupplementaryMetadataStartAnimation()
    {
        AddDynamicLoadingAnimation(DOTween.To(_value =>
            {
                progressImage.fillAmount = _value;
            }, 0.01f, 0, 0.01f).SetEase(Ease.Linear), "正在加载资源...");
        _lastSetProgress = 0;
    }
    public void PlaySupplementaryMetadataLoadingAnimation(float progress)
    {
        string tipText = progress == 1 ? "加载数据完成,准备进入" : null;
        float duration = 0.3f;
        #if UNITY_EDITOR
        duration = 0.01f;
        #endif
        AddDynamicLoadingAnimation(DOTween.To(_value =>
            {
                progressImage.fillAmount = _value;
            }, _lastSetProgress, progress,duration).SetEase(Ease.Linear), tipText);
        _lastSetProgress = progress;
    }

    private void OnHotFixAssetsProgress(long currentDownloadBytes, long totalDownloadBytes)
    {
        float targetProgress = (float)currentDownloadBytes / (float)totalDownloadBytes;
        Debug.Log(string.Format("当前下载进度：{0}, 总下载进度：{1}", _lastSetProgress, targetProgress));
        AddDynamicLoadingAnimation(DOTween.To(_value =>
            {
                progressImage.fillAmount = _value;
            }, _lastSetProgress, targetProgress, 0.02f).SetEase(Ease.Linear));
        _lastSetProgress = targetProgress;
    }

    private void OnHotFixPackageDownloadTextDisplay(string packageName, long totalDownloadBytes)
    {
        var packChinesName = packageName == "DefaultPackage" ? "资源包" : "源码包";
        if (totalDownloadBytes > 0)
        {
            var tipText = $"正在下载{packChinesName}，总计{((float)totalDownloadBytes / 1024 / 1024).ToString("F2")}MB";
            _lastSetProgress = 0;
            AddDynamicLoadingAnimation(DOTween.To(_value =>
            {
                progressImage.fillAmount = _value;
            }, 0, 0.1f, 0.01f).SetEase(Ease.Linear), tipText);
        }
        else
        {
            var tipText = $"正在加载本地{packChinesName}";
            progressImage.fillAmount = 0;
            AddDynamicLoadingAnimation(DOTween.To(_value =>
            {
                progressImage.fillAmount = _value;
            }, 0, 1f, 0.3f).SetEase(Ease.Linear), tipText);
        }
    }
    void AddDynamicLoadingAnimation(Tweener tweener, string tipText = null)
    {
        tweener.Pause();
        if (tipText != null)
        {
            tweener.OnPlay(() =>
            {
                progressText.text = tipText;
            });
        }
        tweener.OnComplete(() =>
        {
            _currentTweener = null;
        });
        _tweenerQueue.Enqueue(tweener);
    }
    private void Update()
    {
        if (_currentTweener == null && _tweenerQueue.Count > 0)
        {
            _currentTweener = _tweenerQueue.Dequeue();
            _currentTweener.Play();
        }
    }
}
