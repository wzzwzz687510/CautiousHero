using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Wing.RPGSystem;

public class BattleUIController : MonoBehaviour
{
    public PlayerController player;
    public GameObject battleUI;

    [Header("HP Visual")]
    public Slider playerHPBar;
    public Text playerHpText;
    public Image hpFill;

    [Header("Armour Visual")]
    public Image physicalArmourFill;
    public Image magicalArmourFill;
    public Text physicalArmourText;
    public Text magicalArmourText;

    [Header("Skill Visual")]
    public Image[] skills;
    public Image[] skillCovers;
    public Sprite unknownSkill;

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
    public Button endTurnButton;
    public Text skillName;
    public Text skillCost;
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
        endTurnButton.gameObject.SetActive(false);
        BindEvent();
    }

    public void EnterAreaAnim()
    {  
        UpdateSkillSprites();

        PlayerHPChangeAnimation(0, 0);
        PlayerHPChangeAnimation(1, 2);
        PlayerArmourPointsChangeAnimation(true, player.PhysicalArmourPoints);
        PlayerArmourPointsChangeAnimation(false, player.MagicalArmourPoints);
    }

    private void BindEvent()
    {
        player.OnMovedEvent += OnPlayerMovedEvent;
        player.HPChangeAnimation += PlayerHPChangeAnimation;
        player.ArmourPointsChangeAnimation += PlayerArmourPointsChangeAnimation;
        player.OnCancelArmourEvent.AddListener(OnPlayerCancelArmourEvent);

        player.ssAnimEvent += PlayerSkillShiftAnimation;
        player.OnAPChanged.AddListener(OnPlayerAPChanged);
        for (int i = 0; i < skillSlots.Length; i++) {
            skillSlots[i].SkillBoardEvent += OnSkillBoardEvent;
        }

        BattleManager.Instance.OnTurnSwitched += OnTurnSwitched;
        BattleManager.Instance.CreatureBoardEvent += OnCreatureBoardEvent;
        BattleManager.Instance.MovePreviewEvent += MovePreviewEvent;
        AnimationManager.Instance.OnGameoverEvent.AddListener(Gameover);
    }

    private void OnPlayerMovedEvent(int steps)
    {
    }

    private void PlayerSkillShiftAnimation(float duration)
    {
        UpdateSkillSprites();
    }

    private void OnPlayerCancelArmourEvent()
    {
        physicalArmourFill.DOFade(0, 0.5f);
        magicalArmourFill.DOFade(0, 0.5f);
        physicalArmourText.text = "0";
        magicalArmourText.text = "0";
    }

    private void PlayerArmourPointsChangeAnimation(bool isPhysical, int remainedNumber)
    {
        if (isPhysical) {
            physicalArmourFill.DOFade(remainedNumber != 0 ? 1 : 0, 0.5f);
            physicalArmourText.text = remainedNumber.ToString();
        }
        else {
            magicalArmourFill.DOFade(remainedNumber != 0 ? 1 : 0, 0.5f);
            magicalArmourText.text = remainedNumber.ToString();
        }
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
        //hpFill.color = hpRatio > 0.2f ? new Color(0.5f, 1, 0.4f) : Color.red;
        playerHpText.text = ((int)(hpRatio * player.MaxHealthPoints)).ToString() + "/" + player.MaxHealthPoints.ToString();
    }

    private void OnPlayerAPChanged()
    {
        for (int i = 0; i < 8; i++) {
            aps[i].isOn = i + 1 <= player.ActionPoints;
        }
    }

    public void UpdateSkillSprites()
    {
        if (player.SkillHashes.Count < player.defaultSkillCount) return;
        for (int i = 0; i < player.defaultSkillCount; i++) {
            skills[i].sprite = player.SkillHashes[i].GetBaseSkill().sprite;
        }
    }

    private void MovePreviewEvent(int steps)
    {
        for (int i = 0; i < steps; i++) {
            if (i > skills.Length) return;
            skills[i].sprite = unknownSkill;
        }

        for (int i = steps; i < skills.Length; i++) {
            skills[i].sprite = player.SkillHashes[i- steps].GetBaseSkill().sprite;
        }
    }

    public void BattleStartAnim()
    {
        StartCoroutine(BattleStart());
        endTurnButton.gameObject.SetActive(true);
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
            text = "Your Turn";
        }
        else {
            text = "Enemy Turn";
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
        turnText.text = "Battle Start";
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
        var skill = player.SkillHashes[selectSkillID].GetBaseSkill() as BaseSkill;
        skillName.text = skill.skillName;
        skillCost.text = skill.actionPointsCost.ToString();
        //skillValue.text = skill.baseValue.ToString() + " + <color=#ffa500ff>" + player.Intelligence * skill.attributeCof * skill.baseValue +"</color>";
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
                creatureAP.text = cc.attribute.actionPerTurn.ToString();
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

            creatureBoard.position = Input.mousePosition +
                new Vector3((Input.mousePosition.x > Screen.width - 360 ? -360 : 60), (Input.mousePosition.y < 360 ? 360 : 0), 0);
        }
    }


}
