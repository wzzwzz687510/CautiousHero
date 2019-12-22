using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Wing.RPGSystem
{
    public enum BuffType
    {
        Attribute,
        Defense
    }

    public abstract class BaseBuff : ScriptableObject
    {
        public string buffName = "New buff";
        public string description = "None";
        public BuffType buffType;
        public int lastTurn;
        public bool infinity;
        public int Hash { get { return buffName.GetStableHashCode(); } }

        static Dictionary<int, BaseBuff> cache;
        public static Dictionary<int, BaseBuff> Dict {
            get {
                // load if not loaded yet
                return cache ?? (cache = Resources.LoadAll<BaseBuff>("Skills").ToDictionary(
                    item => item.buffName.GetStableHashCode(), item => item)
                );
            }
        }
    }
}

