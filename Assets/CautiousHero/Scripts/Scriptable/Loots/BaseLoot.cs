using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wing.RPGSystem
{

    [CreateAssetMenu(fileName = "Loot", menuName = "Wing/Scriptable Loots/BaseLoot", order = 40)]
    public class BaseLoot : ScriptableObject
    {
        public int extraCoin;
        public int extraExp;
        public BaseEquipment[] equipments;

        public int RandomItem()
        {
            return equipments[equipments.Length.Random()].Hash;
        }
    }

}

