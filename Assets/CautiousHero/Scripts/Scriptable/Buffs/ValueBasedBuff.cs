using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wing.RPGSystem
{
    [CreateAssetMenu(fileName = "Buff", menuName = "Wing/Scriptable Buffs/ValueBasedBuff", order = 21)]
    public class ValueBasedBuff : BaseBuff
    {
        public int baseValue;
        public float cof;

        public ValueBasedBuff() : base(BuffType.DamageAdjustment, false, false) { }
    }
}