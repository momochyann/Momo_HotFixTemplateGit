using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HybridCLR;
using Cysharp.Threading.Tasks;
using YooAsset;
using UnityEditor;
using UnityEngine.Networking;
using UnityEngine.UI;
using System;
using UnityEngine.Events;
using JetBrains.Annotations;
public enum Badge
{
    latest,
    test,
    online
}
public class LoadDll : MonoBehaviour
{
    public string BadgeKey = "Badge";
    [SerializeField] private Badge badge = Badge.latest;
    public string targetBadge => GetBadge();
    // 资源系统运行模式
    public EPlayMode PlayMode = EPlayMode.EditorSimulateMode;
    //热更新的dll名称、注意这里是因为使用了yooasset所以直接资源原名称读取即可
    public string HotDllName = "hotfix.dll";
    //加载进入热更新流程的第一个预制体
    public string HotPrefabName = "HotFix_Import";

    #region CDN远程URL属性设置
    //远端先获取CDN中的版本号，判断热更新的版本url路径，这里是为了适配微信小游戏
    // //因为微信热更新判断是需要每次都更新url路径，才会进行下载，资源路径不变是不会下载的  
    // private string CdnVersion =
    // "https://a.unity.cn/client_api/v1/buckets/2bd437f2-f81d-49aa-99bc-ab14c7190cbe/release_by_badge/latest/entry_by_path/content/?path=version.txt";
    private string CdnVersion => GetCDNVersion();

    //远端CDN地址版本号更新地址
    private string DefaultHostServer;
    private string FallbackHostServer;
    #endregion
    public UnityAction<long, long> OnHotFixAssetsProgress;
    public UnityAction<string, long> OnHotFixPackageDownload;
    #region 获取远端CDN资源更新的完整路径
    private async UniTask<string> Get_CDN_URL(string fileURL)
    {
        Debug.Log(fileURL);
        using (UnityWebRequest www = UnityWebRequest.Get(fileURL))
        {
            await www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Failed to retrieve file version: " + www.error);
            }
            string version = www.downloadHandler.text;
            Debug.Log("File version: " + version);
            return version;
        }
    }

    #endregion
    string GetCDNVersion()
    {
        return $"https://a.unity.cn/client_api/v1/buckets/{BadgeKey}/release_by_badge/{targetBadge}/entry_by_path/content/?path={GetPlatformURL()}version.txt";
    }
    public string GetPlatformURL()
    {
#if UNITY_EDITOR
        if (UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.Android)
            return "Android/";
        else if (UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.iOS)
            return "IPhone/";
        else if (UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.WebGL)
            return "WebGL/";
        else
            return "PC/";
#else
        if (Application.platform == RuntimePlatform.Android)
            return "Android/";
        else if (Application.platform == RuntimePlatform.IPhonePlayer)
            return "IPhone/";
        else if (Application.platform == RuntimePlatform.WebGLPlayer)
            return "WebGL/";
        else
            return "PC/";
#endif
    }
    public async UniTask<string> GetCdnVersion()
    {
        return await Get_CDN_URL(CdnVersion);
    }
    public string GetBadge()
    {
        var _targetBadge = PlayerPrefs.GetString("Badge");
        if (string.IsNullOrEmpty(_targetBadge))
        {
            _targetBadge = badge.ToString();
        }
        return _targetBadge;
    }
    private void Start()
    {
        // LoadStart().Forget();
    }
    public void Button_LoadStart(bool isLocal)
    {
        LoadStart(isLocal).Forget();
    }
    public async UniTaskVoid LoadStart(bool isLocal)
    {
        if (isLocal)
        {
            DefaultHostServer = FallbackHostServer = PlayerPrefs.GetString("DefaultHostServer");
        }
        else
        {
            DefaultHostServer = FallbackHostServer =
            $"https://a.unity.cn/client_api/v1/buckets/{BadgeKey}/release_by_badge/{targetBadge}/entry_by_path/content/?path={GetPlatformURL()}{await Get_CDN_URL(CdnVersion)}";
            PlayerPrefs.SetString("DefaultHostServer", DefaultHostServer);
        }

        YooAssets.Initialize();
        await LoadLocalPackage("LocalDefaultPackage", EDefaultBuildPipeline.BuiltinBuildPipeline);
        await LoadLocalPackage("LocalRawFilePackage", EDefaultBuildPipeline.RawFileBuildPipeline);
        await DownLoadYooAssets("DefaultPackage", EDefaultBuildPipeline.BuiltinBuildPipeline, isLocal);
        await DownLoadYooAssets("RawFilePackage", EDefaultBuildPipeline.RawFileBuildPipeline, isLocal);
        StartGame().Forget();

        //
    }
    // 资源包更新
    #region yooasset
    private async UniTask LoadLocalPackage(string PackageName, EDefaultBuildPipeline eDefaultBuildPipeline)
    {
        var package = YooAssets.CreatePackage(PackageName);
        YooAssets.SetDefaultPackage(package);
        if (PlayMode != EPlayMode.EditorSimulateMode)
        {
            var initParametersOfflinePlayMode = new OfflinePlayModeParameters();
            //initParametersOfflinePlayMode.RootFolder = "Assets/StreamingAssets/";
            await package.InitializeAsync(initParametersOfflinePlayMode);
        }
        else
        {
            var initParametersEditorSimulateMode = new EditorSimulateModeParameters();
            initParametersEditorSimulateMode.SimulateManifestFilePath =
            EditorSimulateModeHelper.SimulateBuild(eDefaultBuildPipeline, PackageName);
            await package.InitializeAsync(initParametersEditorSimulateMode);
        }
    }


    private async UniTask DownLoadYooAssets(string PackageName, EDefaultBuildPipeline eDefaultBuildPipeline, bool isLoacl)
    {
        var package = YooAssets.CreatePackage(PackageName);
        // 设置该资源包为默认的资源包，可以使用YooAssets相关加载接口加载该资源包内容。
        YooAssets.SetDefaultPackage(package);
        switch (PlayMode)
        {
            case EPlayMode.EditorSimulateMode:
                //编辑器模拟模式

                var initParametersEditorSimulateMode = new EditorSimulateModeParameters();
                initParametersEditorSimulateMode.SimulateManifestFilePath =
                EditorSimulateModeHelper.SimulateBuild(eDefaultBuildPipeline, PackageName);
                await package.InitializeAsync(initParametersEditorSimulateMode);
                break;
            case EPlayMode.OfflinePlayMode:
                //单机模式
                var initParametersOfflinePlayMode = new OfflinePlayModeParameters();
                //initParametersOfflinePlayMode.RootFolder = "Assets/StreamingAssets/";
                await package.InitializeAsync(initParametersOfflinePlayMode);
                break;
            case EPlayMode.HostPlayMode:
                //联机运行模式
                var initParametersHostPlayMode = new HostPlayModeParameters();
                initParametersHostPlayMode.BuildinQueryServices = new GameQueryServices();
                // initParametersHostPlayMode.DecryptionServices  = DefaultHostServer;  加密类
                initParametersHostPlayMode.RemoteServices = new RemoteServices(DefaultHostServer, FallbackHostServer);
                //  initParametersHostPlayMode.CacheFileAppendExtension
                await package.InitializeAsync(initParametersHostPlayMode);
                //原始文件更新
                break;
        }
        string PackageVersion = PlayerPrefs.GetString(PackageName + "GameVersion");
        if (!isLoacl)
        {
            //2.获取资源版本
            var operation = package.UpdatePackageVersionAsync(false);

            //yield return operation;
            await operation.ToUniTask();

            if (operation.Status != EOperationStatus.Succeed)
            {
                //ShowText("获取远程资源版本信息失败", DownloadEnum.false_download).Forget();
                Debug.LogError(operation.Error);
                return;

            }
            PackageVersion = operation.PackageVersion;
            PlayerPrefs.SetString(PackageName + "GameVersion", PackageVersion);
            //3.更新补丁清单
        }

        var operation2 = package.UpdatePackageManifestAsync(PackageVersion);
        await operation2.ToUniTask();

        if (operation2.Status != EOperationStatus.Succeed)
        {
            // ShowText("更新资源版本清单失败", DownloadEnum.false_download).Forget();
            //更新失败
            Debug.LogError(operation2.Error);
            return;
        }
        await DownloadFlie(PackageName);
    }

    private async UniTask DownloadFlie(string PackageName)
    {
        int downloadingMaxNum = 10;
        int failedTryAgain = 3;
        int timeout = 60;
        var package = YooAssets.GetPackage(PackageName);
        var downloader = package.CreateResourceDownloader(downloadingMaxNum, failedTryAgain, timeout);
        //没有需要下载的资源
        if (downloader.TotalDownloadCount == 0)
        {
            Debug.Log("没有资源更新，直接进入游戏加载环节");
            OnHotFixPackageDownload?.Invoke(PackageName, 0);
            // StartGame().Forget();
            return;
        }
        OnHotFixPackageDownload?.Invoke(PackageName, downloader.TotalDownloadBytes);
        //需要下载的文件总数和总大小
        int totalDownloadCount = downloader.TotalDownloadCount;
        long totalDownloadBytes = downloader.TotalDownloadBytes;
        Debug.Log($"文件总数:{totalDownloadCount}:::总大小:{totalDownloadBytes}");
        await UniTask.DelayFrame(1);
        //进行下载
        await GetDownloadFile(PackageName);
    }

    private async UniTask GetDownloadFile(String PackageName)
    {
        int downloadingMaxNum = 10;
        int failedTryAgain = 3;
        int timeout = 60;
        var package = YooAssets.GetPackage(PackageName);
        var downloader = package.CreateResourceDownloader(downloadingMaxNum, failedTryAgain, timeout);

        //注册回调方法
        downloader.OnDownloadErrorCallback = OnDownloadErrorFunction;
        downloader.OnDownloadProgressCallback = OnDownloadProgressUpdateFunction;
        downloader.OnDownloadOverCallback = OnDownloadOverFunction;
        downloader.OnStartDownloadFileCallback = OnStartDownloadFileFunction;

        //开启下载
        downloader.BeginDownload();
        await downloader.ToUniTask();
        //检测下载结果
        if (downloader.Status == EOperationStatus.Succeed)
        {
            //下载成功,加载AOT泛型dll数据

            Debug.Log(PackageName + "更新完成!");
        }
        else
        {
            //下载失败
            Debug.LogError(PackageName + "更新失败！");
            //TODO:
        }
    }

    #region yooasset下载回调函数
    /// <summary>
    /// 下载数据大小
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="sizeBytes"></param>
    /// <exception cref="NotImplementedException"></exception>
    private void OnStartDownloadFileFunction(string fileName, long sizeBytes)
    {
        Debug.Log(string.Format("开始下载：文件名：{0}, 文件大小：{1}", fileName, sizeBytes));
    }
    /// <summary>
    /// 下载完成与否
    /// </summary>
    /// <param name="isSucceed"></param>
    /// <exception cref="NotImplementedException"></exception>
    private void OnDownloadOverFunction(bool isSucceed)
    {
        Debug.Log("下载" + (isSucceed ? "成功" : "失败"));
    }


    /// <summary>
    /// 更新中
    /// </summary>
    /// <param name="totalDownloadCount"></param>
    /// <param name="currentDownloadCount"></param>
    /// <param name="totalDownloadBytes"></param>
    /// <param name="currentDownloadBytes"></param>
    /// <exception cref="NotImplementedException"></exception>
    private void OnDownloadProgressUpdateFunction(int totalDownloadCount, int currentDownloadCount, long totalDownloadBytes, long currentDownloadBytes)
    {
        OnHotFixAssetsProgress?.Invoke(currentDownloadBytes, totalDownloadBytes);
        Debug.Log(string.Format("文件总数：{0}, 已下载文件数：{1}, 下载总大小：{2}, 已下载大小：{3}", totalDownloadCount, currentDownloadCount, totalDownloadBytes, currentDownloadBytes));
    }
    /// <summary>
    /// 下载出错
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="error"></param>
    /// <exception cref="NotImplementedException"></exception>
    private void OnDownloadErrorFunction(string fileName, string error)
    {
        Debug.LogError(string.Format("下载出错：文件名：{0}, 错误信息：{1}", fileName, error));
    }
    #endregion

    #endregion


    private async UniTask StartGame()
    {
        Debug.Log("StartGame");
#if !UNITY_EDITOR
      	byte[] dllBytes = await LoadYooAssetsTool.LoadRawFile_DP(HotDllName,false);
        System.Reflection.Assembly.Load(dllBytes);                      //加载热更新dll
#endif
        Debug.Log("StartGame");
        var go = await LoadYooAssetsTool.LoadAsset<GameObject>(HotPrefabName);
        Instantiate(go);
    }

    private class RemoteServices : IRemoteServices      //请求网址容器
    {
        private readonly string _defaultHostServer;
        private readonly string _fallbackHostServer;

        public RemoteServices(string defaultHostServer, string fallbackHostServer)
        {
            _defaultHostServer = defaultHostServer;
            _fallbackHostServer = fallbackHostServer;
        }
        string IRemoteServices.GetRemoteMainURL(string fileName)
        {
            return $"{_defaultHostServer}/{fileName}";
        }
        string IRemoteServices.GetRemoteFallbackURL(string fileName)
        {
            return $"{_fallbackHostServer}/{fileName}";
        }
    }
}
