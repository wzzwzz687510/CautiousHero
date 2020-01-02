using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wing.RPGSystem
{
    public enum SubAreaType
    {
        Corner,
        VerticalEdge,
        HorizontalEdge,
        Centre
    }

    //[CreateAssetMenu(fileName = "AreaConfig", menuName = "Wing/SubAreaPrefab", order = 2)]
    public class SubArea : ScriptableObject
    {
        public int[,] coordinateValues;
        public SubAreaType type;

        public int GetTileType(Location coord)
        {
            return coordinateValues[coord.x, coord.y];
        }

        public int[,] GetInverseValues(bool horizontal,bool vertical)
        {
            int[,] res = new int[8,8];
            for (int x = 0; x < 8; x++) {
                for (int y = 0; y < 8; y++) {
                    res[x, y] = coordinateValues[horizontal ? 7 - x : x, vertical ? 7 - y : y];
                }
            }
            return res;
        }
    }
}

