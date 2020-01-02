using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Wing.RPGSystem
{
    [CustomEditor(typeof(SubAreaPrefabTool))]
    public class SubAreaTemplateDrawer : Editor
    {
        public int[,] coordinateValues;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            SubAreaPrefabTool t = (SubAreaPrefabTool)target;
            coordinateValues = new int[8, 8];

            for (int y = 0; y < 8; y++) {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField((7-y).ToString(), new GUILayoutOption[] { GUILayout.Width(20) });
                for (int x = 0; x < 8; x++) {
                    EditorGUILayout.IntField(coordinateValues[x, 7-y], new GUILayoutOption[] { GUILayout.MinWidth(40) });
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
            if (GUILayout.Button("Create")) {
                t.Button_CreateAsset(coordinateValues);
            }
        }
    }
}