using System.Linq;
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

        public int LastingTurn { get; private set; }
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

        public void ImpactLastingTurn(int number)
        {
            LastingTurn += number;
        }

        public void ResetBuff(int casterHash)
        {
            CasterHash = casterHash;
            LastingTurn = Template.lastingTurn;
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
            if (Infinity || (TriggerTime > 0 && --LastingTurn > 0))
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
                TargetHash.GetEntity().EntityBuffManager.RemoveBuffHashes.Add(BuffHash);
            }              
        }

        public void ConnectEvent(bool isBind)
        {
            //Debug.Log("Name: " + Template.buffName + ", trigger: " + Template.trigger);
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
                    case BuffTrigger.CasterMovement:
                        CasterHash.GetEntity().OnMovedEvent -= OnTriggeredMultipleTimes;
                        break;
                    case BuffTrigger.TargetDeath:
                        target.OnDead.RemoveListener(OnTriggered);
                        break;
                    case BuffTrigger.TargetMovement:
                        target.OnMovedEvent -= OnTriggeredMultipleTimes;
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
        public List<int> BuffHashes { get; private set; }

        public List<int> RemoveBuffHashes { get; private set; }

        public bool Movable => CheckMovable();
        public bool Castable => CheckCastable();
        public bool APRecoverable => CheckAPRecoverable();

        public delegate void OnBuffChanged(int buffID, bool isAdd);
        public OnBuffChanged OnBuffChangedEvent;

        public BuffManager(int entityHash)
        {
            this.entityHash = entityHash;
            buffTypeDic = new Dictionary<BuffType, Dictionary<int, BuffHandler>>();
            BuffHashes = new List<int>();
            RemoveBuffHashes = new List<int>();
        }

        public void ResetManager()
        {
            // Unbind all event
            foreach (var buffDic in buffTypeDic.Values) {
                foreach (var bh in buffDic.Values) {
                    //OnBuffChangedEvent.Invoke(BuffHashes.IndexOf(bh.BuffHash), false);
                    bh.ConnectEvent(false);
                }
            }
            buffTypeDic.Clear();
            BuffHashes.Clear();
            RemoveBuffHashes.Clear();            
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
                    BuffHashes.Add(buffhash);
                    OnBuffChangedEvent?.Invoke(BuffHashes.Count-1,true);
                }
            }
            else {
                buffTypeDic.Add(buffType, new Dictionary<int, BuffHandler>());
                buffTypeDic[buffType].Add(buffhash, bh);
                BuffHashes.Add(buffhash);
                OnBuffChangedEvent?.Invoke(BuffHashes.Count - 1,true);
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
                    RemoveBuff(hash);
                }
                removeHashes.Clear();
            }
        }

        public void RemoveBuff(int buffHash)
        {
            if (!BuffHashes.Contains(buffHash)) return;
            buffTypeDic[buffHash.GetBaseBuff().type][buffHash].ConnectEvent(false);
            buffTypeDic[buffHash.GetBaseBuff().type].Remove(buffHash);
            OnBuffChangedEvent?.Invoke(BuffHashes.IndexOf(buffHash), false);
            BuffHashes.Remove(buffHash);
        }

        public EntityAttribute GetAttributeAdjustment()
        {
            EntityAttribute tmp = new EntityAttribute();
            if (buffTypeDic.ContainsKey(BuffType.Attribute))
                foreach (var buffHandler in buffTypeDic[BuffType.Attribute].Values) {
                    tmp += buffHandler.Template.adjustValue * 
                        (buffHandler.Template.isAttributeCof ? buffHandler.StackCount : 1);
                }

            return tmp;
        }

        public float GetDamageAdjustment()
        {
            float cof = 1.0f;
            if (buffTypeDic.ContainsKey(BuffType.DamageAdjustment))
                foreach (var bh in buffTypeDic[BuffType.DamageAdjustment].Values) {
                    cof += (bh.Template as ValueBasedBuff).cofAdjustment;
                }
            return cof;
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

        private bool CheckMovable() {
            if (!buffTypeDic.ContainsKey(BuffType.Control)) return true;

            foreach (var bh in buffTypeDic[BuffType.Control].Values) {
                if ((bh.Template as ControlBuff).movement) return false;
            }
            return true;
        }

        private bool CheckCastable()
        {
            if (!buffTypeDic.ContainsKey(BuffType.Control)) return true;

            foreach (var bh in buffTypeDic[BuffType.Control].Values) {
                if ((bh.Template as ControlBuff).castment) return false;
            }
            return true;
        }

        private bool CheckAPRecoverable()
        {
            if (!buffTypeDic.ContainsKey(BuffType.Control)) return true;

            foreach (var bh in buffTypeDic[BuffType.Control].Values) {
                if ((bh.Template as ControlBuff).recoverAP) return false;
            }
            return true;
        }
    }
}
