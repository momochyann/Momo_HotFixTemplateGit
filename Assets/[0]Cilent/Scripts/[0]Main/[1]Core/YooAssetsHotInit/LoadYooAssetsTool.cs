
using YooAsset;
using Cysharp.Threading.Tasks;

public static class LoadYooAssetsTool
{
    public static bool isInit = false;
    public static ResourcePackage package = YooAssets.GetPackage("DefaultPackage");
    public static ResourcePackage LocalPackage = YooAssets.GetPackage("LocalDefaultPackage");

    public static ResourcePackage RawFilePackage = YooAssets.GetPackage("RawFilePackage");
    public static ResourcePackage LocalRawFilePackage = YooAssets.GetPackage("LocalRawFilePackage");
    /// <summary>
    /// 加载资源
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="AssetNames">资源名称</param>
    /// <param name="isLocal">是否本地加载</param>
    /// <returns></returns>
    public static async UniTask<T> LoadAsset<T>(string AssetNames, bool isLocal = false) where T : UnityEngine.Object
    {
        if (isLocal)
        {
            var handle = LocalPackage.LoadAssetAsync<T>(AssetNames);
            await handle.ToUniTask();
            return handle.AssetObject as T;
        }
        else
        {
            var handle = package.LoadAssetAsync<T>(AssetNames);
            await handle.ToUniTask();
            return handle.AssetObject as T;
        }
    }
    public static async UniTask<byte[]> LoadRawFile_DP(string AssetNames, bool isLocal = true)
    {
        if (isLocal)
        {
            var handle = LocalRawFilePackage.LoadRawFileAsync(AssetNames);
            await handle.ToUniTask();
            return handle.GetRawFileData();
        }
        else
        {
            var handle = RawFilePackage.LoadRawFileAsync(AssetNames);
            await handle.ToUniTask();
            return handle.GetRawFileData();
        }
    }
    public static async UniTaskVoid LoadSceneAsync(string AssetNames, bool isLocal = false)
    {
        var sceneMode = UnityEngine.SceneManagement.LoadSceneMode.Single;
        bool suspendLoad = false;
        if (isLocal)
        {
            SceneHandle handle = LocalPackage.LoadSceneAsync(AssetNames, sceneMode, suspendLoad);
            await handle.ToUniTask();
        }
        else
        {
            SceneHandle handle = package.LoadSceneAsync(AssetNames, sceneMode, suspendLoad);
            await handle.ToUniTask();
        }
        // return handle.InstantiateSync();
    }

}





