using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Wing.RPGSystem
{
    [System.Serializable]
    public struct CreatureElement
    {
        public BaseCreature tCreature;
        public Location pattern;
    }

    [CreateAssetMenu(fileName = "Creature Set", menuName = "Wing/Configs/CreatureSet", order = 1)]
    public class CreatureSet : ScriptableObject
    {
        public string setName;
        public string description;
        public int Hash => setName.GetStableHashCode();
        public  CreatureElement[] creatures;

        public int difficulty;
        public int coin;
        public int exp;
        public BaseLoot loot;

        static Dictionary<int, CreatureSet> cache;
        public static Dictionary<int, CreatureSet> Dict {
            get {
                return cache ?? (cache = Resources.LoadAll<CreatureSet>("CreatureSet").ToDictionary(
                    item => item.setName.GetStableHashCode(), item => item)
                    );
            }
        }
    }
}

