using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wing.RPGSystem
{
    //[System.Serializable]
    //public struct SubSet<T>
    //{
    //    public T[] defaultSet;
    //    public T[] lockedSet;
    //}

    [System.Serializable]
    public struct SubRelicSet
    {
        public BaseRelic[] defaultSet;
        public BaseRelic[] lockedSet;
    }

    [CreateAssetMenu(fileName = "RandomPool", menuName = "Wing/Configs/RandomPool", order = 1)]
    public class RandomPool : ScriptableObject
    {
        public SkillSet battle_skillSet;
        public SkillSet guild_skillSet;
        public SkillSet event_skillSet;

        public SubRelicSet battle_relicSet;        
        public SubRelicSet shop_relicSet;       
        public SubRelicSet event_relicSet;
    }
}

