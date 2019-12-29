using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Wing.RPGSystem
{
    [System.Serializable]
    public class BuffHandler
    {
        public int CasterHash { get; private set; }
        public int TargetHash { get; private set; }
        public int BuffHash { get; private set; }
        public BaseBuff TemplateBuff{get{return BuffHash.GetBaseBuff();} }
        public bool Infinity { get { return TemplateBuff.infinity; } }
        public bool Stackable { get { return TemplateBuff.stackable; } }

        public int LastTurn { get; set; }
        public int StackCount { get; set; }

        public BuffHandler(int casterHash, int targetHash, int buffhash)
        {
            CasterHash = casterHash;
            TargetHash = targetHash;
            BuffHash = buffhash;
            LastTurn = TemplateBuff.lastTurn;

            StackCount = 1;
            if(TemplateBuff.stackable) (TemplateBuff as StackableEventBuff).OnStacked(this);

            Entity target = targetHash.GetEntity();
            switch (TemplateBuff.trigger) {
                case BuffTrigger.TurnStart:
                    target.OnTurnStartedEvent.AddListener(OnTriggered);
                    break;
                case BuffTrigger.TurnEnd:
                    target.OnTurnEndedEvent.AddListener(OnTriggered);
                    break;
                case BuffTrigger.HPChange:
                    target.OnHPChanged.AddListener(OnTriggered);
                    break;
                case BuffTrigger.APChange:
                    target.OnAPChanged.AddListener(OnTriggered);
                    break;
                case BuffTrigger.PhysicalAPChange:
                    target.OnPhysicalAPChanged.AddListener(OnTriggered);
                    break;
                case BuffTrigger.MagicalAPChange:
                    target.OnMagicalAPChanged.AddListener(OnTriggered);
                    break;
                case BuffTrigger.SkillChange:
                    target.OnSkillChanged.AddListener(OnTriggered);
                    break;
                case BuffTrigger.CasterDeath:
                    casterHash.GetEntity().OnDead.AddListener(OnTriggered);
                    break;
                case BuffTrigger.TargetDeath:
                    target.OnDead.AddListener(OnTriggered);
                    break;
                default:
                    break;
            }
        }

        public void ResetBuff(int casterHash)
        {
            CasterHash = casterHash;
            LastTurn = TemplateBuff.lastTurn;
        }

        /// <summary>
        /// Cast this buff
        /// </summary>
        /// <returns>True for last to next turn</returns>
        public bool UpdateBuff()
        {
            if (Infinity || --LastTurn > 0)
                return true;

            Entity target = TargetHash.GetEntity();
            // Clear event registration.
            switch (TemplateBuff.trigger) {
                case BuffTrigger.TurnStart:
                    target.OnTurnStartedEvent.RemoveListener(OnTriggered);
                    break;
                case BuffTrigger.TurnEnd:
                    target.OnTurnEndedEvent.RemoveListener(OnTriggered);
                    break;
                case BuffTrigger.HPChange:
                    target.OnHPChanged.RemoveListener(OnTriggered);
                    break;
                case BuffTrigger.APChange:
                    target.OnAPChanged.RemoveListener(OnTriggered);
                    break;
                case BuffTrigger.PhysicalAPChange:
                    target.OnPhysicalAPChanged.RemoveListener(OnTriggered);
                    break;
                case BuffTrigger.MagicalAPChange:
                    target.OnMagicalAPChanged.RemoveListener(OnTriggered);
                    break;
                case BuffTrigger.SkillChange:
                    target.OnSkillChanged.RemoveListener(OnTriggered);
                    break;
                case BuffTrigger.CasterDeath:
                    CasterHash.GetEntity().OnDead.RemoveListener(OnTriggered);
                    break;
                case BuffTrigger.TargetDeath:
                    target.OnDead.RemoveListener(OnTriggered);
                    break;
                default:
                    break;
            }
            return false;
        }

        private void OnTriggered()
        {
            (TemplateBuff as EventBuff).ApplyEffect(this);
        }
    }

    [System.Serializable]
    public class BuffManager
    {
        public int entityHash;
        public Dictionary<BuffType, Dictionary<int, BuffHandler>> buffs = 
            new Dictionary<BuffType, Dictionary<int, BuffHandler>>(); // Key->buffhash;

        public BuffManager(int entityHash)
        {
            this.entityHash = entityHash;
        }

        public void AddBuff(BuffHandler bh)
        {
            int buffhash = bh.BuffHash;
            BuffType buffType = bh.TemplateBuff.type;
            if (buffs.ContainsKey(buffType)) {
                if (buffs[buffType].ContainsKey(buffhash)) {
                    BuffHandler buffHandler = buffs[buffType][buffhash];
                    if (buffHandler.Stackable) {
                        buffHandler.StackCount++;
                        (buffHandler.TemplateBuff as StackableEventBuff).OnStacked(bh);
                    }
                    else {
                        buffHandler.ResetBuff(bh.CasterHash);
                    }
                }
                else {
                    buffs[buffType].Add(buffhash, bh);
                }
            }
            else {
                buffs.Add(buffType, new Dictionary<int, BuffHandler>());
                buffs[buffType].Add(buffhash, bh);
            }
        }

        public void UpdateBuffs()
        {
            // Do something to host

            foreach (var buffDic in buffs.Values) {
                foreach (var buff in buffDic.Values) {
                    if (!buff.UpdateBuff()) {
                        buffDic.Remove(buff.BuffHash);
                    }
                }
            }
        }

        public EntityAttribute GetAttributeAdjustment()
        {
            EntityAttribute tmp = new EntityAttribute();
            if (buffs.ContainsKey(BuffType.Attribute))
                foreach (var buffHandler in buffs[BuffType.Attribute].Values) {
                    tmp += (buffHandler.TemplateBuff as AttributeBuff).adjustValue;
                }

            return tmp;
        }

        public BuffHandler GetBuffHandler(int buffhash) => buffs[buffhash.GetBaseBuff().type][buffhash];
    }
}
