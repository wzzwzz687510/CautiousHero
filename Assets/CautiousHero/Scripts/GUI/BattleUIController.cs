using DG.Tweening;
using System;
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
    public Color[] colors;
    public Image skillNameBg;
    private int selectSkillID=-1;
    private bool isSkillBoardDisplayed;

    private float timer;
    private bool startCount;

    [Header("Creature Board")]
    public Transform creatureBoard;
    public Image creatureSprite;
    public Text creatureName;
    public Text creatureLv;
    public Text creatureHP;
    public Text creatureAP;
    public Text creatureResistance;
    public Text creatureElement;
    private int selectCreatureID = -1;
    private bool isCreatureBoardDisplayed;

    void Awake()
    {

    }

    private void FixedUpdate()
    {
        if (startCount && !isSkillBoardDisplayed && !isCreatureBoardDisplayed) {
            timer += Time.deltaTime;
            if (timer > waitDuration) {
                if (selectSkillID != -1) {
                    ShowSkillBoard();
                    isSkillBoardDisplayed = true;
                }
                else if (selectCreatureID != -1) {
                    ShowCreatureBoard();
                    isCreatureBoardDisplayed = true;
                }
            }
        }
        else if (!startCount && timer > 0) {
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
        player.HPChangeAnimation += PlayerHPChangeAnimation;
        player.ssAnimEvent += PlayerSkillShiftAnimation;
        player.OnAPChanged.AddListener(OnPlayerAPChanged);
        for (int i = 0; i < 4; i++) {
            skillSlots[i].SkillBoardEvent += OnSkillBoardEvent;
        }

        BattleManager.Instance.OnTurnSwitched += OnTurnSwitched;
        BattleManager.Instance.CreatureBoardEvent += OnCreatureBoardEvent;
        AnimationManager.Instance.OnGameoverEvent.AddListener(Gameover);

        for (int i = 0; i < 4; i++) {
            skills[i].sprite = player.Skills[i].sprite;
        }

        PlayerHPChangeAnimation(0, 0);
        PlayerHPChangeAnimation(1, 2);
        StartCoroutine(BattleStart());
    }

    private void PlayerSkillShiftAnimation(float duration)
    {
        throw new NotImplementedException();
    }

    private void PlayerHPChangeAnimation(float hpRatio, float duration)
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

    public IEnumerator BattleStart()
    {
        yield return new WaitForSeconds(1f);
        turnText.text = "战  略  布  局";
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

    public void OnSkillBoardEvent(int id, bool isExit)
    {
        if (selectSkillID == id && !isExit)
            return;
        selectSkillID = id;
        startCount = !isExit;
        isSkillBoardDisplayed &= false;
        if (!isExit && timer > waitDuration) {
            ShowSkillBoard();
        }
        else if (isExit) {
            skillBoard.position = new Vector3(-300, 0, 0);
        }
    }

    public void OnCreatureBoardEvent(int hash, bool isExit)
    {
        if (selectCreatureID == hash && !isExit)
            return;
        selectCreatureID = hash;
        startCount = !isExit;
        isCreatureBoardDisplayed &= false;
        if (!isExit && timer > waitDuration) {
            ShowCreatureBoard();
        }
        else if (isExit) {
            creatureBoard.position = new Vector3(-300, 0, 0);
        }
    }

    public void ShowSkillBoard()
    {
        var skill = player.Skills[selectSkillID] as ValueBasedSkill;
        skillName.text = skill.skillName;
        skillCost.text = skill.actionPointsCost.ToString();
        skillCooldown.text = skill.cooldownTime.ToString();
        skillValue.text = skill.baseValue.ToString() + " + <color=#ffa500ff>" + player.Intelligence * skill.attributeCof * skill.baseValue +"</color>";
        switch (skill.damageType) {
            case DamageType.Physical:
                skillNameBg.color = colors[0];
                skillType.text = "物理";
                break;
            case DamageType.Magical:
                skillType.text = "魔法";
                break;
            case DamageType.Pure:
                skillType.text = "纯粹";
                skillNameBg.color = colors[1];
                break;
            default:
                break;
        }
        switch (skill.skillElement) {
            case SkillElement.None:
                skillElement.text = "无";
                break;
            case SkillElement.Fire:
                skillElement.text = "火";
                skillNameBg.color = colors[2];
                break;
            case SkillElement.Water:
                skillElement.text = "水";
                skillNameBg.color = colors[3];
                break;
            case SkillElement.Earth:
                skillElement.text = "地";
                skillNameBg.color = colors[4];
                break;
            case SkillElement.Air:
                skillElement.text = "气";
                skillNameBg.color = colors[5];
                break;
            case SkillElement.Light:
                skillElement.text = "光";
                skillNameBg.color = colors[6];
                break;
            case SkillElement.Dark:
                skillElement.text = "暗";
                skillNameBg.color = colors[7];
                break;
            default:
                break;
        }
        switch (skill.castType) {
            case CastType.Instant:
                skillCastType.text = "瞬间";
                break;
            case CastType.Trajectory:
                skillCastType.text = "弹道";
                break;
            default:
                break;
        }
        skillBoard.position = skillSlots[selectSkillID].transform.position + new Vector3(0, 150, 0);
    }

    public void ShowCreatureBoard()
    {
        if (EntityManager.Instance.TryGetEntity(selectCreatureID, out Entity entity)) {
            var cc = (entity as CreatureController).Template;
            creatureSprite.sprite = cc.sprite;
            creatureName.text = cc.creatureName;
            if (entity.Intelligence <= player.Intelligence) {
                creatureHP.text = cc.attribute.maxHealth.ToString();
                creatureAP.text = cc.attribute.action.ToString();
                if(cc.skills.Length==0)
                    creatureResistance.text = "无";
                else
                switch (cc.skills[0].skillElement) {
                    case SkillElement.None:
                        creatureResistance.text = "无";
                        break;
                    case SkillElement.Fire:
                        creatureResistance.text = "火";
                        break;
                    case SkillElement.Water:
                        creatureResistance.text = "水";
                        break;
                    case SkillElement.Earth:
                        creatureResistance.text = "地";
                        break;
                    case SkillElement.Air:
                        creatureResistance.text = "气";
                        break;
                    case SkillElement.Light:
                        creatureResistance.text = "光";
                        break;
                    case SkillElement.Dark:
                        creatureResistance.text = "暗";
                        break;
                    default:
                        break;
                }
                creatureElement.text = creatureResistance.text;
            }
            else {
                creatureLv.text = "?";
                creatureHP.text = "?";
                creatureAP.text = "?";
                creatureResistance.text = "?";
                creatureElement.text = "?";
            }
            creatureBoard.position = new Vector3(10, 1070, 0);
        }
    }


}
