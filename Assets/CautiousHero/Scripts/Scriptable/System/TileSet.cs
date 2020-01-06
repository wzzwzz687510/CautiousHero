using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wing.RPGSystem
{
    [CreateAssetMenu(fileName = "Tile set", menuName = "Wing/Configs/TileSet", order = 1)]
    public class TileSet : ScriptableObject
    {
        public TTile defaultTile;
        public TTile entranceTile;
        public TTile[] tTiles;
    }
}

