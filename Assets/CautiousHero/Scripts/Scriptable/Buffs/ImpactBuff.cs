using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wing.RPGSystem
{
    [CreateAssetMenu(fileName = "Buff", menuName = "Wing/Scriptable Buffs/ImpactBuff", order = 23)]
    public class ImpactBuff : BaseBuff
    {
        public int hP;
        public int pAP;
        public int mAP;
        public int aP;
        public float damageCof;
        public float coinCof;
        public float expCof;

        public override void ApplyEffect(BuffHandler bh)
        {
            Entity target = bh.TargetHash.GetEntity();
            target.ImpactHP(hP, hP<0);
            target.ImpactArmour(pAP, true, pAP < 0);
            target.ImpactArmour(mAP, false, mAP < 0);
            target.ImpactActionPoints(aP, aP < 0);

        }
    }
}

