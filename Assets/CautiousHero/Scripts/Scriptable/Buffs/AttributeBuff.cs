using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wing.RPGSystem
{
    [CreateAssetMenu(fileName = "Buff", menuName = "Wing/Scriptable Buffs/AttributeBuff", order = 20)]
    public class AttributeBuff : BaseBuff
    {
        public new BuffType type = BuffType.Attribute;
        public EntityAttribute adjustValue;
    }
}
