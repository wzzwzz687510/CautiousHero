﻿using System.Collections;
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

    [System.Serializable]
    public struct SubAreaArray
    {
        public int[] values;
    }

    //[CreateAssetMenu(fileName = "AreaConfig", menuName = "Wing/SubAreaPrefab", order = 2)]
    public class SubArea : ScriptableObject
    {
        public int[] coordinateValues;
        public SubAreaType type;

        public virtual void SetValues(int[] values)
        {
            coordinateValues = new int[64];
            values.CopyTo(coordinateValues,0);
        }

        public void SetType(SubAreaType type)
        {
            this.type = type;
        }

        public int[] GetInverseValues(bool horizontal, bool vertical)
        {
            int[] res = new int[64];
            for (int x = 0; x < 8; x++) {
                for (int y = 0; y < 8; y++) {
                    int xx = horizontal ? 7 - x : x;
                    int yy = vertical ? 7 - y : y;
                    res[x + 8 * y] = coordinateValues[xx + 8 * yy];
                }
            }
            return res;
        }
    }
}

