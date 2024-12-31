///-------------------------------------------------------------------
///
///
///   Description:    Constants for Unity MDS Updater
///   Author:         Alex Kern, Vincent Uhlmann             
///  
///
///-------------------------------------------------------------------

using System;
using System.Text.RegularExpressions;
using UnityEngine;

public static class MDSxConstants {
    // Uris
    public static readonly Uri AndroidDependencyFilePath = new Uri("https://bitbucket.org/movesense/movesense-mobile-lib/raw/master/android/AndroidGradle.json");
    
    public static readonly Uri AndroidFileBucket = new Uri("https://bitbucket.org/!api/internal/repositories/movesense/movesense-mobile-lib/srcdir-with-metadata/master/android/Movesense");
    public static readonly Uri iOSFilePath = new Uri("https://bitbucket.org/movesense/movesense-mobile-lib/raw/master/IOS/Movesense/Release-iphoneos/libmds.a");
    public static readonly Uri ServerVersionBasePath = new Uri("https://bitbucket.org/movesense/movesense-mobile-lib/raw/master/");

    // Paths
    public readonly static string MOVESENSE_PLUGIN_DIRECTORY = MDSxPathHelper.Combine(Application.dataPath, "Movesense Plugin", "Scripts", "Movesense");
    public readonly static string GRADLE_DEST_DIRECTORY = MDSxPathHelper.Combine(Application.dataPath, "Plugins", "Android");
    public const string GRADLE_DEST_FILE = "mainTemplate.gradle";
    public const string GRADLE_PROP_DEST_FILE = "gradleTemplate.properties";

    public readonly static string GRADLE_BASE_DIRECTORY = MDSxPathHelper.Combine(Application.dataPath, "Movesense Plugin", "Scripts", "Editor");
    public const string GRADLE_BASE_FILE = "gradleBase.file";
    public const string GRADLE_PROP_BASE_FILE = "propertiesBase.file";

    // Regex
    public static readonly Regex VersionRegex = new Regex(@"^##\s*Version\s*(\d+\.\d+\.\d+)\D", RegexOptions.Multiline);

    
}