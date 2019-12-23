using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wing.RPGSystem
{
    public abstract class ArrowReturnBuff : StackableEventBuff
    {
        public new BuffType type = BuffType.ArrowReturn;
        public new BuffTrigger trigger = BuffTrigger.TargetDeath;
        public new bool stackable = true;

        public override void ApplyEffect(BuffHandler bh)
        {
            // Add arrow to caster
        }
    }
}

