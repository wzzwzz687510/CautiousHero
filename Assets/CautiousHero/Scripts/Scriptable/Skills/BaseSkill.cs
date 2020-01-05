﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Wing.RPGSystem
{
    public enum DamageType
    {
        Physical,
        Magical,
        Pure
    }

    public enum SkillElement
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
        SuicideAttack,
        Combo1st,
        Combo2rd,
        Combo3th,
        Ally
    }

    [System.Serializable]
    public struct EffectPattern
    {
        public Location loc;
        public float coefficient;
        public BaseBuff[] additionBuffs;
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
        public string skillName;
        public string description;
        public Sprite sprite;
        public int actionPointsCost;
        public int Hash => skillName.GetStableHashCode(); 

        [Header("Labels")]
        public DamageType damageType;
        public SkillElement skillElement;
        public CastType castType;
        public List<Label> labels;
        //public HashSet<Label> hashlabels;

        [Header("Point Pattern")]
        public CastEffect castEffect;
        public ScriptablePattern tCastPatterns;
        public ScriptableEffectPattern tEffectPatterns;
        public Location[] CastPattern => ScriptablePattern.Dict[Hash];
        public EffectPattern[] EffectPattern => tEffectPatterns.effectPatterns;

        public abstract void ApplyEffect(int casterHash, Location castLoc);

        public virtual Location GetFixedEffectPattern(Location cp, Location ep)
        {
            int x = cp.x;
            int y = cp.y;

            int xDir = 1, yDir = 1;
            bool flip = false;
            if (x >= 0 && y > 0) {

            }
            else if (x > 0 && y <= 0) {
                flip = true;
                xDir = -1;
            }
            else if (x <= 0 && y < 0) {
                xDir = -1;
                yDir = -1;
            }
            else if (x < 0 && y >= 0) {
                flip = true;
                yDir = -1;
            }

            if (flip) {
                return new Location(yDir * ep.y, xDir * ep.x);
            }
            else {
                return new Location(xDir * ep.x, yDir * ep.y);
            }
        }
        public virtual IEnumerable<Location> GetFixedEffectPatterns(Location cp)
        {
            int x = cp.x;
            int y = cp.y;

            int xDir = 1, yDir = 1;
            bool flip = false;
            if (x >= 0 && y > 0) {

            }
            else if (x > 0 && y <= 0) {
                flip = true;
                xDir = -1;
            }
            else if (x <= 0 && y < 0) {
                xDir = -1;
                yDir = -1;
            }
            else if (x < 0 && y >= 0) {
                flip = true;
                yDir = -1;
            }

            foreach (var skillPattern in EffectPattern) {
                if (flip) {
                    yield return new Location(yDir * skillPattern.loc.y, xDir * skillPattern.loc.x);
                }
                else {
                    yield return new Location(xDir * skillPattern.loc.x, yDir * skillPattern.loc.y);
                }
            }
        }
        /// <summary>
        /// Return effect location
        /// </summary>
        /// <param name="casterLoc"></param>
        /// <param name="cp"></param>
        /// <param name="includingPassLocation"></param>
        /// <returns></returns>
        public virtual IEnumerable<Location> GetSubEffectZone(Location casterLoc, Location cp, bool includingPassLocation = false)
        {
            switch (castType) {
                case CastType.Instant:
                    foreach (var ep in GetFixedEffectPatterns(cp)) {
                        yield return casterLoc + cp + ep;
                    }
                    break;
                case CastType.Trajectory:
                    foreach (var ep in GetFixedEffectPatterns(cp)) {
                        foreach (var tile in GridManager.Instance.GetTrajectoryHitTile(casterLoc + cp, ep, includingPassLocation)) {
                            if (includingPassLocation || !tile.IsEmpty)
                                yield return tile.Loc;
                        }
                    }
                    break;
                default:
                    break;
            }
        }
        /// <summary>
        /// Return effect location
        /// </summary>
        /// <param name="casterLoc"></param>
        /// <param name="includingPassLocation"></param>
        /// <returns></returns>
        public virtual IEnumerable<Location> GetEffectZone(Location casterLoc, bool includingPassLocation = false)
        {
            foreach (var cp in CastPattern) {
                foreach (var effectLoc in GetSubEffectZone(casterLoc, cp, includingPassLocation)) {
                    yield return effectLoc;
                }
            }
        }

        static Dictionary<int, BaseSkill> cache;
        public static Dictionary<int, BaseSkill> Dict {
            get {
                // load if not loaded yet
                return cache ?? (cache = Resources.LoadAll<BaseSkill>("Skills").ToDictionary(
                    item => item.skillName.GetStableHashCode(), item => item)
                );
            }
        }
    }    
}