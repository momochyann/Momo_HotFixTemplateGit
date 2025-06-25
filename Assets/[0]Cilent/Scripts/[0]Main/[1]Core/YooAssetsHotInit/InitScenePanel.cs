using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
public class InitScenePanel : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] LoadDll loadDll;
    Image titleImage;
    Image BackImage;
    string BadgeKey => loadDll.BadgeKey;
    string TargetBadge => loadDll.targetBadge;
    string GetPlatformURL() => loadDll.GetPlatformURL();
    string TitleImageUrl => $"https://a.unity.cn/client_api/v1/buckets/{BadgeKey}/release_by_badge/{TargetBadge}/entry_by_path/content/?path={GetPlatformURL()}StartTitleImage.png";
    string TitleImageLocalPath => $"{Application.persistentDataPath}/TitleImage.png";
    string StartBackImageUrl => $"https://a.unity.cn/client_api/v1/buckets/{BadgeKey}/release_by_badge/{TargetBadge}/entry_by_path/content/?path={GetPlatformURL()}StartBackImage.png";
    string StartBackImageLocalPath => $"{Application.persistentDataPath}/StartBackImage.png";
    private void Awake()
    {
        titleImage = transform.Find("TitleImage").GetComponent<Image>();
        BackImage = GetComponent<Image>();
        LoadCacheImage(titleImage, TitleImageLocalPath);
        LoadCacheImage(BackImage, StartBackImageLocalPath);
    }

    void Start()
    {
        LoadStart().Forget();
    }
    async UniTask LoadStart()
    {
        bool isNetwork = await NetWorkCheck.CheckNetworkAsync();//判断是否联网
        if (isNetwork && await CheckCdnVersion())
        {
            await DownLoadHostImage(TitleImageUrl, TitleImageLocalPath);
            await DownLoadHostImage(StartBackImageUrl, StartBackImageLocalPath);
        }
    }
    async UniTask<bool> CheckCdnVersion()
    {
        string version = await loadDll.GetCdnVersion();
        var currentVersion = PlayerPrefs.GetString("PackageVersion");
        PlayerPrefs.SetString("PackageVersion", version);
        return version != currentVersion;
    }
    async UniTask DownLoadHostImage(string url, string localPath)
    {
        try
        {
            Debug.Log("开始下载初始图片: " + url);
            using (UnityWebRequest www = UnityWebRequest.Get(url))
            {
                await www.SendWebRequest();
                if (www.result != UnityWebRequest.Result.Success)
                {
                    throw new Exception(www.error);
                }
                byte[] imageData = www.downloadHandler.data;
                File.WriteAllBytes(localPath, imageData);
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("下载初始图片失败: " + e.Message);
        }
    }

    private void LoadCacheImage(Image image, string localPath)
    {
        if (File.Exists(localPath))
        {
            try
            {
                byte[] imageData = File.ReadAllBytes(localPath);
                Texture2D texture = new Texture2D(2, 2);
                texture.LoadImage(imageData);
                image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                image.SetNativeSize();
            }
            catch (Exception e)
            {
                Debug.LogWarning("加载缓存背景图片失败: " + e.Message);
            }
        }
    }
}
