using System.Collections;
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

    public enum ElementType
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

    public enum Rarity
    {
        Normal,
        Uncommon,
        Rare,
        Legend
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
        public string effectName;
        public float animDuration;
        public AudioClip sound;
    }

    [CreateAssetMenu(fileName = "Skill", menuName = "Wing/Scriptable Skills/BaseSkill", order = 0)]
    public class BaseSkill : ScriptableObject
    {
        [Header("Basic Parameters")]
        public string skillName;
        public string description;
        public Sprite sprite;
        public int actionPointsCost;
        public int Hash => skillName.GetStableHashCode();

        [Header("Labels")]
        public Rarity rarity;
        public DamageType damageType;
        public ElementType skillElement;
        public CastType castType;
        public List<Label> labels;

        [Header("Point Pattern")]
        public CastEffect castEffect;
        public ScriptablePattern tCastPatterns;
        public ScriptableEffectPattern tEffectPatterns;
        public Location[] CastPattern => ScriptablePattern.Dict[Hash];
        public EffectPattern[] EffectPattern => tEffectPatterns.effectPatterns;

        [Header("Addon")]
        public BaseSkill[] additionSkills;
        public BaseBuff[] additionBuffs;

        public virtual void ApplyEffect(int casterHash,Location casterLoc, Location selecLoc, bool anim)
        {
            foreach (var skill in additionSkills) {
                skill.ApplyEffect(casterHash,casterLoc, selecLoc,anim);
            }

            TileController tc = selecLoc.GetTileController();
            if (!tc.IsEmpty) {
                foreach (var buff in additionBuffs) {
                    tc.StayEntity.BuffManager.AddBuff(new BuffHandler(casterHash, tc.StayEntity.Hash, buff.Hash));
                }
            }

            if (anim) {
                AnimationManager.Instance.AddAnimClip(new CastAnimClip(castType,
                    Hash, casterHash.GetEntity().Loc, selecLoc, castEffect.animDuration));
                if (BattleManager.Instance.IsPlayerTurn)
                    AnimationManager.Instance.PlayOnce();
            }
        }

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

        public static int CompareByName(BaseSkill s1,BaseSkill s2)
        {
            return s1.skillName.CompareTo(s2.name);
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