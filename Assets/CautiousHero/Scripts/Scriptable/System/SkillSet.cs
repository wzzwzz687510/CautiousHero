using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Wing.RPGSystem
{
    [System.Serializable]
    public struct SubSkillSet
    {
        public BaseSkill[] defaultSet;
        public BaseSkill[] lockedSet;
    }

    [CreateAssetMenu(fileName = "SkillSet", menuName = "Wing/Configs/SkillSet", order = 1)]
    public class SkillSet : ScriptableObject
    {
        public SubSkillSet commonSet;
        public SubSkillSet squireSet;
        public SubSkillSet knightSet;
        public SubSkillSet rangerSet;
        public SubSkillSet mageSet;
        public SubSkillSet monkSet;
        public SubSkillSet ninjaSet;
        public SubSkillSet lancerSet;
    }
}

