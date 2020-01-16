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
        Invincible,
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
        CasterMovement,
        TargetDeath,
        TargetMovement,
        BattleEnd
    }

    public interface IStackableBuff
    {
        void OnStacked(BuffHandler bh);
    }

    [CreateAssetMenu(fileName = "Buff", menuName = "Wing/Scriptable Buffs/BaseBuff", order = 20)]
    public class BaseBuff : ScriptableObject
    {
        [Header("Basic Parameters")]
        public string buffName;
        public string description;
        public Sprite sprite;
        public BuffType type;
        public VisualEffect buffEffect;

        [Header("Trigger Parameters")]
        public BuffTrigger trigger;
        public int triggerTimes;
        public bool triggerInfinityTimes;
        public int lastTurn;
        public bool infinity;

        [Header("Stack Parameters")]        
        public bool stackable;
        public int stackTriggerNumber;
        public bool isTriggeredOnStacked;

        public EntityAttribute adjustValue;
        public int Hash => buffName.GetStableHashCode();

        public BaseBuff(){ }

        public BaseBuff(BuffType type,bool infinity, bool stackable)
        {
            this.type = type;
            this.infinity = infinity;
            this.stackable = stackable;
        }

        public virtual void ApplyEffect(BuffHandler bh)
        {

        }

        public virtual void OnStacked(BuffHandler bh)
        {
            if (bh.StackCount >= stackTriggerNumber || isTriggeredOnStacked) ApplyEffect(bh);
        }

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

