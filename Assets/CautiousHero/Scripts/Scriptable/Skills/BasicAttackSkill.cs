using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wing.RPGSystem
{
    [CreateAssetMenu(fileName = "Skill", menuName = "Wing/Scriptable Skills/BasicAttackSkill", order = 10)]
    public class BasicAttackSkill : ValueBasedSkill
    {
        public AdditiveAttribute attribute;
        public ElementMana cost;

        public override int CalculateValue(int casterHash,float cof)
        {
            float attributeAdjusment = ApplyAttributeAdjustment(casterHash, baseValue);
            return Mathf.RoundToInt(cof * ApplyResistanceAdjustment(casterHash, attributeAdjusment));
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
            extraValue = Mathf.Max(0, extraValue);

            return (baseValue + attributeMag * extraValue) * caster.EntityBuffManager.GetDamageAdjustment();
        }

        public virtual float ApplyResistanceAdjustment(int casterHash,float baseValue)
        {
            Entity caster = casterHash.GetEntity();
            int resistanceValue = 0;
            switch (skillElement) {
                case ElementType.None:
                    resistanceValue = caster.Resistance.physcialResistance;
                    break;
                case ElementType.Fire:
                    resistanceValue = caster.Resistance.Fire;
                    break;
                case ElementType.Water:
                    resistanceValue = caster.Resistance.Water;
                    break;
                case ElementType.Earth:
                    resistanceValue = caster.Resistance.Earth;
                    break;
                case ElementType.Air:
                    resistanceValue = caster.Resistance.Air;
                    break;
                case ElementType.Light:
                    resistanceValue = caster.Resistance.Light;
                    break;
                case ElementType.Dark:
                    resistanceValue = caster.Resistance.Dark;
                    break;
                default:
                    break;
            }
            return (1 - resistanceValue / 200.0f) * baseValue;
        }
    }
}
