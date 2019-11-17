using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wing.RPGSystem
{
    [System.Serializable]
    public struct EntityAttribute
    {
        public int level;
        public int maxHealth;
        public int action;
        public int strength;
        public int intelligence;
        public int agility;
        public int moveCost;

        public EntityAttribute(int lvl, int maxHp, int act, int str, int inte, int agi,int mvCost)
        {
            level = lvl;
            maxHealth = maxHp;
            action = act;
            strength = str;
            intelligence = inte;
            agility = agi;
            moveCost = mvCost;
        }

        public static EntityAttribute operator -(EntityAttribute a) =>
            new EntityAttribute(-a.level, -a.maxHealth, -a.action, -a.strength, -a.intelligence, -a.agility,-a.moveCost);
        public static EntityAttribute operator +(EntityAttribute a, EntityAttribute b) =>
            new EntityAttribute(a.level + b.level, a.maxHealth + b.maxHealth, a.action + b.action,
                a.strength + b.strength, a.intelligence + b.intelligence, a.agility + b.agility, a.moveCost + b.moveCost);
        public static EntityAttribute operator -(EntityAttribute a, EntityAttribute b) => a + -(b);
    }

    [CreateAssetMenu(fileName = "Creature", menuName = "Wing/ScriptableCreatures/BaseCreature", order = 1)]
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

