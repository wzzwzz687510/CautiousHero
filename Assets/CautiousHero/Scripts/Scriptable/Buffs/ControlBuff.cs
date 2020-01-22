using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wing.RPGSystem
{
    [CreateAssetMenu(fileName = "Buff", menuName = "Wing/Scriptable Buffs/ControlBuff", order = 21)]
    public class ControlBuff : BaseBuff
    {
        [Header("Control Parameters")]
        public bool movement;
        public bool castment;
        public bool recoverAP;

        public ControlBuff() : base(BuffType.Control, false, false) { }
    }
}