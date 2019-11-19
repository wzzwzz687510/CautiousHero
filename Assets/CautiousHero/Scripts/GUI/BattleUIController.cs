using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Wing.RPGSystem;

public class BattleUIController : MonoBehaviour
{
    public PlayerController player;

    [Header("HP Visual")]
    public Slider slider_playerHp;
    public Text text_playerHp;
    public Image hpFill;

    [Header("Skill Visual")]
    public Image[] skills;
    public Image[] skillCovers;

    [Header("AP Visual")]
    public Toggle[] toggle_aps;

    [Header("Turn Switch Visual")]
    public Image image_turnBG;
    public Text text_turnText;

    [Header("Battle End Visual")]
    public Image image_blackBG;
    public Image image_die;

    void Awake()
    {

    }

    private void Start()
    {
        player.OnSkillUpdated += OnPlayerSkillUpdated;
        player.OnHPChanged += OnPlayerHPChanged;
        player.OnAPChanged.AddListener(OnPlayerAPChanged);

        BattleManager.Instance.OnTurnSwitched += OnTurnSwitched;
        AnimationManager.Instance.OnGameoverEvent.AddListener(Gameover);
    }

    private void OnDestroy()
    {
        player.OnAPChanged.RemoveListener(OnPlayerAPChanged);
    }

    public void UpdateUI()
    {
        // For Test
        for (int i = 0; i < 4; i++) {
            skills[i].sprite = player.Skills[i].sprite;
        }
    }

    private void OnPlayerHPChanged(float hpRatio,float duration)
    {
        DOTween.To(() => slider_playerHp.value, ratio => slider_playerHp.value = ratio, hpRatio, duration);
        hpFill.fillAmount = hpRatio;
        hpFill.color = hpRatio > 0.2f ? new Color(0.5f, 1, 0.4f) : Color.red;
        text_playerHp.text = ((int)(hpRatio * player.MaxHealthPoints)).ToString() + "/" + player.MaxHealthPoints.ToString();
    }

    private void OnPlayerAPChanged()
    {
        for (int i = 0; i < 8; i++) {
            toggle_aps[i].isOn = i + 1 <= player.ActionPoints;
        }
    }

    private void OnPlayerSkillUpdated(int skillID,int cooldown)
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

        text_turnText.text = text;
        text_turnText.color = Color.white;
        image_turnBG.fillAmount = 0;
        image_turnBG.color = Color.white;
        DOTween.To(() => image_turnBG.fillAmount, ratio => image_turnBG.fillAmount = ratio, 1f, 0.5f);
        yield return new WaitForSeconds(0.5f);
        DOTween.ToAlpha(() => text_turnText.color, color => text_turnText.color = color, 1f, 0.5f);
        yield return new WaitForSeconds(0.5f);
        DOTween.ToAlpha(() => image_turnBG.color, color => image_turnBG.color = color, 0f, 0.5f);
        DOTween.ToAlpha(() => text_turnText.color, color => text_turnText.color = color, 0f, 0.5f);
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

  
}
