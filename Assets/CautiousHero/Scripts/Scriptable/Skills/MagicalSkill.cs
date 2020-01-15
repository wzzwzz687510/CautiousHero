using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wing.RPGSystem
{
    public interface IMagicalCost
    {
        void ApplyMagicCost(int casterHash);
    }

    public class MagicalSkill : BaseSkill, IMagicalCost
    {

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

