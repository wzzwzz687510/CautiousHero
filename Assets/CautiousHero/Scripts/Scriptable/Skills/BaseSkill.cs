using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wing.TileUtils;

namespace Wing.RPGSystem
{
    public enum DamageType
    {
        Physical,
        Magical,
        Pure
    }

    public enum DamageElement
    {
        Fire,
        Water,
        Earth,
        Air,
        Light,
        Dark,
        None
    }

    public enum SkillType
    {
        Instant,
        Trajectory
    }

    [System.Serializable]
    public struct SkillPattern
    {
        Location loc;
        float coefficient;
    }

    [CreateAssetMenu(fileName = "Skill", menuName = "ScriptableSkills/BaseSkill", order = 1)]
    public class BaseSkill : ScriptableObject
    {
        public string skillName = "New Skill";
        public string description = "A mystical skill";
        public Sprite sprite;
        public DamageType damageType;
        public DamageElement damageElement;
        public SkillType skillType;     

        public int castCost;

        // Final number = baseValue * (1 + levelCof * level + attributeCof * attribute)
        public int baseValue;
        public float levelCof;
        public float attributeCof;
        public Location[] castPoints;
        public Location[] affectPattern;

        public virtual IEnumerable<Location> AffectPoints()
        {
            return affectPattern;
        }

        public virtual void ApplyEffect(Entity castEntity, Entity[] targetEntity)
        {

        }
    }

    [CreateAssetMenu(fileName = "Skill", menuName = "ScriptableSkills/TrajectorySkill", order = 2)]
    public class TrajectorySkill : BaseSkill
    {

    }
}