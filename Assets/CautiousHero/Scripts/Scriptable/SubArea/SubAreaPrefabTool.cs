using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;


namespace Wing.RPGSystem
{
    public class SubAreaPrefabTool : MonoBehaviour
    {
        public string templateName;
        public SubAreaType type;
        public AreaConfig a;
        [HideInInspector]public int[] values = new int[64];
        readonly string path = "Assets/Resources/SubAreas/";

        public void Button_CreateAsset()
        {
            switch (type) {
                case SubAreaType.Corner:
                    var corner = ScriptableObject.CreateInstance<CornerArea>();
                    Save(corner);
                    break;
                case SubAreaType.VerticalEdge:
                    var vEdge = ScriptableObject.CreateInstance<VEdgeArea>();
                    Save(vEdge);
                    break;
                case SubAreaType.HorizontalEdge:
                    var hEdge = ScriptableObject.CreateInstance<HEdgeArea>();
                    Save(hEdge);
                    break;
                case SubAreaType.Centre:
                    var centre = ScriptableObject.CreateInstance<CentreArea>();
                    Save(centre);
                    break;
            }
        }

        private void Save(SubArea asset)
        {
            asset.SetValues(values);
            asset.SetType(type);
        
            if (!Directory.Exists(path)) return;
            AssetDatabase.CreateAsset(asset, path + templateName + ".asset");
            AssetDatabase.SaveAssets();
            EditorUtility.FocusProjectWindow();
            //Selection.activeObject = asset;
        }

        public void LoadTest()
        {
            switch (type) {
                case SubAreaType.Corner:
                    var corner = Resources.Load<CornerArea>("SubAreas/" + templateName);
                    corner.coordinateValues.CopyTo(values, 0);
                    break;
                case SubAreaType.VerticalEdge:
                    var vEdge = Resources.Load<VEdgeArea>("SubAreas/" + templateName);
                    vEdge.coordinateValues.CopyTo(values, 0);
                    break;
                case SubAreaType.HorizontalEdge:
                    var hEdge = Resources.Load<HEdgeArea>("SubAreas/" + templateName);
                    hEdge.coordinateValues.CopyTo(values, 0);
                    break;
                case SubAreaType.Centre:
                    var centre = Resources.Load<CentreArea>("SubAreas/" + templateName);
                    centre.coordinateValues.CopyTo(values, 0);
                    Debug.Log(values[0]);
                    break;
                default:
                    break;
            }
        }


    }

}
