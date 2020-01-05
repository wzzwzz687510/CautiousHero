using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Wing.RPGSystem
{ 
    [CreateAssetMenu(fileName = "Creature", menuName = "Wing/Scriptable Creatures/BaseCreature", order = 30)]
    public class BaseCreature : ScriptableObject
    {
        public string creatureName;
        public string description;
        public int Hash => creatureName.GetStableHashCode();
        public Sprite sprite;
        public EntityAttribute attribute;

        public BaseSkill[] skills;
        public List<BaseBuff> buffs;

        static Dictionary<int, BaseCreature> cache;
        public static Dictionary<int, BaseCreature> Dict {
            get {
                // load if not loaded yet
                return cache ?? (cache = Resources.LoadAll<BaseCreature>("Creatures").ToDictionary(
                    item => item.creatureName.GetStableHashCode(), item => item)
                );
            }
        }
    }
}

