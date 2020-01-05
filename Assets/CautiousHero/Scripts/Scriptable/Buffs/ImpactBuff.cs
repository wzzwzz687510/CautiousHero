using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wing.RPGSystem
{
    [CreateAssetMenu(fileName = "Buff", menuName = "Wing/Scriptable Buffs/ImpactBuff", order = 23)]
    public class ImpactBuff : BaseBuff
    {
        public int hp;
        public int pap;
        public int map;
        public int ap;
        public float damageCof;
        public float coinCof;
        public float expCof;

        public override void ApplyEffect(BuffHandler bh)
        {
            Entity target = bh.TargetHash.GetEntity();
            target.ImpactHP(hp, hp<0);
            target.ImpactArmour(pap, true, pap < 0);
            target.ImpactArmour(map, false, map < 0);
            target.ImpactActionPoints(ap, ap < 0);

        }
    }
}

