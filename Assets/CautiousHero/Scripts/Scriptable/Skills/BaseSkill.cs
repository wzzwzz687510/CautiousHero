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
        HardControl,
        SoftControl,
        Obstacle,
        DefenseBuff,
        StrengthenBuff,
        Healing,
        Damage,
        SuicideAttack
    }

    [System.Serializable]
    public struct EffectPattern
    {
        public Location pattern;
        public float coefficient;

    }
    [System.Serializable]
    public struct CastEffect
    {
        public GameObject prefab;
        public float animDuration;
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
        public List<Label> labels;
        //public HashSet<Label> hashlabels;

        [Header("Point Pattern")]
        public Location[] castPatterns;
        public CastEffect castEffect;
        public EffectPattern[] effectPatterns;

        public abstract Location GetFixedEffectPattern(Location cp, Location ep);
        public abstract IEnumerable<Location> GetFixedEffectPatterns(Location cp);
        public abstract IEnumerable<Location> GetSubEffectZone(Location casterLoc, Location cp, bool includingPassLocation = false);
        public abstract IEnumerable<Location> GetEffectZone(Location casterLoc, bool includingPassLocation = false);
        public abstract void ApplyEffect(Entity castEntity, Location castLoc);
    }    
}