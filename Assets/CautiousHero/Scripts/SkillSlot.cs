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

        public bool IsActive { get; private set; }
        public UnityEvent CheckSlotState;

        public void OnPointerEnter(PointerEventData eventData)
        {
            IsActive = true;
            CheckSlotState?.Invoke();
            frame.DOColor(selectColour, 0.1f);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            IsActive = false;
            CheckSlotState?.Invoke();
            frame.DOColor(Color.white, 0.1f);
        }

        private void OnDisable()
        {
            IsActive = false;
            CheckSlotState?.Invoke();
        }
    }
}

