
using System;
using System.IO;
using UnityEditor;
using UnityEngine;
///-------------------------------------------------------------------
///
///
///   Description:    Path Helper functions for Unity MDS Updater
///   Author:         Alex Kern, Vincent Uhlmann                  
///  
///
///-------------------------------------------------------------------
public static class MDSxPathHelper
{
    public static string Combine(params string[] paths)
    {
        string combined = "";
        foreach (var path in paths)
        {
            combined = Path.Combine(combined, path);
        }
        return combined;
    }

    public static Tuple<string, string, string> GetFilePath(BuildTarget target)
    {
        string[] assets = new string[0];
        string compareString = string.Empty;
        string assetpath = string.Empty;
        string defaultPath = string.Empty;

        switch (target)
        {
            case BuildTarget.iOS:
                assets = AssetDatabase.FindAssets("libmds");
                compareString = "libmds.a";
                defaultPath = Path.Combine("Assets", "Movesense Plugin", "Scripts", "Movesense", "iOS", "libmds.a");
                break;
            case BuildTarget.Android:
                assets = AssetDatabase.FindAssets("mdslib");
                compareString = "release.aar";
                defaultPath = Path.Combine("Assets", "Movesense Plugin", "Scripts", "Movesense", "Android");
                break;
        }

        string projectPath = Application.dataPath;
        projectPath = projectPath.Remove(projectPath.Length - 7); // remove Assets/

        if (assets != null)
        {
            foreach (string file in assets)
            {
                var filePath = AssetDatabase.GUIDToAssetPath(file);

                if (filePath.Contains(compareString))
                {
                    var filePathSplitPosition = filePath.LastIndexOf("/");
                    if (filePathSplitPosition == -1)
                        filePathSplitPosition = filePath.LastIndexOf("\\");
                    
                    assetpath = filePath.Substring(0, filePathSplitPosition);
                    
                    var fileName = filePath.Substring(filePathSplitPosition + 1, filePath.Length - filePathSplitPosition - 1);

                    return new Tuple<string, string, string>(projectPath, assetpath, fileName);
                }
            }
        }

        return new Tuple<string, string, string>(projectPath, defaultPath, string.Empty);
    }
}

