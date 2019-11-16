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
    public PlayerController player;

    void Awake()
    {
        player.OnHpChanged += OnPlayerHpChanged;        
    }

    public void UpdateUI()
    {
        // For Test
        for (int i = 0; i < 4; i++) {
            skills[i].sprite = player.skills[i].sprite;
        }
    }

    private void OnPlayerHpChanged(int value)
    {
        float tmp = 1.0f * value / player.MaxHealthPoints;
        DOTween.To(() => slider_playerHp.value, ratio => slider_playerHp.value = ratio, tmp, 1);
        hpFill.fillAmount = tmp;
        //hpFill.color = slider_playerHp.value > 0.2f ? new Color(0.5f, 1, 0.4f) : Color.red;
        text_playerHp.text = value.ToString() + "/" + player.MaxHealthPoints.ToString();
    }
}
