using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wing.TileUtils;

namespace Wing.RPGSystem
{
    [CreateAssetMenu(fileName = "Skill", menuName = "ScriptableSkills/BasicAttackSkill", order = 1)]
    public class BasicAttackSkill : BaseSkill
    {
        // Final number = baseValue * (1 + levelCof * level + attributeCof * attribute)
        public int baseValue;
        public float levelCof;
        public float attributeCof;

        public override IEnumerable<Location> GetFixedEffectPattern(Location castPattern)
        {
            int x = castPattern.x;
            int y = castPattern.y;

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
                    yield return new Location(yDir * skillPattern.loc.y, xDir * skillPattern.loc.x);
                }
                else {
                    yield return new Location(xDir * skillPattern.loc.x, yDir * skillPattern.loc.y);
                }
            }
        }

        public override IEnumerable<Location> EffectZone(Location origin)
        {
            switch (skillType) {
                case SkillType.Instant:
                    foreach (var cp in castPatterns) {
                        foreach (var ep in GetFixedEffectPattern(cp)) {
                            yield return origin + cp + ep;
                        }
                    }
                    break;
                case SkillType.Trajectory:
                    foreach (var cp in castPatterns) {
                        foreach (var ep in GetFixedEffectPattern(cp)) {
                            foreach (var tile in GridManager.Instance.GetTrajectoryHitTile(origin + cp,ep)) {
                                yield return tile.Loc;
                            }
                        }                       
                    }
                    break;
                default:
                    break;
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
