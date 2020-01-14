using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Wing.RPGSystem
{
    [CreateAssetMenu(fileName = "Class", menuName = "Wing/Scriptable Characters/TClass", order = 6)]
    public class TClass : ScriptableObject
    {
        public string className;
        public BaseRelic relic;
        public BaseSkill[] skillSet;
        public int Hash => className.GetStableHashCode();

        static Dictionary<int, TClass> cache;
        public static Dictionary<int, TClass> Dict {
            get {
                // load if not loaded yet
                return cache ?? (cache = Resources.LoadAll<TClass>("Character").ToDictionary(
                    item => item.Hash, item => item)
                );
            }
        }
    }
}

