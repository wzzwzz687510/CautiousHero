using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wing.RPGSystem
{
    public class MagicalSkill : BaseSkill, IMagicalCost
    {
        public ElementMana cost;
        public override void ApplyEffect(int casterHash, Location casterLoc, Location selecLoc, bool anim)
        {
            base.ApplyEffect(casterHash,casterLoc, selecLoc,false);
        }

        public virtual void ApplyMagicCost(int casterHash)
        {
            throw new System.NotImplementedException();
        }
    }
}

