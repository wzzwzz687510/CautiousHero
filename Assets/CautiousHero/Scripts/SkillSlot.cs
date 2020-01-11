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
        public Image icon;
        public Image frame;
        public Color selectColour;

        private UnityAction<int> DisplayAction;
        private UnityAction hideAction;

        public void RegisterDisplayAction(UnityAction<int> action)
        {
            this.DisplayAction = action;
        }

        public void RegisterHideAction(UnityAction action)
        {
            hideAction = action;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            DisplayAction?.Invoke(slotID);
            frame?.DOColor(selectColour, 0.1f);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            hideAction?.Invoke();
            frame?.DOColor(Color.white, 0.1f);
        }

        private void OnDisable()
        {
            hideAction?.Invoke();
        }
    }
}

