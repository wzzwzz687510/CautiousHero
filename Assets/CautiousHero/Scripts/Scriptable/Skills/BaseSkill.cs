﻿using System.Collections;
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
        Healing,
        Damage,
        Debuff,
        SuicideAttack
    }

    [System.Serializable]
    public struct EffectPattern
    {
        public Location pattern;
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
        public HashSet<Label> labels;

        [Header("Point Pattern")]
        public Location[] castPatterns;
        public EffectPattern[] effectPatterns;

        public abstract Location GetFixedEffectPattern(Location cp, Location ep);
        public abstract IEnumerable<Location> GetFixedEffectPatterns(Location cp);
        public abstract IEnumerable<Location> SubEffectZone(Location casterLoc, Location cp, bool includingPassLocation = false);
        public abstract IEnumerable<Location> EffectZone(Location casterLoc, bool includingPassLocation = false);
        public abstract void ApplyEffect(Entity castEntity, Location castLoc);
    }    
}