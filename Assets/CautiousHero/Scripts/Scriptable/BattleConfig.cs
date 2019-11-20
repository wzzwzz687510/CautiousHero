using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Wing.RPGSystem
{
    [System.Serializable]
    public struct CreatureSet
    {
        public BaseCreature tCreature;
        public Location location;
    }
    [System.Serializable]
    public struct AbioticSet
    {
        public BaseAbiotic tAbiotic;
        public int power;
    }

    [CreateAssetMenu(fileName = "Battle", menuName = "Wing/BattleConfig", order = 0)]
    public class BattleConfig : ScriptableObject
    {
        public string setName = "New battle set";
        public string description = "A standard battle set";
        public CreatureSet[] creatureSets;
        public AbioticSet[] abioticSets;
        [Range(0, 1)] public float coverage;
    }
}


