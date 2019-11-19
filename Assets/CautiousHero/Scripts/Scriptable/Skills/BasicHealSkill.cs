using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wing.TileUtils;

namespace Wing.RPGSystem
{
    [CreateAssetMenu(fileName = "Skill", menuName = "Wing/Scriptable Skills/BasicHealSkill", order = 3)]
    public class BasicHealSkill : ValueBasedSkill
    {
        public override void ApplyEffect(Entity castEntity, Location castLoc)
        {
            Location effectLocation;
            switch (castType) {
                case CastType.Instant:
                    // Sequnence is important, 
                    AnimationManager.Instance.AddAnimClip(new CastAnimClip(castType,
                        castEffect.prefab, castEntity.Loc, castLoc, castEffect.animDuration));
                    if (BattleManager.Instance.IsPlayerTurn)
                        AnimationManager.Instance.PlayOnce();
                    foreach (var ep in EffectPatterns) {
                        effectLocation = castLoc + GetFixedEffectPattern(castLoc - castEntity.Loc, ep.pattern);
                        if (!GridManager.Instance.IsEmptyLocation(effectLocation)) {
                            GridManager.Instance.GetTileController(effectLocation).stayEntity.
                                ChangeHP(CalculateHealing(castEntity, ep.coefficient));
                        }
                    }
                    break;
                case CastType.Trajectory:
                    foreach (var ep in EffectPatterns) {
                        var dir = GetFixedEffectPattern(castLoc - castEntity.Loc, ep.pattern);
                        effectLocation = castLoc + dir;
                        foreach (var loc in GridManager.Instance.GetTrajectoryHitTile(castLoc, dir)) {
                            if (!loc.isEmpty) {
                                AnimationManager.Instance.AddAnimClip(new CastAnimClip(castType,
                                    castEffect.prefab, castEntity.Loc, loc.Loc, castEffect.animDuration));
                                if (BattleManager.Instance.IsPlayerTurn)
                                    AnimationManager.Instance.PlayOnce();
                                loc.stayEntity.ChangeHP(CalculateHealing(castEntity, ep.coefficient));
                                break;
                            }
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        protected virtual int CalculateHealing(Entity caster,float cof)
        {
            return -Mathf.RoundToInt(cof * baseValue * (1 + levelCof * caster.Level + attributeCof * caster.Intelligence));
        }
    }
}

