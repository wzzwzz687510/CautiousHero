using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Wing.RPGSystem
{

    [System.Serializable]
    public struct AbioticElement
    {
        public BaseAbiotic tAbiotic;
        public int power;
    }

    public enum AreaType
    {
        Standard,
        Town,
        Puzzle,
        Secret,
        Boss
    }

    [CreateAssetMenu(fileName = "AreaConfig", menuName = "Wing/Configs/AreaConfig", order = 0)]
    public class AreaConfig : ScriptableObject
    {
        public string configName;
        public string description;
        public AreaType type;
        public Sprite sprite;
        public int Hash => configName.GetStableHashCode();
        public SubArea[] cornerAreas;
        public SubArea[] vEdgeAreas;
        public SubArea[] hEdgeAreas;
        public SubArea[] centreAreas;
        public CreatureSetDifficulty creatureSets;
        public AbioticElement[] abioticSets;

        static Dictionary<int, AreaConfig> cache;
        public static Dictionary<int, AreaConfig> Dict {
            get {
                // load if not loaded yet
                return cache ?? (cache = Resources.LoadAll<AreaConfig>("AreaConfigs").ToDictionary(
                    item => item.configName.GetStableHashCode(), item => item)
                );
            }
        }
    }
}


