using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wing.TileUtils;

namespace Wing.RPGSystem
{
    [CreateAssetMenu(fileName = "Pattern", menuName = "Wing/Scriptable Patterns/Base Pattern", order = 1)]
    public class ScriptablePattern : ScriptableObject
    {
        public Location[] patterns;
    }
}

