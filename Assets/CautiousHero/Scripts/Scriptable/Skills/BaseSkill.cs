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
        None,
        Fire,
        Water,
        Earth,
        Air,
        Light,
        Dark
    }

    public enum CastType
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
        [Header("Basic Parameters")]
        public string skillName = "New skill";
        public string description = "A mystical skill";
        public Sprite sprite;
        public int cooldownTime;
        public int actionPointsCost;

        [Header("Labels")]
        public DamageType damageType;
        public DamageElement damageElement;
        public CastType castType;
        public Label[] labels;

        [Header("Point Pattern")]
        public Location[] castPatterns;
        public SkillPattern[] effectPatterns;

        public abstract IEnumerable<Location> GetFixedEffectPattern(Location castPattern);
        public abstract IEnumerable<Location> EffectZone(Location origin);
        public abstract void ApplyEffect(Entity castEntity, Entity targetEntity, int patternID);
    }    
}