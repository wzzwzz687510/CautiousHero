using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wing.RPGSystem
{
    [System.Serializable]
    public struct CreatureElement
    {
        public BaseCreature tCreature;
        public Location pattern;
    }

    [CreateAssetMenu(fileName = "Creature Set", menuName = "Wing/Config/CreatureSet", order = 1)]
    public class CreatureSet : ScriptableObject
    {
        public  CreatureElement[] creatures;

        public int coin;
        public int exp;
        public BaseLoot loot;
    }
}

