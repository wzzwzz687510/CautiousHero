using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wing.RPGSystem
{
    public enum BuffType
    {
        Attribute,
        Health,
        Mana,
        Defend,
        Element
    }

    public class BaseBuff : ScriptableObject
    {
        public BuffType buffType;
        public int lastTurn;
        public bool infinity;

        public virtual void ApplyEffect(Entity castEntity, Entity targetEntity)
        {

        }
    }

    public class AttributeBuff : BaseBuff
    {
        public EntityAttribute adjustValue;
    }

    public class DicountBuff: BaseBuff
    {
        public float reduceCof;
        public int reduceConst;
    }

    public class DamageBuff: DicountBuff
    {
    }

    public class ManaBuff : DicountBuff
    {
    }
}

