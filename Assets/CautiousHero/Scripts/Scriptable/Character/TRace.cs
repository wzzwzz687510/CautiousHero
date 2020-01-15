using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Wing.RPGSystem
{
    [CreateAssetMenu(fileName = "Race", menuName = "Wing/Scriptable Characters/TRace", order = 7)]
    public class TRace : ScriptableObject
    {
        public string raceName;
        public string description;
        public BaseRelic relic;
        public BaseSkill skill;
        public EntityAttribute attribute;
        public int Hash => raceName.GetStableHashCode();

        static Dictionary<int, TRace> cache;
        public static Dictionary<int, TRace> Dict {
            get {
                // load if not loaded yet
                return cache ?? (cache = Resources.LoadAll<TRace>("Character").ToDictionary(
                    item => item.Hash, item => item)
                );
            }
        }
    }
}
