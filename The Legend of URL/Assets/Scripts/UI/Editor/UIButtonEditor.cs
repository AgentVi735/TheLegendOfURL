using UnityEngine;
using UnityEditor;
using UnityEditor.UI;

[CustomEditor(typeof(UIButton))]
public class UIButtonEditor : ButtonEditor
{
    private SerializedProperty textProp;
    private SerializedProperty defaultMatProp;
    private SerializedProperty hoverMatProp;
    private SerializedProperty matLerpTimeProp;

    protected override void OnEnable()
    {
        base.OnEnable();
        textProp = serializedObject.FindProperty("_text");
        defaultMatProp = serializedObject.FindProperty("_defaultMat");
        hoverMatProp = serializedObject.FindProperty("_hoverMat");
        matLerpTimeProp = serializedObject.FindProperty("_matLerpTime");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        if (textProp != null)
            EditorGUILayout.PropertyField(textProp, new GUIContent("Text"));
        if (defaultMatProp != null)
            EditorGUILayout.PropertyField(defaultMatProp, new GUIContent("Default Mat"));
        if (hoverMatProp != null)
            EditorGUILayout.PropertyField(hoverMatProp, new GUIContent("Hover Mat"));
        if (matLerpTimeProp != null)
            EditorGUILayout.PropertyField(matLerpTimeProp, new GUIContent("Mat Lerp Time"));

        EditorGUILayout.Space();
        
        serializedObject.ApplyModifiedProperties();
        base.OnInspectorGUI();
    }
}