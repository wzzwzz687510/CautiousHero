using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Wing.RPGSystem;

public class BattleUIController : MonoBehaviour
{
    public PlayerController player;

    [Header("HP Visual")]
    public Slider playerHPBar;
    public Text playerHpText;
    public Image hpFill;

    [Header("Skill Visual")]
    public Image[] skills;
    public Image[] skillCovers;

    [Header("AP Visual")]
    public Toggle[] aps;

    [Header("Turn Switch Visual")]
    public Image turnBG;
    public Text turnText;

    [Header("Battle End Visual")]
    public Image image_blackBG;
    public Image image_die;

    [Header("Skill Board")]
    public float waitDuration;
    public Transform skillBoard;
    public Text skillName;
    public Text skillCost;
    public Text skillCooldown;
    public Text skillValue;
    public Text skillType;
    public Text skillElement;
    public Text skillCastType;
    public SkillSlot[] skillSlots;
    private int selectSkillID=-1;
    private float timer;
    private bool startCount;
    private bool isBoardDisplayed;

    void Awake()
    {

    }

    private void FixedUpdate()
    {
        if (startCount && !isBoardDisplayed) {
            timer += Time.deltaTime;
            if (timer > waitDuration) {
                ShowSkillBoard();
                isBoardDisplayed = true;
            }
        }else if (!startCount&&timer>0) {
            timer -= Time.deltaTime;
        }
    }

    private void Start()
    {

    }

    private void OnDestroy()
    {
        player.OnAPChanged.RemoveListener(OnPlayerAPChanged);
    }

    public void Init()
    {
        player.OnSkillUpdated += OnPlayerSkillUpdated;
        player.OnHPChanged += OnPlayerHPChanged;
        player.OnAPChanged.AddListener(OnPlayerAPChanged);
        for (int i = 0; i < 4; i++) {
            skillSlots[i].SkillBoardEvent += OnSkillBoardEvent;
        }

        BattleManager.Instance.OnTurnSwitched += OnTurnSwitched;
        AnimationManager.Instance.OnGameoverEvent.AddListener(Gameover);

        for (int i = 0; i < 4; i++) {
            skills[i].sprite = player.Skills[i].sprite;
        }

        OnPlayerHPChanged(0, 0);
        OnPlayerHPChanged(1, 2);
    }

    private void OnPlayerHPChanged(float hpRatio, float duration)
    {
        if(hpRatio< playerHPBar.value) {
            hpFill.fillAmount = hpRatio;
        }
        else {
            DOTween.To(() => hpFill.fillAmount, ratio => hpFill.fillAmount = ratio, hpRatio, duration);
        }
        DOTween.To(() => playerHPBar.value, ratio => playerHPBar.value = ratio, hpRatio, duration);
        hpFill.color = hpRatio > 0.2f ? new Color(0.5f, 1, 0.4f) : Color.red;
        playerHpText.text = ((int)(hpRatio * player.MaxHealthPoints)).ToString() + "/" + player.MaxHealthPoints.ToString();
    }

    private void OnPlayerAPChanged()
    {
        for (int i = 0; i < 8; i++) {
            aps[i].isOn = i + 1 <= player.ActionPoints;
        }
    }

    private void OnPlayerSkillUpdated(int skillID, int cooldown)
    {
        float tmp = Mathf.Clamp01(1.0f * cooldown / player.Skills[skillID].cooldownTime);
        if (tmp == 1) {
            skillCovers[skillID].fillAmount = tmp;
        }
        else {
            DOTween.To(() => skillCovers[skillID].fillAmount, ratio => skillCovers[skillID].fillAmount = ratio, tmp, 1);
        }
    }

    public void OnTurnSwitched(bool isPlayerTurn)
    {
        StartCoroutine(TurnSwitchAnimation(isPlayerTurn));
    }

    public IEnumerator TurnSwitchAnimation(bool isPlayerTurn)
    {
        while (AnimationManager.Instance.IsPlaying) {
            yield return null;
        }
        string text;
        if (isPlayerTurn) {
            text = "我  方  回  合";
        }
        else {
            text = "敌  方  回  合";
        }

        turnText.text = text;
        turnText.color = Color.white;
        turnBG.fillAmount = 0;
        turnBG.color = Color.white;
        DOTween.To(() => turnBG.fillAmount, ratio => turnBG.fillAmount = ratio, 1f, 0.5f);
        yield return new WaitForSeconds(0.5f);
        DOTween.ToAlpha(() => turnText.color, color => turnText.color = color, 1f, 0.5f);
        yield return new WaitForSeconds(0.5f);
        DOTween.ToAlpha(() => turnBG.color, color => turnBG.color = color, 0f, 0.5f);
        DOTween.ToAlpha(() => turnText.color, color => turnText.color = color, 0f, 0.5f);
        yield return new WaitForSeconds(0.5f);

        BattleManager.Instance.StartNewTurn(isPlayerTurn);
    }

    public void Button_CancelMove()
    {
        BattleManager.Instance.CancelMove();
    }

    public void Button_EndTurn()
    {
        BattleManager.Instance.EndTurn();
    }

    public void Button_CastSkill(int skillID)
    {
        BattleManager.Instance.CastSkill(skillID);
    }

    public void Gameover()
    {
        DOTween.ToAlpha(() => image_blackBG.color, color => image_blackBG.color = color, 0.7f, 0.5f);
        DOTween.ToAlpha(() => image_die.color, color => image_die.color = color, 1, 2);
        image_blackBG.raycastTarget = true;
    }

    public void OnSkillBoardEvent(int id, bool isStop)
    {
        if (selectSkillID == id && !isStop)
            return;
        selectSkillID = id;
        startCount = !isStop;
        isBoardDisplayed &= false;
        if (!isStop && timer > waitDuration) {
            ShowSkillBoard();
        }
        else if (isStop) {
            selectSkillID = -1;
            skillBoard.position = new Vector3(-300, -300, 0);
        }

    }

    public void ShowSkillBoard()
    {
        var skill = player.Skills[selectSkillID];
        skillName.text = skill.skillName;
        skillCost.text = skill.actionPointsCost.ToString();
        skillCooldown.text = skill.cooldownTime.ToString();
        skillValue.text = (skill as ValueBasedSkill).baseValue.ToString();
        skillType.text = skill.skillType.ToString();
        skillElement.text = skill.skillElement.ToString();
        skillCastType.text = skill.castType.ToString();
        skillBoard.position = skillSlots[selectSkillID].transform.position + new Vector3(0, 150, 0);
    }


}
