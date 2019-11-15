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

    public enum AdditiveAttribute
    {
        Strength,
        Intelligence,
        Agility
    }

    public enum Label
    {
        HardMoveControll,
        SoftMoveControll,
        Obstacle,
        Debuff,
        SuicideAttack
    }

    [System.Serializable]
    public struct SkillPattern
    {
        public Location loc;
        public float coefficient;
    }

    //[CreateAssetMenu(fileName = "Skill", menuName = "ScriptableSkills/BaseSkill", order = 1)]
    public abstract class BaseSkill : ScriptableObject
    {
        public string skillName = "New Skill";
        public string description = "A mystical skill";
        public Sprite sprite;
        public DamageType damageType;
        public DamageElement damageElement;
        public AdditiveAttribute attribute;
        public SkillType skillType;
        public Label[] labels;

        public int cooldownTile;
        public Location[] castPoints;
        public SkillPattern[] affectPattern;

        public abstract IEnumerable<Location> AffectPoints();
        public abstract void ApplyEffect(Entity castEntity, Entity targetEntity, int patternID);
    }

    [CreateAssetMenu(fileName = "Skill", menuName = "ScriptableSkills/BasicAttackSkill", order = 1)]
    public class BasicAttackSkill : BaseSkill
    {
        // Final number = baseValue * (1 + levelCof * level + attributeCof * attribute)
        public int baseValue;
        public float levelCof;
        public float attributeCof;

        public override IEnumerable<Location> AffectPoints()
        {
            foreach (var point in affectPattern) {
                yield return point.loc;
            }
        }

        public override void ApplyEffect(Entity castEntity, Entity targetEntity, int patternID)
        {
            int aa = 0;
            if (attribute == AdditiveAttribute.Agility)
                aa = castEntity.Agility;
            else if (attribute == AdditiveAttribute.Intelligence)
                aa = castEntity.Intelligence;
            else if (attribute == AdditiveAttribute.Strength)
                aa = castEntity.Strength;

            targetEntity.DealDamage(Mathf.RoundToInt(baseValue * (1 + levelCof * levelCof + attributeCof * aa)));
        }
    }
}