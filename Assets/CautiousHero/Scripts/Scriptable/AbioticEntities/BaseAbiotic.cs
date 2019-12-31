using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wing.RPGSystem
{
    [CreateAssetMenu(fileName = "Abiotic", menuName = "Wing/Scriptable Abiotic/BaseAbiotic", order = 30)]
    public class BaseAbiotic : ScriptableObject
    {
        public string abioticName = "New Abiotic";
        public string description = "A secret abiotic";
        public Sprite[] sprite;
        public EntityAttribute attribute;

        public List<BaseBuff> buffs;
    }
}

