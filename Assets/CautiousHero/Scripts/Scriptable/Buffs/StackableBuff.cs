using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wing.RPGSystem
{
    public interface IStackableBuff
    {
        void OnStacked(BuffHandler bh);
    }

    public abstract class StackableBuff : BaseBuff, IStackableBuff
    {
        public int triggerNumber;
        public bool triggerEachTime;

        public virtual void OnStacked(BuffHandler bh)
        {
            if (bh.StackCount >= triggerNumber || triggerEachTime) ApplyEffect(bh);
        }
    }
}
