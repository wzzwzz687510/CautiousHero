using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wing.RPGSystem
{
    public enum LootType
    {
        Skill,
        Coin,       
        Exp,
        Relic
    }

    [CreateAssetMenu(fileName = "Chest", menuName = "Wing/Scriptable Chests/BaseChest", order = 40)]
    public class BaseChest : ScriptableObject
    {
        public int coins;
        public BaseRelic[] relics;


    }

}

