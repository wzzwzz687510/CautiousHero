using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wing.RPGSystem
{
    public interface IMagicalCost
    {
        void ApplyMagicCost(int casterHash);
    }

    public class MagicalSkill : CombinedSkill, IMagicalCost
    {

        public override void ApplyEffect(int casterHash, Location castLoc)
        {
            base.ApplyEffect(casterHash, castLoc);
        }

        public virtual void ApplyMagicCost(int casterHash)
        {
            throw new System.NotImplementedException();
        }
    }
}

