using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wing.RPGSystem
{
    [System.Serializable]
    public class BuffHandler
    {
        public Entity CastEntity { get; private set; }
        public Entity TargetEntity { get; private set; }
        public BaseBuff TemplateBuff { get; private set; }
        public int LastTurn { get; private set; }
        public bool Infinity { get; private set; }


        public BuffHandler(Entity caster, Entity target, int buffhash)
        {
            CastEntity = caster;
            TargetEntity = target;
            TemplateBuff = BaseBuff.Dict[buffhash];
            LastTurn = TemplateBuff.lastTurn;
            Infinity = TemplateBuff.infinity;
        }

        public void ResetBuff(Entity caster)
        {
            CastEntity = caster;
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
            return false;
        }
    }

    [System.Serializable]
    public class BuffManager
    {
        public int entityHash;
        public Dictionary<BuffType, Dictionary<int, BuffHandler>> buffs = 
            new Dictionary<BuffType, Dictionary<int, BuffHandler>>();

        public BuffManager(int entityHash)
        {
            this.entityHash = entityHash;
        }

        public void AddBuff(BuffHandler bh)
        {
            int buffhash = bh.TemplateBuff.Hash;
            if (buffs.ContainsKey(bh.TemplateBuff.buffType)) {                
                if (buffs[bh.TemplateBuff.buffType].ContainsKey(buffhash)) {
                    buffs[bh.TemplateBuff.buffType][buffhash].ResetBuff(bh.CastEntity);
                }
                else {
                    buffs[bh.TemplateBuff.buffType].Add(buffhash, bh);
                }
            }
            else {
                buffs.Add(bh.TemplateBuff.buffType,new Dictionary<int, BuffHandler>());
                buffs[bh.TemplateBuff.buffType].Add(buffhash, bh);
            }
        }

        public void UpdateBuffs()
        {
            // Do something to host

            foreach (var buffDic in buffs.Values) {
                foreach (var buff in buffDic.Values) {
                    if (!buff.UpdateBuff()) {
                        buffDic.Remove(buff.TemplateBuff.Hash);
                    }
                }
            }
        }

        public EntityAttribute GetAttributeAdjustment()
        {
            EntityAttribute tmp = new EntityAttribute();
            foreach (var buffHandler in buffs?[BuffType.Attribute].Values) {
                tmp += (buffHandler.TemplateBuff as AttributeBuff).adjustValue;
            }

            return tmp;
        }

        public float GetConstDefense()
        {
            float ret = 0;
            foreach (var buffHandler in buffs?[BuffType.Defense].Values) {
                ret += (buffHandler.TemplateBuff as DefenseBuff).constReduction;
            }
            return ret;
        }

        public float GetCofDefense()
        {
            float ret = 0;
            foreach (var buffHandler in buffs?[BuffType.Defense].Values) {
                ret += (buffHandler.TemplateBuff as DefenseBuff).cofReduction;
            }
            return ret;
        }
    }
}
