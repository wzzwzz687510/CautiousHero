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

        public override void ApplyEffect(int casterHash, Location castLoc, bool anim)
        {
            base.ApplyEffect(casterHash, castLoc,false);
        }

        public virtual void ApplyMagicCost(int casterHash)
        {
            throw new System.NotImplementedException();
        }
    }
}

