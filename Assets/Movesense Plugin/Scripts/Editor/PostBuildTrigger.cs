#if UNITY_IOS
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEngine;

public static class PostBuildTrigger {
    [PostProcessBuild]
    public static void OnPostProcessBuild(BuildTarget target, string pathToBuiltProject) { 
        string projPath = pathToBuiltProject + "/Unity-iPhone.xcodeproj/project.pbxproj";
        PBXProject proj = new PBXProject();
        proj.ReadFromFile(projPath);
#if UNITY_2017_1_OR_NEWER
        string targetGUID = proj.ProjectGuid();
#else
        string targetGUID = proj.TargetGuidByName("Unity-iPhone");
#endif
        proj.AddBuildProperty(targetGUID, "OTHER_LDFLAGS", "-ObjC -lz");
        proj.WriteToFile(projPath);

        string plistPath = pathToBuiltProject + "/Info.plist";
        var plist = new PlistDocument();

        plist.ReadFromFile(plistPath);
        var rootDict = plist.root;

        rootDict.SetString("NSBluetoothAlwaysUsageDescription", "App needs Bluetooth to connect to Movesense devices");

        rootDict.CreateArray("UIBackgroundModes").AddString("bluetooth-central");

        plist.WriteToFile(plistPath);
    }
}
#endif
