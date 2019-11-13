using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wing.RPGSystem
{
    public enum BuffType
    {
        AttributeAdjust,
        HealthAdjust,
        ManaAdjust,
        ElementAdjust
    }

    public class BaseBuff : ScriptableObject
    {
        public BuffType buffType;
        public int lastTurn;
        public bool infinity;
    }

    public class AttributeBuff : BaseBuff
    {
        public EntityAttribute adjustValue;
    }
}

