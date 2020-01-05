using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wing.RPGSystem
{
    [CreateAssetMenu(fileName = "Buff", menuName = "Wing/Scriptable Buffs/AttributeBuff", order = 20)]
    public class AttributeBuff : BaseBuff
    {
        public EntityAttribute adjustValue;

        public override void ApplyEffect(BuffHandler bh)
        {
            
        }
    }
}
