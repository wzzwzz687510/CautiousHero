using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wing.RPGSystem
{
    [CreateAssetMenu(fileName = "Skill", menuName = "Wing/Scriptable Skills/CombinedSkill", order = 13)]
    public class CombinedSkill : BaseSkill
    {
        public BaseSkill[] skillSequence;

        public override void ApplyEffect(int casterHash, Location castLoc)
        {
            foreach (var skill in skillSequence) {
                skill.ApplyEffect(casterHash, castLoc);
            }
        }
    }
}

