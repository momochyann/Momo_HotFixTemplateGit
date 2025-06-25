using UnityEngine;
using UnityEditor;
using HybridCLR.Editor.Settings;
using HybridCLR.Editor.Commands;
using System.IO;
using YooAsset.Editor;
using System.Collections.Generic;
using System;

public class Build2AsyncCDN : EditorWindow
{
    private string targetFolderPath = "";
    private string hotUpdateTargetPath = "";
    private string packageVersion = "1.0"; // 版本号
    private Dictionary<string, bool> packageStates = new Dictionary<string, bool>(); // 选择状态
    private Dictionary<string, bool> buildStates = new Dictionary<string, bool>(); // 构建状态
    private Dictionary<string, bool> hotUpdateStates = new Dictionary<string, bool>(); // 热更新状态
    private bool firstCopyNormal = true;
    private bool firstCopyHotUpdate = true;
    private bool enableYooAssetsPreload = true;

    [MenuItem("Tools/YooAsset Build Tool")]
    public static void ShowWindow()
    {
        GetWindow<Build2AsyncCDN>("YooAsset Build Tool");
    }

    private void OnEnable()
    {
        targetFolderPath = EditorPrefs.GetString(GetProjectFolderNamePath("TargetFolderPath"), "");
        hotUpdateTargetPath = EditorPrefs.GetString(GetProjectFolderNamePath("HotUpdateTargetPath"), "");
        enableYooAssetsPreload = EditorPrefs.GetBool("YooAssets_EnablePreload", true);

        // 初始化包体状态
        packageStates.Clear();
        buildStates.Clear();
        hotUpdateStates.Clear();
        foreach (var package in AssetBundleCollectorSettingData.Setting.Packages)
        {
            packageStates[package.PackageName] = false;
            buildStates[package.PackageName] = false;
            hotUpdateStates[package.PackageName] = false;
        }
    }

    private void OnDisable()
    {
        EditorPrefs.SetString(GetProjectFolderNamePath("TargetFolderPath"), targetFolderPath);
        EditorPrefs.SetString(GetProjectFolderNamePath("HotUpdateTargetPath"), hotUpdateTargetPath);
        EditorPrefs.SetBool("YooAssets_EnablePreload", enableYooAssetsPreload);
    }

    private string GetProjectFolderNamePath(string path)
    {
        string projectPath = Application.dataPath;
        return new DirectoryInfo(projectPath).Parent.Name + path;
    }

    private void OnGUI()
    {
        GUILayout.Label("YooAsset Build Tool", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        targetFolderPath = EditorGUILayout.TextField("HotFixDll Folder", targetFolderPath);
        if (GUILayout.Button("Browse", GUILayout.Width(60)))
        {
            string path = EditorUtility.OpenFolderPanel("Select Target Folder", "", "");
            if (!string.IsNullOrEmpty(path))
            {
                targetFolderPath = path;
            }
        }
        EditorGUILayout.EndHorizontal();

        // 热更新包目标路径
        EditorGUILayout.BeginHorizontal();
        hotUpdateTargetPath = EditorGUILayout.TextField("Hot Update Target", hotUpdateTargetPath);
        if (GUILayout.Button("Browse", GUILayout.Width(60)))
        {
            string path = EditorUtility.OpenFolderPanel("Select Hot Update Target Folder", "", "");
            if (!string.IsNullOrEmpty(path))
            {
                hotUpdateTargetPath = path;
            }
        }
        EditorGUILayout.EndHorizontal();
        // 添加预加载选项
        EditorGUILayout.Space();
        GUILayout.Label("YooAssets Settings", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();
        enableYooAssetsPreload = EditorGUILayout.Toggle("Enable Scene Preloading", enableYooAssetsPreload);
        if (EditorGUI.EndChangeCheck())
        {
            EditorPrefs.SetBool("YooAssets_EnablePreload", enableYooAssetsPreload);
        }
        if (enableYooAssetsPreload)
        {
            EditorGUILayout.HelpBox(
                "When enabled, YooAssets will be initialized in a temporary scene before loading the original scene.\n" +
                "You can debug YooAssets resources from any scene, no need to start from HotFixScene",
                MessageType.Info);
        }
        // 显示包体选项
        EditorGUILayout.Space();
        GUILayout.Label("Select Packages to Build:", EditorStyles.boldLabel);

        // 表头
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Package Name", GUILayout.Width(200));
        EditorGUILayout.LabelField("Select", GUILayout.Width(50));
        EditorGUILayout.LabelField("Build", GUILayout.Width(50)); // 新增：构建选项
        EditorGUILayout.LabelField("Hot Update", GUILayout.Width(70));
        EditorGUILayout.EndHorizontal();

        // 包体列表
        if (AssetBundleCollectorSettingData.Setting != null &&
            AssetBundleCollectorSettingData.Setting.Packages != null)
        {
            foreach (var package in AssetBundleCollectorSettingData.Setting.Packages)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(package.PackageName, GUILayout.Width(200));

                // 选择选项
                packageStates[package.PackageName] = EditorGUILayout.Toggle(
                    packageStates[package.PackageName],
                    GUILayout.Width(50)
                );

                // 构建选项
                buildStates[package.PackageName] = EditorGUILayout.Toggle(
                    buildStates[package.PackageName],
                    GUILayout.Width(50)
                );

                // 热更新选项
                hotUpdateStates[package.PackageName] = EditorGUILayout.Toggle(
                    hotUpdateStates[package.PackageName],
                    GUILayout.Width(70)
                );

                EditorGUILayout.EndHorizontal();
            }
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Hot Update"))
        {
            firstCopyHotUpdate = true;
            BuildSelectedPackages(true);
        }

        if (GUILayout.Button("Local Update"))
        {
            firstCopyNormal = true;
            BuildSelectedPackages(false);
        }
    }

    private void BuildSelectedPackages(bool buildHotUpdate)
    {
        // 自动生成版本号
        string originalVersion = packageVersion;
        GenerateTimeStampVersion();

        bool anyProcessed = false;
        foreach (var package in AssetBundleCollectorSettingData.Setting.Packages)
        {
            // 只处理被选中且热更新状态匹配的包
            if (packageStates[package.PackageName] && hotUpdateStates[package.PackageName] == buildHotUpdate)
            {
                anyProcessed = true;
                firstCopyNormal = true;
                // 根据构建状态决定是构建还是直接复制
                bool shouldBuildPackage = buildStates[package.PackageName];

                if (shouldBuildPackage)
                {
                    // 构建包
                    Debug.Log($"Building package: {package.PackageName}");

                    if (package.PackageName == "RawFilePackage")
                    {
                        BuildAndCopyDll();
                    }
                    BuildPackage(package.PackageName);

                    // 复制新构建的包
                    if (buildHotUpdate)
                    {
                        // 热更新包使用用户选择的目标路径
                        if (!string.IsNullOrEmpty(hotUpdateTargetPath))
                        {
                            CopyBuildOutput(package.PackageName, hotUpdateTargetPath, true);
                        }
                    }
                    else
                    {
                        // 普通包使用 StreamingAssets 的上一级目录加上包体名
                        string streamingAssetsRoot = AssetBundleBuilderHelper.GetStreamingAssetsRoot();
                        string targetDir = Path.Combine(streamingAssetsRoot, package.PackageName);
                        CopyBuildOutput(package.PackageName, targetDir, false);
                    }
                }
                else
                {
                    // 不构建，直接复制上一版本
                    Debug.Log($"Skipping build for package: {package.PackageName} (Build option is disabled)");

                    if (buildHotUpdate)
                    {
                        // 热更新包使用用户选择的目标路径
                        if (!string.IsNullOrEmpty(hotUpdateTargetPath))
                        {
                            CopyPreviousVersion(package.PackageName, hotUpdateTargetPath, true);
                        }
                    }
                    else
                    {
                        // 普通包使用 StreamingAssets 的上一级目录加上包体名
                        string streamingAssetsRoot = AssetBundleBuilderHelper.GetStreamingAssetsRoot();
                        string targetDir = Path.Combine(streamingAssetsRoot, package.PackageName);
                        CopyPreviousVersion(package.PackageName, targetDir, false);
                    }
                }
            }
        }

        if (!anyProcessed)
        {
            Debug.LogWarning($"No {(buildHotUpdate ? "hot update" : "normal")} package selected for processing!");
        }

        // 显示使用的版本号
        Debug.Log($"Built with version: {packageVersion}");

        // 强制重绘窗口以显示新版本号
        Repaint();
    }

    // 新增方法：复制上一版本的包
    private void CopyPreviousVersion(string packageName, string targetPath, bool isHotUpdate)
    {
        try
        {
            // 获取构建输出目录
            string buildOutputRoot = AssetBundleBuilderHelper.GetDefaultBuildOutputRoot();
            string platformName = EditorUserBuildSettings.activeBuildTarget.ToString();

            // 查找该包的版本目录
            string packageRoot = Path.Combine(buildOutputRoot, platformName, packageName);

            if (!Directory.Exists(packageRoot))
            {
                Debug.LogError($"Package directory not found: {packageRoot}");
                return;
            }

            // 获取所有版本目录
            string[] versionDirs = Directory.GetDirectories(packageRoot);
            if (versionDirs.Length == 0)
            {
                Debug.LogError($"No version directories found for package: {packageName}");
                return;
            }

            // 按名称排序
            Array.Sort(versionDirs);

            // 获取第一个版本目录（按字母排序）作为上一版本
            string previousVersionDir = versionDirs[0];
            string previousVersion = Path.GetFileName(previousVersionDir);

            Debug.Log($"Using previous version for {packageName}: {previousVersion}");

            // 确保目标目录存在
            if (!Directory.Exists(targetPath))
            {
                Directory.CreateDirectory(targetPath);
            }

            // 第一次复制时清空目标目录
            bool isFirstCopy = isHotUpdate ? firstCopyHotUpdate : firstCopyNormal;
            if (isFirstCopy)
            {
                if (isHotUpdate)
                    firstCopyHotUpdate = false;
                else
                    firstCopyNormal = false;

                // 清空目标目录
                DirectoryInfo di = new DirectoryInfo(targetPath);
                foreach (FileInfo file in di.GetFiles())
                {
                    file.Delete();
                }
                foreach (DirectoryInfo dir in di.GetDirectories())
                {
                    dir.Delete(true);
                }

                Debug.Log($"Cleared target directory: {targetPath}");
            }

            // 复制所有文件
            CopyDirectory(previousVersionDir, targetPath, true);

            Debug.Log($"Successfully copied previous version from {previousVersionDir} to {targetPath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error copying previous version: {e.Message}");
        }
    }

    private void BuildPackage(string packageName)
    {
        Debug.Log($"Building package: {packageName}");

        // 获取构建管线类型
        var buildPipeline = AssetBundleBuilderSetting.GetPackageBuildPipeline(packageName);
        // 构建参数
        if (buildPipeline == EBuildPipeline.BuiltinBuildPipeline)
        {
            BuiltinBuildParameters buildParameters = new BuiltinBuildParameters();
            buildParameters.BuildTarget = EditorUserBuildSettings.activeBuildTarget;
            buildParameters.BuildPipeline = buildPipeline.ToString();
            buildParameters.BuildMode = EBuildMode.ForceRebuild;
            buildParameters.PackageName = packageName;
            buildParameters.PackageVersion = packageVersion;
            buildParameters.VerifyBuildingResult = true;
            buildParameters.BuildOutputRoot = AssetBundleBuilderHelper.GetDefaultBuildOutputRoot();
            buildParameters.BuildinFileRoot = AssetBundleBuilderHelper.GetStreamingAssetsRoot();
            buildParameters.BuildinFileCopyOption = EBuildinFileCopyOption.None;
            buildParameters.CompressOption = ECompressOption.LZ4;
            buildParameters.FileNameStyle = EFileNameStyle.HashName;
            buildParameters.EnableSharePackRule = true;

            // 执行构建
            BuiltinBuildPipeline pipeline = new BuiltinBuildPipeline();
            var buildResult = pipeline.Run(buildParameters, false);

            if (buildResult.Success)
            {
                Debug.Log($"Package {packageName} built successfully: {buildResult.OutputPackageDirectory}");
                EditorUtility.RevealInFinder(buildResult.OutputPackageDirectory);
            }
            else
            {
                Debug.LogError($"Failed to build package {packageName}!");
            }
        }
        else if (buildPipeline == EBuildPipeline.RawFileBuildPipeline)
        {
            RawFileBuildParameters buildParameters = new RawFileBuildParameters();
            buildParameters.BuildOutputRoot = AssetBundleBuilderHelper.GetDefaultBuildOutputRoot();
            buildParameters.BuildinFileRoot = AssetBundleBuilderHelper.GetStreamingAssetsRoot();
            buildParameters.BuildPipeline = buildPipeline.ToString();
            buildParameters.BuildTarget = EditorUserBuildSettings.activeBuildTarget;
            buildParameters.BuildMode = EBuildMode.ForceRebuild;
            buildParameters.PackageName = packageName;
            buildParameters.PackageVersion = packageVersion;
            buildParameters.FileNameStyle = EFileNameStyle.HashName;
            buildParameters.BuildinFileCopyOption = EBuildinFileCopyOption.None;
            buildParameters.BuildinFileCopyParams = string.Empty;

            // 执行构建
            RawFileBuildPipeline pipeline = new RawFileBuildPipeline();
            var buildResult = pipeline.Run(buildParameters, true);

            if (buildResult.Success)
            {
                Debug.Log($"Package {packageName} built successfully: {buildResult.OutputPackageDirectory}");
                EditorUtility.RevealInFinder(buildResult.OutputPackageDirectory);
            }
            else
            {
                Debug.LogError($"Failed to build package {packageName}!");
            }
        }
    }

    private void CopyBuildOutput(string packageName, string targetPath, bool isHotUpdate)
    {
        try
        {
            // 获取构建输出目录
            string buildOutputRoot = AssetBundleBuilderHelper.GetDefaultBuildOutputRoot();
            string platformName = EditorUserBuildSettings.activeBuildTarget.ToString();
            string sourcePath = Path.Combine(buildOutputRoot, platformName, packageName, packageVersion);

            if (!Directory.Exists(sourcePath))
            {
                Debug.LogError($"Source build directory not found: {sourcePath}");
                return;
            }

            // 确保目标目录存在
            if (!Directory.Exists(targetPath))
            {
                Directory.CreateDirectory(targetPath);
            }

            // 第一次复制时清空目标目录
            bool isFirstCopy = isHotUpdate ? firstCopyHotUpdate : firstCopyNormal;
            if (isFirstCopy)
            {
                if (isHotUpdate)
                    firstCopyHotUpdate = false;
                else
                    firstCopyNormal = false;

                // 清空目标目录
                DirectoryInfo di = new DirectoryInfo(targetPath);
                foreach (FileInfo file in di.GetFiles())
                {
                    file.Delete();
                }
                foreach (DirectoryInfo dir in di.GetDirectories())
                {
                    dir.Delete(true);
                }

                Debug.Log($"Cleared target directory: {targetPath}");
            }

            // 复制所有文件
            CopyDirectory(sourcePath, targetPath, true);

            Debug.Log($"Successfully copied build output from {sourcePath} to {targetPath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error copying build output: {e.Message}");
        }
    }

    private void CopyDirectory(string sourceDir, string targetDir, bool overwrite)
    {
        // 创建目标目录
        Directory.CreateDirectory(targetDir);

        // 复制文件
        foreach (var file in Directory.GetFiles(sourceDir))
        {
            string fileName = Path.GetFileName(file);
            string targetFile = Path.Combine(targetDir, fileName);
            File.Copy(file, targetFile, overwrite);
        }

        // 递归复制子目录
        foreach (var directory in Directory.GetDirectories(sourceDir))
        {
            string dirName = Path.GetFileName(directory);
            string targetSubDir = Path.Combine(targetDir, dirName);
            CopyDirectory(directory, targetSubDir, overwrite);
        }
    }

    private void BuildAndCopyDll()
    {
        // 先构建 DLL
        CompileDllCommand.CompileDll(EditorUserBuildSettings.activeBuildTarget);

        // 获取完整的源文件夹路径，包含平台信息
        string sourceFolder = Path.Combine(
            Directory.GetCurrentDirectory(),
            HybridCLRSettings.Instance.hotUpdateDllCompileOutputRootDir,
            EditorUserBuildSettings.activeBuildTarget.ToString()
        );

        string hotFixDllPath = Path.Combine(sourceFolder, "HotFix.dll");

        if (!File.Exists(hotFixDllPath))
        {
            Debug.LogError($"HotFix.dll not found after build at path: {hotFixDllPath}");
            return;
        }

        if (string.IsNullOrEmpty(targetFolderPath))
        {
            Debug.LogError("Target folder path is empty!");
            return;
        }

        if (!Directory.Exists(targetFolderPath))
        {
            Directory.CreateDirectory(targetFolderPath);
        }

        string targetPath = Path.Combine(targetFolderPath, "HotFix.dll.bytes");
        try
        {
            File.Copy(hotFixDllPath, targetPath, true);
            Debug.Log($"HotFix.dll successfully copied to: {targetPath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error copying HotFix.dll: {e.Message}");
        }
    }

    private void GenerateTimeStampVersion()
    {
        // 生成时间戳版本号：主版本号.年月日.时分
        string dateTime = System.DateTime.Now.ToString("yy-MMdd-HHmmss");
        packageVersion = $"{dateTime}";
    }
}
