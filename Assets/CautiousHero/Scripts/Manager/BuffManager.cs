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
        public BaseBuff Template => BuffHash.GetBaseBuff();
        public BuffType Type => Template.type;
        public bool Infinity => Template.infinity; 
        public bool Stackable => Template.stackable; 

        public int LastTurn { get; private set; }
        public int TriggerTime { get; private set; }
        public int StackCount { get; private set; }

        public BuffHandler(int casterHash, int targetHash, int buffhash)
        {
            CasterHash = casterHash;
            TargetHash = targetHash;
            BuffHash = buffhash;
            ResetBuff(casterHash);

            ConnectEvent(true);
        }

        //~BuffHandler()
        //{
        //    ConnectEvent(false);
        //}

        public void ResetBuff(int casterHash)
        {
            CasterHash = casterHash;
            LastTurn = Template.lastTurn;
            TriggerTime = Template.triggerTimes;
            StackCount = 1;
        }

        public void ApplyEffect()
        {
            Template.ApplyEffect(this);
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
            Template.OnStacked(this);
        }

        public void OnPopped(int number)
        {
            StackCount -= number;
            if (StackCount < 1) {
                ConnectEvent(false);
                TargetHash.GetEntity().EntityBuffManager.RemoveBuffHashes.Add(BuffHash);
            }              
        }

        private void ConnectEvent(bool isBind)
        {
            Entity target = TargetHash.GetEntity();
            if (isBind)
                switch (Template.trigger) {
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
                    case BuffTrigger.CasterMovement:
                        CasterHash.GetEntity().OnMovedEvent += OnTriggeredMultipleTimes;
                        break;
                    case BuffTrigger.TargetDeath:
                        target.OnDead.AddListener(OnTriggered);
                        break;
                    case BuffTrigger.TargetMovement:
                        target.OnMovedEvent += OnTriggeredMultipleTimes;
                        break;
                    case BuffTrigger.BattleEnd:
                        BattleManager.Instance.BattleEndEvent.AddListener(OnTriggered);
                        break;
                    default:
                        break;
                }
            else
                switch (Template.trigger) {
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

        private void OnTriggeredMultipleTimes(int times)
        {
            for (int i = 0; i < times; i++) {
                OnTriggered();
            }
        }

        private void OnTriggered()
        {
            Template.ApplyEffect(this);
            if (!Template.triggerInfinityTimes) {
                TriggerTime--;
                if (TriggerTime <= 0) AnimationManager.Instance.AddAnimClip(new BuffRemovementAnimClip(TargetHash, BuffHash));
                return;
            }            
        }
    }

    [System.Serializable]
    public class BuffManager
    {
        public int entityHash;
        public Dictionary<BuffType, Dictionary<int, BuffHandler>> buffTypeDic; // Key->buffhash;
        public List<int> buffHashes;

        public List<int> RemoveBuffHashes { get; private set; }

        public delegate void OnBuffChanged(int buffHash, bool isAdd);
        public OnBuffChanged OnBuffChangedEvent;

        public BuffManager(int entityHash)
        {
            this.entityHash = entityHash;
            buffTypeDic = new Dictionary<BuffType, Dictionary<int, BuffHandler>>();
            buffHashes = new List<int>();
            RemoveBuffHashes = new List<int>();
        }

        public void ResetManager()
        {
            buffTypeDic = new Dictionary<BuffType, Dictionary<int, BuffHandler>>();
            buffHashes = new List<int>();
            RemoveBuffHashes = new List<int>();
        }

        public void AddBuff(BuffHandler bh)
        {
            int buffhash = bh.BuffHash;
            BuffType buffType = bh.Template.type;
            if (buffTypeDic.ContainsKey(buffType)) {
                if (buffTypeDic[buffType].ContainsKey(buffhash)) {
                    BuffHandler buffHandler = buffTypeDic[buffType][buffhash];
                    if (buffHandler.Stackable) {
                        buffHandler.OnStacked();
                    }
                    else {
                        buffHandler.ResetBuff(bh.CasterHash);
                    }
                }
                else {
                    buffTypeDic[buffType].Add(buffhash, bh);
                    buffHashes.Add(buffhash);
                    OnBuffChangedEvent?.Invoke(buffHashes.Count-1,true);
                }
            }
            else {
                buffTypeDic.Add(buffType, new Dictionary<int, BuffHandler>());
                buffTypeDic[buffType].Add(buffhash, bh);
                buffHashes.Add(buffhash);
                OnBuffChangedEvent?.Invoke(buffHashes.Count - 1,true);
            }
        }

        public void UpdateBuffs()
        {
            // Do something to host
            List<int> removeHashes = new List<int>();
            foreach (var buffDic in buffTypeDic.Values) {
                foreach (var buff in buffDic.Values) {
                    if (!buff.UpdateBuff()) {
                        removeHashes.Add(buff.BuffHash);
                    }
                }
                foreach (var hash in removeHashes) {
                    buffDic.Remove(hash);
                    OnBuffChangedEvent?.Invoke(buffHashes.IndexOf(hash), false);
                    buffHashes.Remove(hash);
                }
                removeHashes.Clear();
            }
        }

        public void RemoveBuff(int buffHash)
        {
            if (!buffHashes.Contains(buffHash)) return;
            buffTypeDic[buffHash.GetBaseBuff().type].Remove(buffHash);            
            OnBuffChangedEvent?.Invoke(buffHashes.IndexOf(buffHash), false);
            buffHashes.Remove(buffHash);
        }

        public EntityAttribute GetAttributeAdjustment()
        {
            EntityAttribute tmp = new EntityAttribute();
            if (buffTypeDic.ContainsKey(BuffType.Attribute))
                foreach (var buffHandler in buffTypeDic[BuffType.Attribute].Values) {
                    tmp += buffHandler.Template.adjustValue * buffHandler.StackCount;
                }

            return tmp;
        }

        public bool CheckIsInvincible()
        {
            if (buffTypeDic.ContainsKey(BuffType.Invincible) && buffTypeDic[BuffType.Invincible].Count != 0) {
                foreach (var bh in buffTypeDic[BuffType.Invincible].Values) {
                    bh.ApplyEffect();
                }
                foreach (var hash in RemoveBuffHashes) {
                    AnimationManager.Instance.AddAnimClip(new BuffRemovementAnimClip(entityHash,hash));
                }
                RemoveBuffHashes.Clear();
                return true;
            }
            return false;
        }

        public BuffHandler GetBuffHandler(int buffhash) => buffTypeDic[buffhash.GetBaseBuff().type][buffhash];
    }
}
