using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wing.TileUtils;

namespace Wing.RPGSystem
{
    [System.Serializable]
    public struct CreatureSet
    {
        public BaseCreature tCreature;
        public Location location;
    }

    [CreateAssetMenu(fileName = "Battle", menuName = "Wing/BattleConfig", order = 0)]
    public class BattleConfig : ScriptableObject
    {
        public string setName = "New battle set";
        public string description = "A standard battle set";
        public CreatureSet[] creatureSets;
        public AbioticEntity[] abioticEntities;
    }
}


