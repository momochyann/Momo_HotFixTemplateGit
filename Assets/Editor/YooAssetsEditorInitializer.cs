using YooAsset.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using Cysharp.Threading.Tasks;
using UnityEngine;
using YooAsset;
using UnityEngine.SceneManagement;

public static class YooAssetsEditorInitializer
{
    // 记录原始场景路径
    private static string _originalScenePath = null;
    [InitializeOnLoadMethod]
    private static void Initialize()
    {
        // 注册播放模式变更事件
#if UNITY_EDITOR
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#endif
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        bool enablePreload = EditorPrefs.GetBool("YooAssets_EnablePreload", true);
        if (!enablePreload)
            return;
            
        if (state == PlayModeStateChange.ExitingEditMode)
        {
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "HotFixScene")
            {
                Debug.Log("进入播放模式，初始化YooAssets");
                PlayerPrefs.SetString("OriginalScenePath", UnityEngine.SceneManagement.SceneManager.GetActiveScene().path);
                var tempScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
                EditorSceneManager.OpenScene(tempScene.path);
            }
        }
        else if (state == PlayModeStateChange.EnteredPlayMode)
        {
            _originalScenePath = PlayerPrefs.GetString("OriginalScenePath");
            Debug.Log(_originalScenePath);
            if (!string.IsNullOrEmpty(_originalScenePath))
            {
                CheckAndLoadPackage().Forget();
            }
        }
        else if (state == PlayModeStateChange.EnteredEditMode)
        {
            _originalScenePath = PlayerPrefs.GetString("OriginalScenePath");
            if (string.IsNullOrEmpty(_originalScenePath))
                return;
            if (SceneManager.GetActiveScene().path != _originalScenePath)
            {
                PlayerPrefs.SetString("OriginalScenePath", null);
                EditorSceneManager.OpenScene(_originalScenePath);
            }
        }

    }
    public static async UniTaskVoid CheckAndLoadPackage()
    {
        Debug.Log("初始化YooAssets");
        YooAssets.Initialize();
        foreach (var package in AssetBundleCollectorSettingData.Setting.Packages)
        {
            var resourcePackage = YooAssets.CreatePackage(package.PackageName);
            var eDefaultBuildPipeline = AssetBundleBuilderSetting.GetPackageBuildPipeline(package.PackageName);
            YooAssets.SetDefaultPackage(resourcePackage);
            if (resourcePackage.InitializeStatus == EOperationStatus.None)
            {
                var initParametersEditorSimulateMode = new EditorSimulateModeParameters();
                initParametersEditorSimulateMode.SimulateManifestFilePath =
                EditorSimulateModeHelper.SimulateBuild((EDefaultBuildPipeline)eDefaultBuildPipeline, package.PackageName);
                await resourcePackage.InitializeAsync(initParametersEditorSimulateMode);
            }
        }
        LoadYooAssetsTool.LoadSceneAsync(ExtractSceneName(_originalScenePath), IsLocalScene(_originalScenePath)).Forget();
    }
    private static bool IsLocalScene(string scenePath)
    {
        return !string.IsNullOrEmpty(scenePath) && scenePath.Contains("Local");
    }
    private static string ExtractSceneName(string scenePath)
    {
        if (string.IsNullOrEmpty(scenePath))
            return string.Empty;

        // 获取文件名（带扩展名）
        string fileName = System.IO.Path.GetFileName(scenePath);

        // 移除扩展名
        return System.IO.Path.GetFileNameWithoutExtension(fileName);
    }
}
