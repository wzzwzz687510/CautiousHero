using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wing.RPGSystem
{
    [CreateAssetMenu(fileName = "Skill", menuName = "Wing/Scriptable Skills/BasicAttackSkill", order = 2)]
    public class BasicAttackSkill : ValueBasedSkill
    {
        public AdditiveAttribute attribute;

        public override int CalculateValue(Entity caster,float cof)
        {
            int aa = 0;
            if (attribute == AdditiveAttribute.Agility)
                aa = caster.Agility;
            else if (attribute == AdditiveAttribute.Intelligence)
                aa = caster.Intelligence;
            else if (attribute == AdditiveAttribute.Strength)
                aa = caster.Strength;

            return Mathf.RoundToInt(cof * baseValue * (1 + attributeCof * aa));
        }
    }
}
