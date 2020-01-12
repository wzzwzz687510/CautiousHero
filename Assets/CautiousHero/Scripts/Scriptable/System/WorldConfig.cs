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

    [CreateAssetMenu(fileName = "WorldConfig", menuName = "Wing/Configs/WorldConfig", order = 0)]
    public class WorldConfig : ScriptableObject
    {
        public string configName;
        public string description;
        public int Hash => configName.GetStableHashCode();

        public AdventureStage[] stages;
        public RandomPool randomPool;

        public int[] RandomBattleSkill(int number)
        {
            int[] skillHashes = new int[number];
            int length = randomPool.battle_skillSet.commonSet.defaultSet.Length;
            if (number > length) return null;
            HashSet<int> ids = new HashSet<int>();
            for (int i = 0; i < number; i++) {
                int r = length.Random();
                while (ids.Contains(r)) {
                    r = length.Random();
                }
                ids.Add(r);
                skillHashes[i] = randomPool.battle_skillSet.commonSet.defaultSet[r].Hash;
            }
            return skillHashes;
        }

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