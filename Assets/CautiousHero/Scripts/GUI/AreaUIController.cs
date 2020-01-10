using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Wing.RPGSystem;

public class AreaUIController : MonoBehaviour
{
    public PlayerController player;
    public GameObject battleUI;

    [Header("Buttons")]
    public Button endStageButton;
    public Button endTurnButton;

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
    public Text skillName;
    public Text skillCost;
    public Text skillValue;
    public Text skillType;
    public Text skillElement;
    public Text skillCastType;
    public SkillSlot[] skillSlots;
    public Color[] colors;
    public Image skillNameBg;
    private int selectSlot = -1;
    private int selectSkillHash;
    private bool isSkillBoardDisplayed;    

    [Header("Skill Learning Page")]
    public Image[] skillLearningImages;
    public GameObject skillLearningPage;
    public SkillSlot[] skillLearningSlots;

    private float timer;
    private bool startTimer;

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
        if (startTimer && !isSkillBoardDisplayed && !isCreatureBoardDisplayed) {
            timer += Time.deltaTime;
            if (timer > waitDuration) {
                if (selectSlot != -1) {
                    ShowSkillBoard();
                    isSkillBoardDisplayed = true;
                }
                else if (selectCreatureID != -1) {
                    ShowCreatureBoard();
                    isCreatureBoardDisplayed = true;
                }
            }
        }
        else if (!startTimer && timer > 0) {
            timer -= Time.deltaTime;
        }
    }

    private void Start()
    {
        BindEvent();
    }

    public void EnterAreaAnim()
    {
        SetSkillsUnknown();
        endStageButton.gameObject.SetActive(false);

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
            skillSlots[i].CheckSlotState.AddListener(OnSkillBoardEvent);
        }
        for (int i = 0; i < skillLearningSlots.Length; i++) {
            skillLearningSlots[i].CheckSlotState.AddListener(OnLearnSkillPageEvent);
        }

        BattleManager.Instance.OnTurnSwitched += OnTurnSwitched;
        BattleManager.Instance.CreatureBoardEvent += OnCreatureBoardEvent;
        BattleManager.Instance.MovePreviewEvent += MovePreviewEvent;
        BattleManager.Instance.CastPreviewEvent += CastPreviewEvent;
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
            aps[i].isOn = i < player.ActionPoints;
        }
    }

    public void UpdateSkillSprites()
    {
        if (player.SkillHashes.Count < player.defaultSkillCount) return;
        for (int i = 0; i < player.defaultSkillCount; i++) {
            skills[i].sprite = player.SkillHashes[i].GetBaseSkill().sprite;
        }
    }

    public void SetSkillsUnknown()
    {
        for (int i = 0; i < player.defaultSkillCount; i++) {
            skills[i].sprite = unknownSkill;
        }

        endTurnButton.gameObject.SetActive(false);
    }

    private void CastPreviewEvent(int skillID)
    {
        for (int i = 0; i < player.ActionPoints; i++) {
            aps[i].isOn = i < player.ActionPoints - player.SkillHashes[skillID].GetBaseSkill().actionPointsCost;
        }
        skills[0].sprite = unknownSkill;
        for (int i = 1; i < skills.Length; i++) {
            skills[i].sprite = player.SkillHashes[i - (skillID >= i ? 1 : 0)].GetBaseSkill().sprite;
        }
    }

    private void MovePreviewEvent(int steps)
    {
        for (int i = 0; i < player.ActionPoints; i++) {
            aps[i].isOn = i < player.ActionPoints - steps;
        }

        for (int i = 0; i < steps; i++) {
            if (i >= skills.Length) return;
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
        yield return StartCoroutine(DisplayInfoAnim(isPlayerTurn ? "Your Turn" : "Enemy Turn"));

        BattleManager.Instance.StartNewTurn(isPlayerTurn);
    }

    public IEnumerator BattleStart()
    {
        yield return StartCoroutine(DisplayInfoAnim("Battle Start"));
    }

    private IEnumerator DisplayInfoAnim(string text)
    {
        while (AnimationManager.Instance.IsPlaying) {
            yield return null;
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
    }

    public void DisplayInfo(string text)
    {
        StartCoroutine(DisplayInfoAnim(text));
    }

    public void Button_EndTurn()
    {
        BattleManager.Instance.EndTurn();
    }

    public void Button_EndStage()
    {
        WorldMapManager.Instance.EnterNextStage();
    }

    public void Button_CastSkill(int skillID)
    {
        BattleManager.Instance.CastSkill(skillID);
    }

    public void Button_WorldMap()
    {
        if (WorldMapManager.Instance.IsWorldView) {
            WorldMapManager.Instance.EnterArea(AreaManager.Instance.CurrentAreaLoc);
        }
        else {
            AreaManager.Instance.CompleteExploration();
        }       
    }

    public void Gameover()
    {
        DOTween.ToAlpha(() => image_blackBG.color, color => image_blackBG.color = color, 0.7f, 0.5f);
        DOTween.ToAlpha(() => image_die.color, color => image_die.color = color, 1, 2);
        image_blackBG.raycastTarget = true;
    }

    public void OnSkillBoardEvent()
    {
        if (player.SkillHashes.Count < player.defaultSkillCount) return;
        startTimer = false;
        for (int i = 0; i < skillSlots.Length; i++) {
            if (skillSlots[i].IsActive) {
                startTimer = true;
                selectSlot = i;
                selectSkillHash = player.SkillHashes[i];
                if(timer > waitDuration) ShowSkillBoard();
            }
        }
        if (startTimer == false) {
            skillBoard.position = new Vector3(-300, 0, 0);
            isSkillBoardDisplayed = false;
            selectSlot = -1;
        }
    }

    public void OnLearnSkillPageEvent()
    {
        startTimer = false;
        for (int i = 0; i < skillLearningSlots.Length; i++) {
            if (skillLearningSlots[i].IsActive) {
                startTimer = true;
                selectSlot = i;
                selectSkillHash = AreaManager.Instance.RandomedSkillHashes[i];
                if (timer > waitDuration) ShowSkillBoard();
            }
        }

        if (startTimer == false) {
            skillBoard.position = new Vector3(-300, 0, 0);
            isSkillBoardDisplayed = false;
            selectSlot = -1;
        }
    }

    public void OnCreatureBoardEvent(int hash, bool isExit)
    {
        if (selectCreatureID == hash && !isExit)
            return;
        selectCreatureID = hash;
        startTimer = !isExit;
        if (!isExit && timer > waitDuration) {
            ShowCreatureBoard();
        }
        else if (isExit) {            
            isCreatureBoardDisplayed = false;
            creatureBoard.position = new Vector3(-300, 0, 0);
        }
    }

    public void ShowSkillBoard()
    {
        var skill = selectSkillHash.GetBaseSkill() as BaseSkill;
        skillName.text = skill.skillName;
        skillCost.text = skill.actionPointsCost.ToString();
        //skillValue.text = skill.baseValue.ToString() + " + <color=#ffa500ff>" + player.Intelligence * skill.attributeCof * skill.baseValue +"</color>";
        switch (skill.damageType) {
            case DamageType.Physical:
                skillNameBg.color = colors[0];
                skillType.text = "Physical";
                break;
            case DamageType.Magical:
                skillType.text = "Magical";
                break;
            case DamageType.Pure:
                skillType.text = "Pure";
                skillNameBg.color = colors[1];
                break;
            default:
                break;
        }
        switch (skill.skillElement) {
            case SkillElement.None:
                skillElement.text = "None";
                break;
            case SkillElement.Fire:
                skillElement.text = "Fire";
                skillNameBg.color = colors[2];
                break;
            case SkillElement.Water:
                skillElement.text = "Water";
                skillNameBg.color = colors[3];
                break;
            case SkillElement.Earth:
                skillElement.text = "Earth";
                skillNameBg.color = colors[4];
                break;
            case SkillElement.Air:
                skillElement.text = "Air";
                skillNameBg.color = colors[5];
                break;
            case SkillElement.Light:
                skillElement.text = "Light";
                skillNameBg.color = colors[6];
                break;
            case SkillElement.Dark:
                skillElement.text = "Dark";
                skillNameBg.color = colors[7];
                break;
            default:
                break;
        }
        switch (skill.castType) {
            case CastType.Instant:
                skillCastType.text = "Instant";
                break;
            case CastType.Trajectory:
                skillCastType.text = "Trajectory";
                break;
            default:
                break;
        }
        skillBoard.position = skillSlots[selectSlot].transform.position + new Vector3(0, 170, 0);
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
                    creatureResistance.text = "None";
                else
                switch (cc.skills[0].skillElement) {
                    case SkillElement.None:
                        creatureResistance.text = "None";
                        break;
                    case SkillElement.Fire:
                        creatureResistance.text = "Fire";
                        break;
                    case SkillElement.Water:
                        creatureResistance.text = "Water";
                        break;
                    case SkillElement.Earth:
                        creatureResistance.text = "Earth";
                        break;
                    case SkillElement.Air:
                        creatureResistance.text = "Air";
                        break;
                    case SkillElement.Light:
                        creatureResistance.text = "Light";
                        break;
                    case SkillElement.Dark:
                        creatureResistance.text = "Dark";
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

            creatureBoard.position = new Vector3(Screen.width - 310, Screen.height - 130, 0);
        }
    }

    public void ShowSkillLearningPage(bool isActive)
    {
        for (int i = 0; i < skillLearningSlots.Length; i++) {
            skillLearningImages[i].sprite = AreaManager.Instance.RandomedSkillHashes[i].GetBaseSkill().sprite;
        }
        skillLearningPage.SetActive(isActive);
    }
}
