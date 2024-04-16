/*
    Created by Unity Adventure
    Copyright (C) 2023 Unity Adventure. All Rights Reserved.
*/
using UnityEditor;

[CustomEditor(typeof(DrawingManager))]
public class DrawingManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        EditorGUILayout.HelpBox("Unity Adventure \n - Right Click to Draw \n - Left Click to Erase \n - C to clean", MessageType.Info);

        DrawDefaultInspector();
    }
}
