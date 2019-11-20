using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wing.RPGSystem
{
    [CreateAssetMenu(fileName = "Skill", menuName = "Wing/Scriptable Skills/BasicHealSkill", order = 3)]
    public class BasicHealSkill : ValueBasedSkill
    {

        public override int CalculateValue(Entity caster, float cof)
        {
            return -Mathf.RoundToInt(cof * baseValue * (1 + levelCof * caster.Level + attributeCof * caster.Intelligence));
        }
    }
}

