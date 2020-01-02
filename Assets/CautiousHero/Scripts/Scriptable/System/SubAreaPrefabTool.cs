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

        public void Button_CreateAsset(int[,] values)
        {
            var asset = ScriptableObject.CreateInstance<SubArea>();
            asset.coordinateValues = values;
            asset.type = type;

            string path = "Assets/Resources/SubAreas/";
            if(Directory.Exists(path))
            AssetDatabase.CreateAsset(asset, path + templateName + ".asset");
            AssetDatabase.SaveAssets();
        }


    }

}
