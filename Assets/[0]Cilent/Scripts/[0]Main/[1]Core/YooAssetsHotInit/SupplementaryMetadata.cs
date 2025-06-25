using UnityEngine;
using UnityEngine.UI;
using HybridCLR;
using System.Collections.Generic;
using System;
using Cysharp.Threading.Tasks;
using UnityEngine.Events;
public class SupplementaryMetadata : MonoBehaviour
{
    //AOT载入Hot后第一时间进行元数据的补充
    public UnityAction onSupplementaryMetadata;

    public List<string> aotMetaAssemblyFiles = new List<string>()
        {
            "mscorlib.dll",
            "System.dll",
            "System.Core.dll",
            "UniTask.dll",
            "Main.dll",
            "QFramework.dll",
            "QFramework.Core.dll",
            "DOTween.dll",
            "UnityEngine.CoreModule.dll",
            "UniTask.Linq.dll"
        };
    private void Awake()
    {
        // LoadMetadataForAOTAssemblies();
        LoadMetadataForAOTAssemblies().Forget();

    }
    async UniTaskVoid LoadMetadataForAOTAssemblies()
    {
        var progress = FindObjectOfType<HotFixAssetsProgress>();
        HomologousImageMode mode = HomologousImageMode.SuperSet;
        int totalCount = aotMetaAssemblyFiles.Count;
        int currentCount = 0;
        progress.PlaySupplementaryMetadataStartAnimation();
        foreach (var aotDllName in aotMetaAssemblyFiles)
        {
            var dllBytes = await LoadYooAssetsTool.LoadRawFile_DP(aotDllName);
            LoadImageErrorCode err = RuntimeApi.LoadMetadataForAOTAssembly(dllBytes, mode);
            progress.PlaySupplementaryMetadataLoadingAnimation(++currentCount / (float)totalCount);
        }
        Debug.Log("LoadDLL");
        StarHotfix();
    }
    void StarHotfix()
    {
        onSupplementaryMetadata?.Invoke();
    }
}

/// <summary>
/// 为aot assembly加载原始metadata， 这个代码放aot或者热更新都行。
/// 一旦加载后，如果AOT泛型函数对应native实现不存在，则自动替换为解释模式执行
/// </summary>
// private static async UniTask LoadMetadataForAOTAssemblies()
// {
// HomologousImageMode mode = HomologousImageMode.SuperSet;
// var package = YooAssets.GetPackage("RawFilePackage");
// RawFileHandle handle1 = package.LoadRawFileAsync("mscorlib.dll");
// await handle1.ToUniTask();
// byte[] dllBytes1 = handle1.GetRawFileData();
// LoadImageErrorCode err1 = RuntimeApi.LoadMetadataForAOTAssembly(dllBytes1, mode);
// List<string> aotMetaAssemblyFiles = new List<string>()
// {
//     // "mscorlib.dll",
//     "System.dll",
//     "System.Core.dll",
//     "UniTask.dll",
// };
// /// 注意，补充元数据是给AOT dll补充元数据，而不是给热更新dll补充元数据。
// /// 热更新dll不缺元数据，不需要补充，如果调用LoadMetadataForAOTAssembly会返回错误


// foreach (var aotDllName in aotMetaAssemblyFiles)
// {
//     RawFileHandle handle = package.LoadRawFileAsync(aotDllName);
//     // RawFileHandle handle = package.LoadRawFileAsync("System.dll");
//     await handle.ToUniTask();
//     byte[] dllBytes = handle.GetRawFileData();
//     // 加载assembly对应的dll，会自动为它hook。一旦aot泛型函数的native函数不存在，用解释器版本代码
//     LoadImageErrorCode err = RuntimeApi.LoadMetadataForAOTAssembly(dllBytes, mode);
//     Debug.Log($"LoadMetadataForAOTAssembly:{aotDllName}. mode:{mode} ret:{err}");
// }
// }