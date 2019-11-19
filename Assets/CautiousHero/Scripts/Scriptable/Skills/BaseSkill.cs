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
        public CastEffect castEffect;
        public ScriptablePattern tCastPatterns;
        public ScriptableEffectPattern tEffectPatterns;
        public Location[] CastPatterns { get { return tCastPatterns.patterns; } }
        public EffectPattern[] EffectPatterns { get { return tEffectPatterns.effectPatterns; } }

        public abstract void ApplyEffect(Entity castEntity, Location castLoc);

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

            foreach (var skillPattern in EffectPatterns) {
                if (flip) {
                    yield return new Location(yDir * skillPattern.pattern.y, xDir * skillPattern.pattern.x);
                }
                else {
                    yield return new Location(xDir * skillPattern.pattern.x, yDir * skillPattern.pattern.y);
                }
            }
        }
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
                            if (includingPassLocation || !tile.isEmpty)
                                yield return tile.Loc;
                        }
                    }
                    break;
                default:
                    break;
            }
        }
        public virtual IEnumerable<Location> GetEffectZone(Location casterLoc, bool includingPassLocation = false)
        {
            foreach (var cp in CastPatterns) {
                foreach (var effectLoc in GetSubEffectZone(casterLoc, cp, includingPassLocation)) {
                    yield return effectLoc;
                }
            }
        }
    }    
}