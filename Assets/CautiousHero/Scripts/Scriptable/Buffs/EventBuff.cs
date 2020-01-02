using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wing.RPGSystem
{
    public abstract class EventBuff : BaseBuff
    {
        public abstract void ApplyEffect(BuffHandler bh);
    }

    public abstract class StackableEventBuff : EventBuff, IStackableBuff
    {
        public int triggerNumber;
        public bool triggerEachTime;

        public virtual void OnStacked(BuffHandler bh)
        {
            if (bh.StackCount >= triggerNumber || triggerEachTime) ApplyEffect(bh);
        }
    }
}

