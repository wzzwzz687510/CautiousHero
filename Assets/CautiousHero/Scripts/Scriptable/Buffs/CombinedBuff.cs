﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wing.RPGSystem
{
    [CreateAssetMenu(fileName = "Buff", menuName = "Wing/Scriptable Buffs/CombinedBuff", order = 21)]
    public class CombinedBuff : BaseBuff
    {
        public BaseBuff[] buffPack;

        public override void ApplyEffect(BuffHandler bh)
        {
            Entity entity = bh.TargetHash.GetEntity();

            foreach (var buff in buffPack) {
                entity.EntityBuffManager.AddBuff(new BuffHandler(bh.CasterHash, bh.TargetHash, buff.Hash));
            }
        }
    }
}

