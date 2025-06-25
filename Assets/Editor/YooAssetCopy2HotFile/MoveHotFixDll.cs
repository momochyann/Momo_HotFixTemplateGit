using UnityEditor;
using UnityEngine;
using System.IO;

public class MoveHotFixDll : EditorWindow
{
    private string sourceFolderPath = "";
    private string targetFolderPath = "";

    [MenuItem("Tools/Move HotFix DLL")]
    public static void ShowWindow()
    {
        GetWindow<MoveHotFixDll>("Move HotFix DLL");
    }

     private void OnEnable()
    {
        sourceFolderPath = EditorPrefs.GetString(GetProjectFolderNamePath("SourceFolderPath"), "");
        targetFolderPath = EditorPrefs.GetString(GetProjectFolderNamePath("targetFolderPath"), "");
    }

    private void OnDisable()
    {
        EditorPrefs.SetString(GetProjectFolderNamePath("SourceFolderPath"), sourceFolderPath);
        EditorPrefs.SetString(GetProjectFolderNamePath("targetFolderPath"), targetFolderPath);
    }
    private string GetProjectFolderNamePath(string path)
    {
        string projectPath = Application.dataPath;
        return new DirectoryInfo(projectPath).Parent.Name + path;
    }
    private void OnGUI()
    {
        GUILayout.Label("Move HotFix DLL", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        sourceFolderPath = EditorGUILayout.TextField("Source Folder", sourceFolderPath);
        if (GUILayout.Button("Browse"))
        {
            sourceFolderPath = EditorUtility.OpenFolderPanel("Select Source Folder", "", "");
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        targetFolderPath = EditorGUILayout.TextField("Target Folder", targetFolderPath);
        if (GUILayout.Button("Browse"))
        {
            targetFolderPath = EditorUtility.OpenFolderPanel("Select Target Folder", "", "");
        }
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Move and Rename DLL"))
        {
            MoveAndRenameDll(sourceFolderPath, targetFolderPath);
        }
    }

    private void MoveAndRenameDll(string sourceFolder, string targetFolder)
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

        string dllFilePath = Path.Combine(sourceFolder, "HotFix.dll");
        if (!File.Exists(dllFilePath))
        {
            Debug.LogError("HotFix.dll does not exist in the source folder.");
            return;
        }

        string targetFilePath = Path.Combine(targetFolder, "HotFix.dll.bytes");
        File.Copy(dllFilePath, targetFilePath, true);

        Debug.Log($"HotFix.dll moved and renamed to {targetFilePath}");
    }
}
