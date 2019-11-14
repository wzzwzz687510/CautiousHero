using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Wing.RPGSystem;

public class BattleUIController : MonoBehaviour
{
    public Slider slider_playerHp;
    public Slider slider_playerMp;
    public Text text_playerHp;
    public Text text_playerMp;
    public Image hpFill;
    public PlayerController player;

    // Start is called before the first frame update
    void Awake()
    {
        player.OnHpChanged += OnPlayerHpChanged;
        player.OnMpChanged += OnPlayerMpChanged;
    }

    private void OnPlayerHpChanged(int value)
    {
        DOTween.To(() => slider_playerHp.value, ratio => slider_playerHp.value = ratio, 1.0f * value / player.MaxHealthPoints, 1);
        hpFill.color = slider_playerHp.value > 0.2f ? new Color(0.5f, 1, 0.4f) : Color.red;
        text_playerHp.text = value.ToString() + "/" + player.MaxHealthPoints.ToString();
    }

    private void OnPlayerMpChanged(int value)
    {
        DOTween.To(() => slider_playerMp.value, ratio => slider_playerMp.value = ratio, 1.0f * value / player.MaxManaPoints, 1);
        text_playerMp.text = value.ToString() + "/" + player.MaxManaPoints.ToString();
    }
}
