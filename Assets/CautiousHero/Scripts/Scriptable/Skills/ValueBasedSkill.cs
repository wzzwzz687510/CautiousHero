using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wing.RPGSystem
{
    public abstract class ValueBasedSkill : BaseSkill
    {
        [Header("Value Parameters")]
        // Final number = baseValue * (1 + levelCof * level + attributeCof * attribute)
        public int baseValue;
        public float levelCof;
        public float attributeCof;

        public override void ApplyEffect(Entity caster, Location castLoc)
        {
            Location effectLocation;
            switch (castType) {
                case CastType.Instant:
                    // Sequnence is important, 
                    AnimationManager.Instance.AddAnimClip(new CastAnimClip(castType,
                        castEffect.prefab, caster.Loc, castLoc, castEffect.animDuration));
                    if (BattleManager.Instance.IsPlayerTurn)
                        AnimationManager.Instance.PlayOnce();
                    foreach (var ep in EffectPatterns) {
                        effectLocation = castLoc + GetFixedEffectPattern(castLoc - caster.Loc, ep.pattern);
                        if (effectLocation.IsValid() && !effectLocation.IsEmpty()) {
                            effectLocation.StayEntity().ChangeHP(CalculateValue(caster, ep.coefficient));
                        }
                    }
                    break;
                case CastType.Trajectory:
                    foreach (var ep in EffectPatterns) {
                        var dir = GetFixedEffectPattern(castLoc - caster.Loc, ep.pattern);
                        effectLocation = castLoc + dir;
                        foreach (var tc in GridManager.Instance.GetTrajectoryHitTile(castLoc, dir)) {
                            if (!tc.IsEmpty) {
                                AnimationManager.Instance.AddAnimClip(new CastAnimClip(castType,
                                    castEffect.prefab, caster.Loc, tc.Loc, castEffect.animDuration));
                                if (BattleManager.Instance.IsPlayerTurn)
                                    AnimationManager.Instance.PlayOnce();
                                tc.StayEntity.ChangeHP(CalculateValue(caster, ep.coefficient));
                                break;
                            }
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        public abstract int CalculateValue(Entity caster, float cof);
    }
}
