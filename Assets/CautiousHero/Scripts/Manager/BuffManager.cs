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

        public int LastTurn { get; private set; }
        public int TriggerTime { get; private set; }
        public int StackCount { get; private set; }

        public BuffHandler(int casterHash, int targetHash, int buffhash)
        {
            CasterHash = casterHash;
            TargetHash = targetHash;
            BuffHash = buffhash;
            LastTurn = TemplateBuff.lastTurn;
            TriggerTime = TemplateBuff.triggerTimes;

            StackCount = 1;

            ConnectEvent(true);
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
            if (Infinity || (TriggerTime > 0 && --LastTurn > 0))
                return true;

            // Clear event registration.
            ConnectEvent(false);
            return false;
        }

        public void OnStacked()
        {
            StackCount++;
            TemplateBuff.OnStacked(this);
        }

        private void ConnectEvent(bool isBind)
        {
            Entity target = TargetHash.GetEntity();
            if (isBind)
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
                        CasterHash.GetEntity().OnDead.AddListener(OnTriggered);
                        break;
                    case BuffTrigger.TargetDeath:
                        target.OnDead.AddListener(OnTriggered);
                        break;
                    case BuffTrigger.BattleEnd:
                        BattleManager.Instance.BattleEndEvent.AddListener(OnTriggered);
                        break;
                    default:
                        break;
                }
            else
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
                    case BuffTrigger.BattleEnd:
                        BattleManager.Instance.BattleEndEvent.RemoveListener(OnTriggered);
                        break;
                    default:
                        break;
                }
        }

        private void OnTriggered()
        {
            TriggerTime--;
            if (TriggerTime <= 0) ConnectEvent(false);
            TemplateBuff.ApplyEffect(this);
        }
    }

    [System.Serializable]
    public class BuffManager
    {
        public int entityHash;
        public Dictionary<BuffType, Dictionary<int, BuffHandler>> buffDic; // Key->buffhash;
        public List<int> buffHashes;

        public delegate void OnBuffChanged(int buffHash, bool isAdd);
        public OnBuffChanged OnBuffChangedEvent;

        public BuffManager(int entityHash)
        {
            this.entityHash = entityHash;
            buffDic = new Dictionary<BuffType, Dictionary<int, BuffHandler>>();
            buffHashes = new List<int>();
        }

        public void AddBuff(BuffHandler bh)
        {
            int buffhash = bh.BuffHash;
            BuffType buffType = bh.TemplateBuff.type;
            if (buffDic.ContainsKey(buffType)) {
                if (buffDic[buffType].ContainsKey(buffhash)) {
                    BuffHandler buffHandler = buffDic[buffType][buffhash];
                    if (buffHandler.Stackable) {
                        buffHandler.OnStacked();
                    }
                    else {
                        buffHandler.ResetBuff(bh.CasterHash);
                    }
                }
                else {
                    buffDic[buffType].Add(buffhash, bh);
                    buffHashes.Add(buffhash);
                    OnBuffChangedEvent?.Invoke(buffHashes.Count-1,true);
                }
            }
            else {
                buffDic.Add(buffType, new Dictionary<int, BuffHandler>());
                buffDic[buffType].Add(buffhash, bh);
                buffHashes.Add(buffhash);
                OnBuffChangedEvent?.Invoke(buffHashes.Count - 1,true);
            }
        }

        public void UpdateBuffs()
        {
            // Do something to host
            List<int> removeHashes = new List<int>();
            foreach (var buffDic in buffDic.Values) {
                foreach (var buff in buffDic.Values) {
                    if (!buff.UpdateBuff()) {
                        removeHashes.Add(buff.BuffHash);
                    }
                }
                foreach (var hash in removeHashes) {
                    buffDic.Remove(hash);
                    OnBuffChangedEvent?.Invoke(buffHashes.IndexOf(hash), false);
                }
                removeHashes.Clear();
            }
        }

        public EntityAttribute GetAttributeAdjustment()
        {
            EntityAttribute tmp = new EntityAttribute();
            if (buffDic.ContainsKey(BuffType.Attribute))
                foreach (var buffHandler in buffDic[BuffType.Attribute].Values) {
                    tmp += buffHandler.TemplateBuff.adjustValue * buffHandler.StackCount;
                }

            return tmp;
        }

        public BuffHandler GetBuffHandler(int buffhash) => buffDic[buffhash.GetBaseBuff().type][buffhash];
    }
}
