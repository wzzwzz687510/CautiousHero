using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wing.RPGSystem
{
    [CreateAssetMenu(fileName = "Creature Set", menuName = "Wing/Configs/CreatureSetDifficulty", order = 2)]
    public class CreatureSetDifficulty : ScriptableObject
    {
        public List<CreatureSet> standardSets;
        public List<CreatureSet> hardSets;

        private int difficultyTracker = 0;

        public CreatureSet GetGivenDifficultySet()
        {
            return cache[difficultyTracker][cache[difficultyTracker].Count.Random()];
        }

        public bool ChangeDifficulty(bool isIncrease)
        {
            difficultyTracker += isIncrease ? 1 : -1;
            for (int i = 0; i < 10; i++) {
                if (cache.ContainsKey(difficultyTracker)) return true;
                difficultyTracker += isIncrease ? 1 : -1;
            }
            return false;
        }

        public CreatureSet GetRandomSet(bool isHardSet)
        {
            return isHardSet ? hardSets[hardSets.Count.Random()]: standardSets[standardSets.Count.Random()];
        }

        Dictionary<int, List<CreatureSet>> cache;
        public Dictionary<int, List<CreatureSet>> Dict {
            get {
                if (cache == null) {
                    cache = new Dictionary<int, List<CreatureSet>>();                    
                    foreach (var set in standardSets) {
                        if (cache.ContainsKey(set.difficulty)) cache[set.difficulty].Add(set);
                        else cache.Add(set.difficulty, new List<CreatureSet>() { set });
                    }
                }
                return cache;
            }
        }
    }
}

