using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wing.RPGSystem
{
    public interface IMagicalCost
    {
        void ApplyMagicCost(int casterHash);
    }

    [CreateAssetMenu(fileName = "Skill", menuName = "Wing/Scriptable Skills/BasicAttackSkill", order = 10)]
    public class BasicAttackSkill : ValueBasedSkill, IMagicalCost
    {
        public AdditiveAttribute attribute;
        public ElementMana cost;

        public override int CalculateValue(int casterHash,float cof)
        {
            float attributeAdjusment = ApplyAttributeAdjustment(casterHash, baseValue);
            return Mathf.RoundToInt(cof * ApplyResistanceAdjustment(casterHash, attributeAdjusment));
        }

        public virtual void ApplyMagicCost(int casterHash)
        {
            throw new System.NotImplementedException();
        }

        public virtual float ApplyAttributeAdjustment(int casterHash, float baseValue)
        {
            Entity caster = casterHash.GetEntity();
            int extraValue = 0;
            if (attribute == AdditiveAttribute.Agility)
                extraValue = caster.Agility;
            else if (attribute == AdditiveAttribute.Intelligence)
                extraValue = caster.Intelligence;
            else if (attribute == AdditiveAttribute.Strength)
                extraValue = caster.Strength;

            return  baseValue + attributeMag * extraValue;
        }

        public virtual float ApplyResistanceAdjustment(int casterHash,float baseValue)
        {
            Entity caster = casterHash.GetEntity();
            switch (skillElement) {
                case ElementType.None:
                    return caster.Resistance.physcialResistance
                case ElementType.Fire:
                    break;
                case ElementType.Water:
                    break;
                case ElementType.Earth:
                    break;
                case ElementType.Air:
                    break;
                case ElementType.Light:
                    break;
                case ElementType.Dark:
                    break;
                default:
                    break;
            }
        }
    }
}
