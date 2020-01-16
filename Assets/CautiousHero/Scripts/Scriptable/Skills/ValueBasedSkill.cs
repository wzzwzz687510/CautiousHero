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
        public float attributeCof = 1;

        public override void ApplyEffect(int casterHash, Location casterLoc, Location selecLoc, bool anim)
        {
            Entity caster = casterHash.GetEntity();
            Location effectLocation;
            switch (castType) {
                case CastType.Instant:
                    foreach (var ep in EffectPattern) {                       
                        effectLocation = selecLoc + GetFixedEffectPattern(selecLoc - casterLoc, ep.loc);
                        //Debug.Log(string.Format("el: {0}, cl: {1}, cp: {2}, ep: {3}", effectLocation, casterLoc, castLoc - casterLoc, ep.loc));
                        if (effectLocation.TryGetStayEntity(out Entity target) && target != null) {
                            if (anim) {
                                AnimationManager.Instance.AddAnimClip(new CastAnimClip(
                                    castType, Hash, casterLoc, effectLocation, castEffect.animDuration));
                                if (BattleManager.Instance.IsPlayerTurn)
                                    AnimationManager.Instance.PlayOnce();
                            }
                            target.DealDamage(CalculateValue(casterHash, ep.coefficient), damageType);
                            for (int i = 0; i < ep.additionBuffs.Length; i++) {
                                target.EntityBuffManager.AddBuff(new BuffHandler(
                                    casterHash, target.Hash, ep.additionBuffs[i].Hash));
                            }
                        }
                    }
                    break;
                case CastType.Trajectory:
                    foreach (var ep in EffectPattern) {
                        var dir = GetFixedEffectPattern(selecLoc - casterLoc, ep.loc);
                        effectLocation = selecLoc + dir;
                        foreach (var tc in GridManager.Instance.GetTrajectoryHitTile(selecLoc, dir)) {
                            if (!tc.IsEmpty) {
                                if (anim) {
                                    AnimationManager.Instance.AddAnimClip(new CastAnimClip(castType,
                                        Hash, casterLoc, tc.Loc, castEffect.animDuration));
                                    if (BattleManager.Instance.IsPlayerTurn)
                                        AnimationManager.Instance.PlayOnce();
                                }

                                Entity target = tc.StayEntity;
                                target.DealDamage(CalculateValue(casterHash, ep.coefficient), damageType);
                                for (int i = 0; i < ep.additionBuffs.Length; i++) {
                                    target.EntityBuffManager.AddBuff(new BuffHandler(
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
            base.ApplyEffect(casterHash,casterLoc, selecLoc,false);
        }

        public abstract int CalculateValue(int casterHash, float cof);
    }
}
