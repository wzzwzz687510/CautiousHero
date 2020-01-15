﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wing.RPGSystem
{
    public abstract class ValueBasedSkill : BaseSkill
    {
        [Header("Value Parameters")]
        // Final number = baseValue * (1 + levelCof * level + attributeCof * attribute)
        public int baseValue;
        public float attributeCof;

        public override void ApplyEffect(int casterHash, Location castLoc, bool anim)
        {
            Entity caster = casterHash.GetEntity();
            Location effectLocation;
            switch (castType) {
                case CastType.Instant:
                    if (anim) {
                        AnimationManager.Instance.AddAnimClip(new CastAnimClip(castType, Hash, caster.Loc, castLoc, castEffect.animDuration));
                        if (BattleManager.Instance.IsPlayerTurn)
                            AnimationManager.Instance.PlayOnce();
                    }

                    foreach (var ep in EffectPattern) {
                        effectLocation = castLoc + GetFixedEffectPattern(castLoc - caster.Loc, ep.loc);
                        if (effectLocation.TryGetStayEntity(out Entity target) && target != null) {
                            target.DealDamage(CalculateValue(casterHash, ep.coefficient), damageType);
                            for (int i = 0; i < ep.additionBuffs.Length; i++) {
                                target.BuffManager.AddBuff(new BuffHandler(
                                    casterHash, target.Hash, ep.additionBuffs[i].Hash));
                            }
                        }
                    }
                    break;
                case CastType.Trajectory:
                    foreach (var ep in EffectPattern) {
                        var dir = GetFixedEffectPattern(castLoc - caster.Loc, ep.loc);
                        effectLocation = castLoc + dir;
                        foreach (var tc in GridManager.Instance.GetTrajectoryHitTile(castLoc, dir)) {
                            if (!tc.IsEmpty) {
                                if (anim) {
                                    AnimationManager.Instance.AddAnimClip(new CastAnimClip(castType,
                                        Hash, caster.Loc, tc.Loc, castEffect.animDuration));
                                    if (BattleManager.Instance.IsPlayerTurn)
                                        AnimationManager.Instance.PlayOnce();
                                }

                                Entity target = tc.StayEntity;
                                target.DealDamage(CalculateValue(casterHash, ep.coefficient), damageType);
                                for (int i = 0; i < ep.additionBuffs.Length; i++) {
                                    target.BuffManager.AddBuff(new BuffHandler(
                                        casterHash, target.Hash, ep.additionBuffs[i].Hash));
                                }
                                break;
                            }
                        }
                    }
                    break;
                default:
                    break;
            }
            base.ApplyEffect(casterHash, castLoc,false);
        }

        public abstract int CalculateValue(int casterHash, float cof);
    }
}
