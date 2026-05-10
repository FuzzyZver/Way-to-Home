using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(InteractionActor))]
public class TriggerActorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(
                    serializedObject.FindProperty("_actorType"));
        EditorGUILayout.PropertyField(
                    serializedObject.FindProperty("_transform"));
        EditorGUILayout.PropertyField(
                    serializedObject.FindProperty("_cameraTarget"));

        SerializedProperty eventType =
            serializedObject.FindProperty("_eventType");

        EditorGUILayout.PropertyField(eventType);

        EventType type = (EventType)eventType.enumValueIndex;

        switch (type)
        {
            case EventType.TextDialogue:
                EditorGUILayout.PropertyField(
                    serializedObject.FindProperty("_characterType"));
                break;

            case EventType.AudioClip:
                EditorGUILayout.PropertyField(
                    serializedObject.FindProperty("_audioClip"));
                break;

            case EventType.SpecialEvent:
                EditorGUILayout.PropertyField(
                    serializedObject.FindProperty("_specialEventId"));
                break;
        }

        serializedObject.ApplyModifiedProperties();
    }
}