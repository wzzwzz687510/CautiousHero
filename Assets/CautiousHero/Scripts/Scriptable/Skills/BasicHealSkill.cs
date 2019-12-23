using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wing.RPGSystem
{
    [CreateAssetMenu(fileName = "Skill", menuName = "Wing/Scriptable Skills/BasicHealSkill", order = 3)]
    public class BasicHealSkill : ValueBasedSkill
    {

        public override int CalculateValue(int casterHash, float cof)
        {
            Entity caster = casterHash.GetEntity();
            return -Mathf.RoundToInt(cof * baseValue * (1 + attributeCof * caster.Intelligence));
        }
    }
}

