using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Wing.RPGSystem
{
    [CustomEditor(typeof(SubAreaPrefabTool))]
    public class SubAreaTemplateDrawer : Editor
    {
        SerializedProperty sp;

        private void OnEnable()
        {
            sp = serializedObject.FindProperty("values");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            serializedObject.Update();
            SubAreaPrefabTool t = (SubAreaPrefabTool)target;
            
            for (int y = 0; y < 8; y++) {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField((7-y).ToString(), new GUILayoutOption[] { GUILayout.Width(20) });
                for (int x = 0; x < 8; x++) {
                    EditorGUI.BeginChangeCheck();
                    int inputNumber = EditorGUILayout.IntField(sp.GetArrayElementAtIndex(x+8*(7-y)).intValue, new GUILayoutOption[] { GUILayout.MinWidth(40) });
                    if (EditorGUI.EndChangeCheck()) {
                        sp.GetArrayElementAtIndex(x + 8 * (7 - y)).intValue = inputNumber;
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("x,y", new GUILayoutOption[] { GUILayout.Width(20) });
                for (int x = 0; x < 8; x++) {
                    EditorGUILayout.LabelField(x.ToString(), new GUILayoutOption[] { GUILayout.MinWidth(40) });
                }
                EditorGUILayout.EndHorizontal();
            }

            serializedObject.ApplyModifiedProperties();
            if (GUILayout.Button("Save")) {
                t.Button_CreateAsset();
            }
            if (GUILayout.Button("Load")) {
                t.LoadTest();
            }
            
        }
    }
}