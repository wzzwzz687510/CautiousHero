using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Wing.RPGSystem;

public class SkillSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public int skillID;

    public delegate void SkillBoard(int id,bool isExit);
    public SkillBoard SkillBoardEvent;

    public void OnPointerEnter(PointerEventData eventData)
    {
        SkillBoardEvent?.Invoke(skillID, false);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SkillBoardEvent?.Invoke(-1, true);
    }
}
