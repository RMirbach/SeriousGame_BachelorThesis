using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Net;
using UnityEditor;
using UnityEngine;


[InitializeOnLoad]
public class GradleMerge
{
    private static MDSxAndroidDependency androidDependency;
    static GradleMerge()
    {
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

        if (!MergeGradleFile())
            {
                return;
            }

            MergeGradleProperties();
    }

    private static bool MergeGradleFile()
    {
        List<string> implementations = androidDependency.android.implementation;

        var txtLines = new List<string>();
        try
        {
            txtLines = File.ReadAllLines(MDSxPathHelper.Combine(MDSxConstants.GRADLE_BASE_DIRECTORY, MDSxConstants.GRADLE_BASE_FILE)).ToList();
        }
        catch (Exception)
        {
            Debug.LogError($"could not find {MDSxConstants.GRADLE_BASE_FILE}");
            return false;
        }

        int lineIndex = 0;
        foreach (var line in txtLines)
        {
            if (line.Contains("dependencies"))
            {
                break;
            }
            lineIndex++;
        }

        foreach (var implementation in implementations)
        {
            lineIndex++;
            txtLines.Insert(lineIndex, "\t" + "implementation " + "\"" + implementation + "\"");
        }

        var file = MDSxPathHelper.Combine(MDSxConstants.GRADLE_DEST_DIRECTORY, MDSxConstants.GRADLE_DEST_FILE);

        if (File.Exists(file))
        {
            File.Delete(file);
            File.Delete(file + ".meta");
        }
        else
        {
            if (!Directory.Exists(MDSxConstants.GRADLE_DEST_DIRECTORY))
            {
                Directory.CreateDirectory(MDSxConstants.GRADLE_DEST_DIRECTORY);
            }
            else
                Debug.Log("directory already exists");
        }

        File.WriteAllLines(file, txtLines);

        return true;
    }

    private static void MergeGradleProperties()
    {
        var txtLines = new List<string>();
        try
        {
            txtLines = File.ReadAllLines(MDSxPathHelper.Combine(MDSxConstants.GRADLE_BASE_DIRECTORY, MDSxConstants.GRADLE_PROP_BASE_FILE)).ToList();
        }
        catch (Exception)
        {
            Debug.LogError($"could not find {MDSxConstants.GRADLE_PROP_BASE_FILE}");
            return;
        }

        txtLines.Insert(0, "android.useAndroidX=true");

        var file = MDSxPathHelper.Combine(MDSxConstants.GRADLE_DEST_DIRECTORY, MDSxConstants.GRADLE_PROP_DEST_FILE);

        if (File.Exists(file))
        {
            File.Delete(file);
            File.Delete(file + ".meta");
        }

        File.WriteAllLines(file, txtLines);
    }
}
