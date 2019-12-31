using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wing.RPGSystem
{
    [CreateAssetMenu(fileName = "Buff", menuName = "Wing/Scriptable Buffs/DefenseBuff", order = 21)]
    public class DefenseBuff : BaseBuff
    {
        public float cofReduction;
        public float constReduction;
    }
}
