using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Wing.RPGSystem
{
    [System.Serializable]
    public struct AdventureStage
    {
        public int complexity;// 0-100
        public int stageLength;
        public int creatureNumber;
        public int treatureNumber;
        public AreaConfig mainConfig;
        public AreaConfig bossConfig;
        public int mainAreaNumber;
        public AreaConfig[] specialConfigs;
    }

    [CreateAssetMenu(fileName = "WorldConfig", menuName = "Wing/Configs/WorldConfig", order = 2)]
    public class WorldConfig : ScriptableObject
    {
        public string configName;
        public string description;
        public int Hash => configName.GetStableHashCode();

        public AdventureStage[] stages;

        static Dictionary<int, WorldConfig> cache;
        public static Dictionary<int, WorldConfig> Dict {
            get {
                // load if not loaded yet
                return cache ?? (cache = Resources.LoadAll<WorldConfig>("WorldConfigs").ToDictionary(
                    item => item.configName.GetStableHashCode(), item => item)
                );
            }
        }
    }
}