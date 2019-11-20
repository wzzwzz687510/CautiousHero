using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wing.RPGSystem
{ 
    [CreateAssetMenu(fileName = "Creature", menuName = "Wing/Scriptable Creatures/BaseCreature", order = 1)]
    public class BaseCreature : ScriptableObject
    {
        public string creatureName = "New Creature";
        public string description = "A secret creature";
        public Sprite sprite;
        public EntityAttribute attribute;

        public BaseSkill[] skills;
        public List<BaseBuff> buffs;
    }
}

