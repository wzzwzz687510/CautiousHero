using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wing.TileUtils;

namespace Wing.RPGSystem
{
    [CreateAssetMenu(fileName = "Skill", menuName = "ScriptableSkills/BasicAttackSkill", order = 1)]
    public class BasicAttackSkill : BaseSkill
    {
        [Header("Basic Attack Parameters")]
        // Final number = baseValue * (1 + levelCof * level + attributeCof * attribute)
        public int baseValue;
        public float levelCof;
        public AdditiveAttribute attribute;
        public float attributeCof;

        public override Location GetFixedEffectPattern(Location cp, Location ep)
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

        public override IEnumerable<Location> GetFixedEffectPatterns(Location cp)
        {
            int x = cp.x;
            int y = cp.y;

            int xDir = 1,yDir = 1;
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

            foreach (var skillPattern in effectPatterns) {
                if (flip) {
                    yield return new Location(yDir * skillPattern.pattern.y, xDir * skillPattern.pattern.x);
                }
                else {
                    yield return new Location(xDir * skillPattern.pattern.x, yDir * skillPattern.pattern.y);
                }
            }
        }

        // cast location = cast pattern(cp) + entity's location.
        public override IEnumerable<Location> SubEffectZone(Location casterLoc, Location cp, bool includingPassLocation = false)
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

        public override IEnumerable<Location> EffectZone(Location casterLoc, bool includingPassLocation = false)
        {
            foreach (var cp in castPatterns) {
                foreach (var effectLoc in SubEffectZone(casterLoc,cp, includingPassLocation)) {
                    yield return effectLoc;
                }
            }
        }

        public override void ApplyEffect(Entity castEntity, Location castLoc)
        {
            int aa = 0;
            if (attribute == AdditiveAttribute.Agility)
                aa = castEntity.Agility;
            else if (attribute == AdditiveAttribute.Intelligence)
                aa = castEntity.Intelligence;
            else if (attribute == AdditiveAttribute.Strength)
                aa = castEntity.Strength;

            Location effectLocation;
            switch (castType) {
                case CastType.Instant:
                    foreach (var ep in effectPatterns) {
                        effectLocation = castLoc + GetFixedEffectPattern(castLoc - castEntity.Loc, ep.pattern);
                        if (!GridManager.Instance.IsEmptyLocation(effectLocation)) {
                            GridManager.Instance.GetTileController(effectLocation).stayEntity.DealDamage(
                                Mathf.RoundToInt(ep.coefficient * baseValue * (1 + levelCof * castEntity.Level + attributeCof * aa)));
                        }
                    }
                    break;
                case CastType.Trajectory:
                    foreach (var ep in effectPatterns) {
                        var dir = GetFixedEffectPattern(castLoc - castEntity.Loc, ep.pattern);
                        effectLocation = castLoc + dir;
                        foreach (var loc in GridManager.Instance.GetTrajectoryHitTile(castLoc, dir)) {
                            if (!loc.isEmpty)
                                loc.stayEntity.DealDamage(Mathf.RoundToInt(ep.coefficient * baseValue * 
                                    (1 + levelCof * castEntity.Level + attributeCof * aa)));
                        }
                    }
                    break;
                default:
                    break;
            }           
            //Debug.Log("cof: " + cof + ", base value: " + baseValue + ", levelCof: " + levelCof + ", attributeCof: " + attributeCof);
            //Debug.Log(cof * baseValue * (1 + levelCof * castEntity.Level + attributeCof * aa));
        }
    }
}
