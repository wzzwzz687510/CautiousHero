using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wing.RPGSystem
{
    public class BuffHandler : MonoBehaviour
    {
        public BaseBuff ScriptableBuff { get; private set; }
        public int LastTurn { get; private set; }
        public bool Infinity { get; private set; }

        public BuffHandler(BaseBuff buff)
        {
            ScriptableBuff = buff;
            LastTurn = buff.lastTurn;
            Infinity = buff.infinity;
        }

        /// <summary>
        /// Cast this buff
        /// </summary>
        /// <returns>True for last to next turn</returns>
        public bool CastBuff()
        {
            if (Infinity || --LastTurn > 0)
                return true;
            return false;
        }
    }
}
