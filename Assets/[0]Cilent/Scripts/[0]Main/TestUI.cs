using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QFramework;
using UnityEngine.UI;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using YooAsset;
public class TestUI : MonoBehaviour
{
    // Start is called before the first frame update
    Image _image;
    void Start()
    {
        _image = GetComponent<Image>();
        LoadStartTitleImage().Forget();

    }
    async UniTaskVoid LoadStartTitleImage()
    {
        await UniTask.Delay(100);
        var sprite = await LoadYooAssetsTool.LoadAsset<Sprite>("StartTitleImage",true);
        _image.sprite = sprite;
        _image.SetNativeSize();
    }
    // Update is called once per frame
    void Update()
    {

    }
}
