using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Wing.RPGSystem
{
    public class InfoBoard : MonoBehaviour
    {
        [Header("Setting")]
        public Color[] colors;
        public Vector2 baseRect;
        public int charLengthPerRow;
        public float hightPerRow;

        [Header("UI Elements")]
        public RectTransform m_rectT;
        public Text title;
        public Text description;
        public Image header;
        public Toggle[] apcostImgs;

        public void UpdateToSkillBoard(int skillHash)
        {
            BaseSkill skill = skillHash.GetBaseSkill();
            title.text = skill.skillName;
            Color c = colors[(int)skill.skillElement];
            string element = skill.damageType == DamageType.Physical ? "Physical" : skill.skillElement.ToString();
            
            description.text = skill.description;
            if (skill.labels.Contains(Label.Damage)) {
                var vSkill = skill as BasicAttackSkill;
                int adjustmentDamage = vSkill.baseValue;
                if (BattleManager.Instance.IsInBattle) {
                    if (vSkill.attribute == AdditiveAttribute.Strength) {
                        adjustmentDamage += AreaManager.Instance.character.Strength;
                    }
                    else if (vSkill.attribute == AdditiveAttribute.Intelligence) {
                        adjustmentDamage += AreaManager.Instance.character.Intelligence;
                    }
                    else {
                        adjustmentDamage += AreaManager.Instance.character.Agility;
                    }
                }
                description.text += string.Format("Deal <color=#{0:X2}{1:X2}{2:X2}>{3} {4}</color> damage to target",
                (int)(c.r * 255), (int)(c.g * 255), (int)(c.b * 255), adjustmentDamage, element);
            }
            header.color = colors[(int)skill.skillElement];
            for (int i = 0; i < apcostImgs.Length; i++) {
                apcostImgs[i].gameObject.SetActive(i < skill.actionPointsCost);
            }

            m_rectT.sizeDelta = baseRect + new Vector2(0, (description.text.Length / charLengthPerRow + 1) * hightPerRow);
        }

        public void UpdateToBuffBoard(int buffHash)
        {
            for (int i = 0; i < apcostImgs.Length; i++) {
                apcostImgs[i].gameObject.SetActive(false);
            }

            BaseBuff buff = buffHash.GetBaseBuff();
            title.text = buff.buffName;
            description.text = buff.description;
            header.color = Color.grey;

            m_rectT.sizeDelta = baseRect + new Vector2(0, (description.text.Length / charLengthPerRow + 1) * hightPerRow);
        }

        public void UpdateToRelicBoard(int relicHash)
        {
            for (int i = 0; i < apcostImgs.Length; i++) {
                apcostImgs[i].gameObject.SetActive(false);
            }

            BaseRelic relic = relicHash.GetRelic();
            title.text = relic.relicName;
            description.text = relic.description;
            header.color = Color.yellow;

            m_rectT.sizeDelta = baseRect + new Vector2(0, (description.text.Length / charLengthPerRow + 1) * hightPerRow);
        }
    }
}

