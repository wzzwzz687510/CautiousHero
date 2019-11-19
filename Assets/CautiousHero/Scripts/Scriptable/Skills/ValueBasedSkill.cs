using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wing.TileUtils;

namespace Wing.RPGSystem
{
    public abstract class ValueBasedSkill : BaseSkill
    {
        [Header("Value Parameters")]
        // Final number = baseValue * (1 + levelCof * level + attributeCof * attribute)
        public int baseValue;
        public float levelCof;
        public AdditiveAttribute attribute;
        public float attributeCof;
    }
}
