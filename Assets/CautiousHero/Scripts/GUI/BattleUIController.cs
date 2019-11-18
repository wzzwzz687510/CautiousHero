using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Wing.RPGSystem;

public class BattleUIController : MonoBehaviour
{
    public Slider slider_playerHp;
    public Text text_playerHp;
    public Image hpFill;
    public Image[] skills;
    public Image[] skillCovers;
    public PlayerController player;
    public Toggle[] toggle_aps;

    public Image blackBG;
    public Image die;

    void Awake()
    {
        player.OnHPDropped += OnPlayerHPChanged;
        player.OnSkillUpdated += OnPlayerSkillUpdated;
        player.OnAPChanged.AddListener(OnPlayerAPChanged);
        BattleManager.Instance.OnGameoverEvent.AddListener(Gameover);
    }

    private void OnDestroy()
    {
        player.OnHPDropped -= OnPlayerHPChanged;
        player.OnAPChanged.RemoveListener(OnPlayerAPChanged);
    }

    public void UpdateUI()
    {
        // For Test
        for (int i = 0; i < 4; i++) {
            skills[i].sprite = player.Skills[i].sprite;
        }
    }

    private void OnPlayerHPChanged(bool isDrop)
    {
        float tmp = 1.0f * player.HealthPoints / player.MaxHealthPoints;
        DOTween.To(() => slider_playerHp.value, ratio => slider_playerHp.value = ratio, tmp, 1);
        hpFill.fillAmount = tmp;
        hpFill.color = tmp > 0.2f ? new Color(0.5f, 1, 0.4f) : Color.red;
        text_playerHp.text = player.HealthPoints.ToString() + "/" + player.MaxHealthPoints.ToString();
    }

    private void OnPlayerAPChanged()
    {
        for (int i = 0; i < 8; i++) {
            toggle_aps[i].isOn = i + 1 <= player.ActionPoints;
        }
    }

    private void OnPlayerSkillUpdated(int skillID,int cooldown)
    {
        skillCovers[skillID].fillAmount = 1.0f * cooldown / player.Skills[skillID].cooldownTime;
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
        DOTween.ToAlpha(() => blackBG.color, color => blackBG.color = color, 0.7f, 0.5f);
        DOTween.ToAlpha(() => die.color, color => die.color = color, 1, 2);
    }
}
