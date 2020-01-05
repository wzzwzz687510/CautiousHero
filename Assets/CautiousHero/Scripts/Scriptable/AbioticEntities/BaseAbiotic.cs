using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wing.RPGSystem
{
    [CreateAssetMenu(fileName = "Abiotic", menuName = "Wing/Scriptable Abiotic/BaseAbiotic", order = 35)]
    public class BaseAbiotic : ScriptableObject
    {
        public string abioticName;
        public string description;
        public Sprite[] sprite;
        public EntityAttribute attribute;

        public List<BaseBuff> buffs;
    }
}

