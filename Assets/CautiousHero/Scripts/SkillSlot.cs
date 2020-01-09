using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

namespace Wing.RPGSystem
{
    public class SkillSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public int slotID;
        public Image frame;
        public Color selectColour;

        public bool isActive { get; private set; }

        public delegate void SkillBoard(int id, bool isExit);
        public SkillBoard SkillBoardEvent;

        public void OnPointerEnter(PointerEventData eventData)
        {
            SkillBoardEvent?.Invoke(slotID, false);
            frame.DOColor(selectColour, 0.1f);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            SkillBoardEvent?.Invoke(-1, true);
            frame.DOColor(Color.white, 0.1f);
        }
    }
}

