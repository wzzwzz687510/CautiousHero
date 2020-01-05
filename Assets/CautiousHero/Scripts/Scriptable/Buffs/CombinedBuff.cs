using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wing.RPGSystem
{
    [CreateAssetMenu(fileName = "Buff", menuName = "Wing/Scriptable Buffs/CombinedBuff", order = 22)]
    public class CombinedBuff : BaseBuff
    {
        public BaseBuff[] buffPack;

        public override void ApplyEffect(BuffHandler bh)
        {
            foreach (var buff in buffPack) {
                buff.ApplyEffect(bh);
            }
        }
    }
}

