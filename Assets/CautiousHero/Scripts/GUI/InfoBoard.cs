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

        [Header("UI Elements")]
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
            description.text = !skill.labels.Contains(Label.Damage) ? skill.description :
                string.Format("Deal <color=#{0:X2}{1:X2}{2:X2}>{3} {4}</color> damage to target",
                 (int)(c.r * 255), (int)(c.g * 255), (int)(c.b * 255), (skill as ValueBasedSkill).baseValue, element);
            header.color = colors[(int)skill.skillElement];
            for (int i = 0; i < apcostImgs.Length; i++) {
                apcostImgs[i].gameObject.SetActive(i < skill.actionPointsCost);
            }
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
        }
    }
}

