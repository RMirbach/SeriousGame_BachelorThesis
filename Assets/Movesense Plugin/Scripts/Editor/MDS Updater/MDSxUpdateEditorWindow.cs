///-------------------------------------------------------------------
///
///
///   Description:    Main EditorWindow for Unity Mds Updater
///   Author:         Alex Kern, Vincent Uhlmann                  
///  
///
///-------------------------------------------------------------------

using UnityEditor;
using UnityEngine;

public class MDSxUpdateEditorWindow : EditorWindow
{
    public static MDSxUpdateAvailable UpdateAvailable = new MDSxUpdateAvailable();
    public static MDSxUpdateProgress UpdateProgress = new MDSxUpdateProgress();
    private bool isUpdating = false;

    void OnGUI()
    {
        if (UpdateAvailable.IsAvailable)
        {
            GUILayout.Label(UpdateAvailable.Message);
            // TODO: Button mit Update Ja/Nein und wenn ja dann
            if (!isUpdating && GUI.Button(new Rect(10, 25, 150, 20), "Update"))
            {
                isUpdating = true;
                MDSxUpdater.UpdateMovesenseLibrary();
            }

            if (UpdateProgress.Progress < UpdateProgress.EndVal)
            {
                EditorUtility.DisplayProgressBar("Progress Bar", $"{UpdateProgress.Message}", UpdateProgress.Progress / UpdateProgress.EndVal);
                GUILayout.Label(UpdateProgress.Message);
            }
            else
            {
                EditorUtility.ClearProgressBar();
                GUILayout.Label(UpdateProgress.Message);
            }    
        }
        else
        {
            EditorUtility.ClearProgressBar();
            GUILayout.Label(UpdateAvailable.Message);
            isUpdating = false;
        }
    }

    void OnInspectorUpdate()
    {
        Repaint();
    }
}