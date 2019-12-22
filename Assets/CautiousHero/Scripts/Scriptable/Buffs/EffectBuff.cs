using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wing.RPGSystem
{
    public abstract class EffectBuff : BaseBuff
    {
        public abstract void ApplyEffect(Entity castEntity, Entity targetEntity);
    }
    
}

