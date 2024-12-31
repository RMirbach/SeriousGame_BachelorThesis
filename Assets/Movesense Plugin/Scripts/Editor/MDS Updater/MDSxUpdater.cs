///-------------------------------------------------------------------
///
///
///   Description:    Main Logic for Unity MDS Updater
///   Author:         Alex Kern, Vincent Uhlmann                  
///  
///
///-------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using UnityEditor;
using UnityEngine;

public class MDSxUpdater : EditorWindow
{
    private static readonly HttpClient client = new HttpClient();
    private static List<BuildTarget> isUpdating = new List<BuildTarget>();
    private static MDSxAndroidDependency androidDependency;
    private static string serverVersion;

    [MenuItem("Tools/Movesense Library/Update")]
    public static void Init()
    {
        EditorWindow window = GetWindow(typeof(MDSxUpdateEditorWindow));
        window.titleContent = new GUIContent("Movesense Library Update");
        window.Show();

        MDSxUpdateEditorWindow.UpdateAvailable = new MDSxUpdateAvailable() { IsAvailable = false, Message = "Check for updates...", ErrorType = MDSxErrorType.Warn };
        CheckForNewVersion();
    }

    public static void CheckForNewVersion()
    {
        if (isUpdating.Count > 0)
        {
            return;
        }

        isUpdating.Add(BuildTarget.iOS);

        using (WebClient wc = new WebClient())
        {
            try
            {
                androidDependency = JsonUtility.FromJson<MDSxAndroidDependency>(wc.DownloadString(MDSxConstants.AndroidDependencyFilePath));
            }
            catch (Exception)
            {
                MDSxUpdateEditorWindow.UpdateAvailable = new MDSxUpdateAvailable() { IsAvailable = false, Message = "Could not receive dependencies from Movesenseserver", ErrorType = MDSxErrorType.Error };
                return;
            }
        }
        serverVersion = androidDependency.android.version;

        string installedVersion = string.Empty;
        try
        {
            var files = Directory.GetFiles(MDSxPathHelper.Combine(MDSxConstants.MOVESENSE_PLUGIN_DIRECTORY, "Android"));
            foreach (var filePath in files)
            {
                var fileName = filePath.Substring(filePath.LastIndexOf(Path.DirectorySeparatorChar) + 1);
                if (fileName.Contains("release") && !fileName.Contains("meta"))
                {
                    installedVersion = fileName.Substring(fileName.IndexOf("mdslib-") + 7, fileName.Length - 7 - 4);
                    break;
                }
            }
        }
        catch (Exception)
        {
            MDSxUpdateEditorWindow.UpdateAvailable = new MDSxUpdateAvailable() { IsAvailable = false, Message = "Movesense plugin folder not found", ErrorType = MDSxErrorType.Error };
            return;
        }

        if (installedVersion == String.Empty)
        {
            MDSxUpdateEditorWindow.UpdateAvailable = new MDSxUpdateAvailable() { IsAvailable = true, Message = "Version not found", ErrorType = MDSxErrorType.Warn };
        }

        if (string.Equals(serverVersion, installedVersion))
        {
            isUpdating.Remove(BuildTarget.iOS);
            MDSxUpdateEditorWindow.UpdateAvailable = new MDSxUpdateAvailable() { IsAvailable = false, Message = "Mds libraries are up to date", ErrorType = MDSxErrorType.Info };
            return;
        }

        MDSxUpdateEditorWindow.UpdateAvailable = new MDSxUpdateAvailable() { IsAvailable = true, Message = "Version is outdated", ErrorType = MDSxErrorType.Warn };
    }

    public static async void UpdateMovesenseLibrary()
    {
        MDSxUpdateEditorWindow.UpdateAvailable = new MDSxUpdateAvailable() { IsAvailable = true, Message = "Updating Movesense library. DO NOT QUIT UNITY", ErrorType = MDSxErrorType.Warn };
        
        Download(MDSxConstants.iOSFilePath, BuildTarget.iOS, string.Empty);

        isUpdating.Add(BuildTarget.Android);

        HttpResponseMessage response = await client.GetAsync(MDSxConstants.AndroidFileBucket);
        response.EnsureSuccessStatusCode();
        string responseBody = await response.Content.ReadAsStringAsync();

        MDSxServerVersion json = JsonUtility.FromJson<MDSxServerVersion>(responseBody);
        string androidServerVersionPath = json.values[0].path;

        var paths = androidServerVersionPath.Split('/');
        string androidServerFileName = paths[paths.Length - 1];

        Uri serverVersionAndroid = new Uri(MDSxConstants.ServerVersionBasePath + androidServerVersionPath);

        Download(serverVersionAndroid, BuildTarget.Android, androidServerFileName);
    }

    private static void Download(Uri uri, BuildTarget target, string serverFileName)
    {
        MDSxUpdateEditorWindow.UpdateProgress = new MDSxUpdateProgress() { Message = $"Downloading Movesense library for {target}...", ErrorType = MDSxErrorType.Info };

        var pathFileName = MDSxPathHelper.GetFilePath(target);
        string absTargetPath = string.Empty;
        string absDeletePath = string.Empty;
        string assetTargetPath = string.Empty;

        switch (target)
        {
            case BuildTarget.iOS:
                if (pathFileName.Item3 == string.Empty) // defaultPath
                {
                    absTargetPath = Path.Combine(pathFileName.Item1, pathFileName.Item2);
                    assetTargetPath = pathFileName.Item2;
                }
                else
                {
                    absTargetPath = Path.Combine(pathFileName.Item1, pathFileName.Item2, pathFileName.Item3);
                    assetTargetPath = Path.Combine(pathFileName.Item2, pathFileName.Item3);
                }
                break;
            case BuildTarget.Android:
                absTargetPath = Path.Combine(pathFileName.Item1, pathFileName.Item2, serverFileName);
                absDeletePath = pathFileName.Item3 == string.Empty ? string.Empty : Path.Combine(pathFileName.Item1, pathFileName.Item2, pathFileName.Item3);
                absDeletePath = absDeletePath == absTargetPath ? string.Empty : absDeletePath;
                break;
        }


        WebClient webClient = new WebClient();
        webClient.DownloadProgressChanged += (sender, e)  => DownloadProgressCallback(sender, e, target);

        webClient.DownloadFileCompleted += (sender, e) => DownloadFileCompleted(sender, e, target, assetTargetPath, absDeletePath);
        webClient.DownloadFileAsync(uri, absTargetPath);
    }
    private static void DownloadProgressCallback(object sender, DownloadProgressChangedEventArgs e, BuildTarget target)
    {
        MDSxUpdateEditorWindow.UpdateProgress = new MDSxUpdateProgress() { Message = $"Downloading Movesense library for {target}...", EndVal = (float)e.TotalBytesToReceive, Progress = (float)e.BytesReceived, ErrorType = MDSxErrorType.Info };
    }

    private static void DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e, BuildTarget target, string assetTargetPath, string absDeletePath)
    {
        if (e.Error == null)
        {
            if (target == BuildTarget.iOS)
            {
                MDSxUpdateEditorWindow.UpdateProgress = new MDSxUpdateProgress() { Message = "downloaded Movesense library for iOS", ErrorType = MDSxErrorType.Info };
                AssetDatabase.Refresh();
            }
            else if (target == BuildTarget.Android)
            {
                MDSxUpdateEditorWindow.UpdateProgress = new MDSxUpdateProgress() { Message = "downloaded Movesense library for Android", ErrorType = MDSxErrorType.Info };
                if (absDeletePath != string.Empty)
                {
                    File.Delete(absDeletePath);
                    File.Delete(absDeletePath + ".meta");
                }
                AssetDatabase.Refresh();
            }
        }
        else
        {
            Debug.LogError($"Error on downloading {target}. Error: {e.Error}");
            MDSxUpdateEditorWindow.UpdateProgress = new MDSxUpdateProgress() { Message = $"Error on downloading {target}. Error: {e.Error}", ErrorType = MDSxErrorType.Error };
        }

        isUpdating.Remove(target);

        if (isUpdating.Count == 0)
        {
            MDSxUpdateEditorWindow.UpdateAvailable = new MDSxUpdateAvailable() { IsAvailable = true, Message = "Merging gradle files", ErrorType = MDSxErrorType.Info };

            try
            {
                PluginImporter iOSPlugin = AssetImporter.GetAtPath(assetTargetPath) as PluginImporter;
                iOSPlugin.SetCompatibleWithAnyPlatform(false);
                iOSPlugin.SetCompatibleWithPlatform(BuildTarget.iOS, true);

                AssetDatabase.Refresh();
            }
            catch (System.Exception)
            {
                Debug.LogError("setting platform for libmds.a failed. Set it manually to iOS only!");
            }

            MDSxUpdateEditorWindow.UpdateAvailable = new MDSxUpdateAvailable() { IsAvailable = false, Message = "Movesense libraries are updated", ErrorType = MDSxErrorType.Info };
            Debug.Log("Movesense libraries are updated");
        }
        AssetDatabase.SaveAssets();
    }
}