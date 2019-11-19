﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wing.TileUtils;

namespace Wing.RPGSystem
{
    [CreateAssetMenu(fileName = "Skill", menuName = "Wing/Scriptable Skills/BasicAttackSkill", order = 2)]
    public class BasicAttackSkill : ValueBasedSkill
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
                                ChangeHP(CalculateDamage(castEntity, ep.coefficient));
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
                                loc.stayEntity.ChangeHP(CalculateDamage(castEntity, ep.coefficient));
                                break;
                            }
                        }
                    }
                    break;
                default:
                    break;
            }

            //Debug.Log("cof: " + cof + ", base value: " + baseValue + ", levelCof: " + levelCof + ", attributeCof: " + attributeCof);
            //Debug.Log(cof * baseValue * (1 + levelCof * castEntity.Level + attributeCof * aa));
        }

        protected virtual int CalculateDamage(Entity caster,float cof)
        {
            int aa = 0;
            if (attribute == AdditiveAttribute.Agility)
                aa = caster.Agility;
            else if (attribute == AdditiveAttribute.Intelligence)
                aa = caster.Intelligence;
            else if (attribute == AdditiveAttribute.Strength)
                aa = caster.Strength;

            return Mathf.RoundToInt(cof * baseValue * (1 + levelCof * caster.Level + attributeCof * aa));
        }
    }
}
