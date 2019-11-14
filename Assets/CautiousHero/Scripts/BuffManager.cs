using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wing.RPGSystem
{
    [System.Serializable]
    public class BuffHandler
    {
        public Entity castEntity { get; private set; }
        public Entity targetEntity { get; private set; }
        public BaseBuff ScriptableBuff { get; private set; }
        public int LastTurn { get; private set; }
        public bool Infinity { get; private set; }


        public BuffHandler(Entity caster, Entity target, BaseBuff buff)
        {
            castEntity = caster;
            targetEntity = target;
            ScriptableBuff = buff;
            LastTurn = buff.lastTurn;
            Infinity = buff.infinity;
        }

        public void ResetBuff(Entity caster)
        {
            castEntity = caster;
            LastTurn = ScriptableBuff.lastTurn;
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
        public Dictionary<BaseBuff, BuffHandler> buffDic = new Dictionary<BaseBuff, BuffHandler>();

        public void AddBuff(BuffHandler buff)
        {
            if (buffDic.ContainsKey(buff.ScriptableBuff)) {
                buffDic[buff.ScriptableBuff].ResetBuff(buff.castEntity);
            }
            else {
                buffDic.Add(buff.ScriptableBuff, buff);
            }
        }

        public void UpdateBuffs()
        {
            foreach (var buffHandler in buffDic.Values) {
                buffHandler.UpdateBuff();
            }
        }

        public EntityAttribute GetAttributeAdjustment()
        {
            EntityAttribute tmp = new EntityAttribute();
            foreach (var buffHandler in buffDic.Values) {
                if (buffHandler.ScriptableBuff.buffType == BuffType.Attribute)
                    tmp += (buffHandler.ScriptableBuff as AttributeBuff).adjustValue;
            }

            return tmp;
        }

        public float GetReduceCof(BuffType type)
        {
            float tmp = 0;
            foreach (var buffHandler in buffDic.Values) {
                {
                    if (buffHandler.ScriptableBuff.buffType == type)
                        tmp = 1 - (1 - tmp) * (1 - (buffHandler.ScriptableBuff as DicountBuff).reduceCof);
                }
            }
            return tmp;
        }

        public int GetReduceConstant(BuffType type)
        {
            int tmp = 0;
            foreach (var buffHandler in buffDic.Values) {
                {
                    if (buffHandler.ScriptableBuff.buffType == type)
                        tmp += (buffHandler.ScriptableBuff as DicountBuff).reduceConst;
                }
            }
            return tmp;
        }
    }
}
