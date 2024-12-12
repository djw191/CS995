using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Registerable))]
public class RegisterableEditor : Editor
{
        public override void OnInspectorGUI()
        {
                Registerable registerable = (Registerable)target;
                if (registerable.guid == "")
                {
                        SerializedProperty targetObjectProperty = serializedObject.FindProperty("guid");
                        
                        Guid guid = Guid.NewGuid();
        
                        registerable.guid = guid.ToString();
                        
                        serializedObject.ApplyModifiedProperties();
                        EditorUtility.SetDirty(target);
                }
        }
}