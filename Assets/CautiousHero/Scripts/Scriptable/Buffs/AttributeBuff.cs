using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wing.RPGSystem
{
    [CreateAssetMenu(fileName = "Buff", menuName = "Wing/Scriptable Buffs/AttributeBuff", order = 21)]
    public class AttributeBuff : BaseBuff
    {
        public AttributeBuff() : base(BuffType.Attribute, false, true)
        {
            isAddLastingTurn = true;
            isTriggeredOnStacked = true;
        }
    }
}