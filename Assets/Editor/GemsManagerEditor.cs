using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(GemsManager))]
public class GemsManagerEditor : Editor
{
    private bool isPreviewOn = false;
    private bool isDirty = false;
    private static GemsManagerEditor instance;

    void OnEnable()
    {
        EditorApplication.update += EditorUpdate;
        EditorApplication.playModeStateChanged += EditorPlayModeChange;
        instance = this;
    }

    void OnDisable()
    {
        EditorApplication.update -= EditorUpdate;
        EditorApplication.playModeStateChanged -= EditorPlayModeChange;

        if(!EditorApplication.isPlaying && isPreviewOn) {
            ((GemsManager)target).DeleteBoard();
        }
    }
    
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if(EditorApplication.isPlaying)  { return; }
        
        GUIStyle style = new GUIStyle();
        style.alignment = TextAnchor.MiddleCenter;
        style.fontStyle = FontStyle.Bold;

        GUILayout.Label("---- Debug Stuff ----", style);

        if(isPreviewOn) {
            if(GUILayout.Button("Stop preview")) {
                isPreviewOn = false;
                isDirty = true;
            }
        } else {
            if(GUILayout.Button("Start preview")) {
                isPreviewOn = true;
                isDirty = true;
            }
        }
    }

    void EditorUpdate()
    {
        if(isDirty) {
            if(isPreviewOn) {
                ((GemsManager)target).CreateBoard(1337);
            } else {
                ((GemsManager)target).DeleteBoard();
            }
            isDirty = false;
        } else if(isPreviewOn) {
            GemsManager manager = (GemsManager)target;
            if(manager.EditorIsDirty) {
                manager.DeleteBoard();
                manager.CreateBoard(1337);
                manager.EditorIsDirty = false;
            }
        }
    }

    void EditorPlayModeChange(PlayModeStateChange currentPlayMode)
    {
        if(currentPlayMode == PlayModeStateChange.ExitingEditMode) {
            if(isPreviewOn) {
                ((GemsManager)target).DeleteBoard();
            }
            isPreviewOn = false;
        }
    }

    [UnityEditor.Callbacks.DidReloadScripts]
    static void EditorDidReloadScripts()
    {
        if(instance) {
            instance.isDirty = true;
        }
    }
}
