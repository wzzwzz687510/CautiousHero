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
        public Toggle[] apcostToggles;

        public void UpdateToSkillBoard(int skillHash)
        {
            BaseSkill skill = skillHash.GetBaseSkill();
            title.text = skill.skillName;
            description.text = skill.description;
            header.color = colors[(int)skill.skillElement];
            for (int i = 0; i < apcostToggles.Length; i++) {
                apcostToggles[i].gameObject.SetActive(i < skill.actionPointsCost);
            }
        }
    }
}

