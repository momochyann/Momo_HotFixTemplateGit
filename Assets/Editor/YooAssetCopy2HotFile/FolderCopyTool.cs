using UnityEditor;
using UnityEngine;
using System.IO;
using HybridCLR.Editor.Commands;
public class FolderCopyTool : EditorWindow
{
    private string sourceFolderPath1 = "";
    private string targetFolderPath1 = "";
    private string sourceFolderPath2 = "";
    private string targetFolderPath2 = "";
    private string sourceFolderPath3 = "";
    private string targetFolderPath3 = "";
    private string sourceFolderPath4 = "";
    private string targetFolderPath4 = "";

    [MenuItem("Tools/Folder Copy Tool")]
    public static void ShowWindow()
    {
        GetWindow<FolderCopyTool>("Folder Copy Tool");
    }

    private void OnEnable()
    {
      //  CompileDllCommand.CompileDll(EditorUserBuildSettings.activeBuildTarget);
        sourceFolderPath1 = EditorPrefs.GetString(GetProjectFolderNamePath("SourceFolderPath1"), "");
        targetFolderPath1 = EditorPrefs.GetString(GetProjectFolderNamePath("TargetFolderPath1"), "");
        sourceFolderPath2 = EditorPrefs.GetString(GetProjectFolderNamePath("SourceFolderPath2"), "");
        targetFolderPath2 = EditorPrefs.GetString(GetProjectFolderNamePath("TargetFolderPath2"), "");
        sourceFolderPath3 = EditorPrefs.GetString(GetProjectFolderNamePath("SourceFolderPath3"), "");
        targetFolderPath3 = EditorPrefs.GetString(GetProjectFolderNamePath("TargetFolderPath3"), "");
        sourceFolderPath4 = EditorPrefs.GetString(GetProjectFolderNamePath("SourceFolderPath4"), "");
        targetFolderPath4 = EditorPrefs.GetString(GetProjectFolderNamePath("TargetFolderPath4"), "");
    }

    private void OnDisable()
    {
        EditorPrefs.SetString(GetProjectFolderNamePath("SourceFolderPath1"), sourceFolderPath1);
        EditorPrefs.SetString(GetProjectFolderNamePath("TargetFolderPath1"), targetFolderPath1);
        EditorPrefs.SetString(GetProjectFolderNamePath("SourceFolderPath2"), sourceFolderPath2);
        EditorPrefs.SetString(GetProjectFolderNamePath("TargetFolderPath2"), targetFolderPath2);
        EditorPrefs.SetString(GetProjectFolderNamePath("SourceFolderPath3"), sourceFolderPath3);
        EditorPrefs.SetString(GetProjectFolderNamePath("TargetFolderPath3"), targetFolderPath3);
        EditorPrefs.SetString(GetProjectFolderNamePath("SourceFolderPath4"), sourceFolderPath4);
        EditorPrefs.SetString(GetProjectFolderNamePath("TargetFolderPath4"), targetFolderPath4);
    }

    private string GetProjectFolderNamePath(string path)
    {
        string projectPath = Application.dataPath;
        return new DirectoryInfo(projectPath).Parent.Name + path;
    }

    private void OnGUI()
    {
        GUILayout.Label("Folder Copy Tool", EditorStyles.boldLabel);

        DrawCopyOperation("DefultPackge Copy", ref sourceFolderPath1, ref targetFolderPath1);
        EditorGUILayout.Space();
        
        DrawCopyOperation("RawFilePackge Copy", ref sourceFolderPath2, ref targetFolderPath2);
        EditorGUILayout.Space();
        
        DrawCopyOperation("LocalRawFilePackge Copy", ref sourceFolderPath3, ref targetFolderPath3);
        EditorGUILayout.Space();
        
        DrawCopyOperation("LocalDefultPackge Copy", ref sourceFolderPath4, ref targetFolderPath4);
    }

    private void DrawCopyOperation(string label, ref string sourcePath, ref string targetPath)
    {
        EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        sourcePath = EditorGUILayout.TextField("Source Folder", sourcePath);
        if (GUILayout.Button("Browse", GUILayout.Width(60)))
        {
            string startPath = string.IsNullOrEmpty(sourcePath) ? "" : Path.GetDirectoryName(sourcePath);
            sourcePath = EditorUtility.OpenFolderPanel("Select Source Folder", startPath, "");
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        targetPath = EditorGUILayout.TextField("Target Folder", targetPath);
        if (GUILayout.Button("Browse", GUILayout.Width(60)))
        {
            string startPath = string.IsNullOrEmpty(targetPath) ? "" : Path.GetDirectoryName(targetPath);
            targetPath = EditorUtility.OpenFolderPanel("Select Target Folder", startPath, "");
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Copy and Clear Target Folder"))
        {
            ClearAndCopyFolder(sourcePath, targetPath);
        }
        if (GUILayout.Button("Copy Without Clearing"))
        {
            CopyFolder(sourcePath, targetPath);
        }
        EditorGUILayout.EndHorizontal();
    }

    private void ClearAndCopyFolder(string sourceFolder, string targetFolder)
    {
        if (Directory.Exists(targetFolder))
        {
            Directory.Delete(targetFolder, true);
        }
        Debug.Log("Is Delete");
        CopyFolder(sourceFolder, targetFolder);
    }

    private void CopyFolder(string sourceFolder, string targetFolder)
    {
        if (!Directory.Exists(sourceFolder))
        {
            Debug.LogError("Source folder does not exist: " + sourceFolder);
            return;
        }
        if (!Directory.Exists(targetFolder))
        {
            Directory.CreateDirectory(targetFolder);
        }

        foreach (var file in Directory.GetFiles(sourceFolder))
        {
            var destFile = Path.Combine(targetFolder, Path.GetFileName(file));
            File.Copy(file, destFile, true);
        }

        foreach (var directory in Directory.GetDirectories(sourceFolder))
        {
            var destDirectory = Path.Combine(targetFolder, Path.GetFileName(directory));
            CopyFolder(directory, destDirectory);
        }
        Debug.Log("Is Copy");
    }
}
