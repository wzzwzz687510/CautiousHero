using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wing.RPGSystem
{
    [CreateAssetMenu(fileName = "Buff", menuName = "Wing/Scriptable Buffs/AdditionTriggerBuff", order = 21)]
    public class AdditionTriggerBuff : BaseBuff
    {
        public BaseBuff[] additionBuffs;

        public override void ApplyEffect(BuffHandler bh)
        {
            bh.ResetBuff(bh.CasterHash);
            Entity entity = bh.TargetHash.GetEntity();

            foreach (var buff in additionBuffs) {
                entity.EntityBuffManager.AddBuff(new BuffHandler(bh.CasterHash, bh.TargetHash, buff.Hash));
            }
        }
    }
}
