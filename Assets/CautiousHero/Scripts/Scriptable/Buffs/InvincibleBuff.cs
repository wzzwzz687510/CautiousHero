using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wing.RPGSystem
{
    [CreateAssetMenu(fileName = "Buff", menuName = "Wing/Scriptable Buffs/InvincibleBuff", order = 22)]
    public class InvincibleBuff : BaseBuff
    {
        public InvincibleBuff() : base(BuffType.Invincible, true, false) { }

        public override void ApplyEffect(BuffHandler bh)
        {
            if (bh.StackCount < 1) return;
            bh.OnPopped(1);
            AnimationManager.Instance.AddAnimClip(new BuffAnimClip(
                                        bh.BuffHash, bh.TargetHash.GetEntity().Loc,bh.Template.buffEffect.animDuration));
            if (BattleManager.Instance.IsPlayerTurn)
                AnimationManager.Instance.PlayOnce();
        }
    }
}
