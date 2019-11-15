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
        public int maxAction;
        public int strength;
        public int intelligence;
        public int agility;

        public EntityAttribute(int lvl, int maxHp, int maxAct, int str, int inte, int agi)
        {
            level = lvl;
            maxHealth = maxHp;
            maxAction = maxAct;
            strength = str;
            intelligence = inte;
            agility = agi;
        }

        public static EntityAttribute operator -(EntityAttribute a) =>
            new EntityAttribute(-a.level, -a.maxHealth, -a.maxAction, -a.strength, -a.intelligence, -a.agility);
        public static EntityAttribute operator +(EntityAttribute a, EntityAttribute b) =>
            new EntityAttribute(a.level + b.level, a.maxHealth + b.maxHealth, a.maxAction + b.maxAction,
                a.strength + b.strength, a.intelligence + b.intelligence, a.agility + b.agility);
        public static EntityAttribute operator -(EntityAttribute a, EntityAttribute b) => a + -(b);
    }

    [CreateAssetMenu(fileName = "Creature", menuName = "ScriptableCreatures/BaseCreature", order = 1)]
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

