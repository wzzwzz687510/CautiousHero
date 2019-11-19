using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Wing.RPGSystem;

public class SkillSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public int skillID;

    public delegate void SkillBoard(int id,bool isStop);
    public SkillBoard SkillBoardEvent;

    //private void FixedUpdate()
    //{
    //    if (startCount && !isBoardDisplayed) {
    //        timer += Time.deltaTime;
    //        if (timer > duration) {
    //            OnSkillBoardShowed?.Invoke(skillID);
    //            isBoardDisplayed = true;
    //        }
    //    }
    //}

    public void OnPointerEnter(PointerEventData eventData)
    {
        SkillBoardEvent?.Invoke(skillID, false);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SkillBoardEvent?.Invoke(skillID, true);
    }
}
