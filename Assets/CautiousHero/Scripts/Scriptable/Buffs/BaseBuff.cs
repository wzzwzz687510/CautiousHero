using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Wing.RPGSystem
{
    public enum BuffType
    {
        Default,
        Attribute,
        Defense,
        ArrowReturn,
        Combined
    }

    public enum BuffTrigger
    {
        None,
        TurnStart,
        TurnEnd,
        HPChange,
        APChange,
        PhysicalAPChange,
        MagicalAPChange,
        ArmourPChange,
        SkillChange,
        CasterDeath,
        TargetDeath,
        BattleEnd
    }

    public abstract class BaseBuff : ScriptableObject
    {
        public string buffName;
        public string description;
        public Sprite sprite;
        public BuffType type;
        public BuffTrigger trigger;
        public int triggerTimes;
        public int lastTurn;
        public bool infinity;
        public bool stackable;
        public int Hash => buffName.GetStableHashCode();

        public abstract void ApplyEffect(BuffHandler bh);

        static Dictionary<int, BaseBuff> cache;
        public static Dictionary<int, BaseBuff> Dict {
            get {
                // load if not loaded yet
                return cache ?? (cache = Resources.LoadAll<BaseBuff>("Buffs").ToDictionary(
                    item => item.buffName.GetStableHashCode(), item => item)
                );
            }
        }
    }
}

