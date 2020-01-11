using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Wing.RPGSystem;

public class AreaUIController : MonoBehaviour
{
    public PlayerController character;
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
    public GameObject[] aps;

    [Header("Turn Switch Visual")]
    public Image turnBG;
    public Text turnText;

    [Header("Battle End Visual")]
    public Image image_blackBG;
    public Image image_die;

    [Header("Skill Board")]
    public float waitDuration;
    public SkillSlot[] skillSlots;
    public InfoBoard infoBoard;
    private int selectSlot = -1;
    private bool isSkillBoardDisplayed;    

    [Header("Skill Learning Page")]
    public GameObject skillLearningPage;
    public SkillSlot[] skillLearningSlots; 

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

    public bool IsDisplayInfoAnim { get; private set; }
    private float timer;
    private bool startTimer;

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

        CharacterHPChangeAnimation(0, 1, 0);
        CharacterHPChangeAnimation(character.HealthPoints,character.MaxHealthPoints, 2);
        CharacterArmourPointsChangeAnimation(true, character.PhysicalArmourPoints);
        CharacterArmourPointsChangeAnimation(false, character.MagicalArmourPoints);
    }

    private void BindEvent()
    {
        character.OnMovedEvent += OnPlayerMovedEvent;
        character.HPChangeAnimation += CharacterHPChangeAnimation;
        character.ArmourPointsChangeAnimation += CharacterArmourPointsChangeAnimation;
        character.OnCancelArmourEvent.AddListener(OnCharacterCancelArmourEvent);

        character.ssAnimEvent += CharacterSkillShiftAnimation;
        character.OnAPChanged.AddListener(OnCharacterAPChanged);
        for (int i = 0; i < skillSlots.Length; i++) {
            skillSlots[i].RegisterDisplayAction(DisplaySkillInfoBoard);
            skillSlots[i].RegisterHideAction(HideSkillInfoBoard);
        }
        for (int i = 0; i < skillLearningSlots.Length; i++) {
            skillLearningSlots[i].RegisterDisplayAction(DisplaySkillInfoBoardAtLearningPage);
            skillLearningSlots[i].RegisterHideAction(HideSkillInfoBoard);
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

    private void CharacterSkillShiftAnimation(float duration)
    {
        UpdateSkillSprites();
    }

    private void OnCharacterCancelArmourEvent()
    {
        physicalArmourFill.DOFade(0, 0.5f);
        magicalArmourFill.DOFade(0, 0.5f);
        physicalArmourText.text = "0";
        magicalArmourText.text = "0";
    }

    private void CharacterArmourPointsChangeAnimation(bool isPhysical, int remainedNumber)
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

    private void CharacterHPChangeAnimation(int hp,int maxHP, float duration)
    {
        maxHP = maxHP == 0 ? 1 : maxHP;
        float hpRatio = 1.0f * hp / maxHP;
        if(hpRatio< playerHPBar.value) {
            hpFill.fillAmount = hpRatio;
        }
        else {
            DOTween.To(() => hpFill.fillAmount, ratio => hpFill.fillAmount = ratio, hpRatio, duration);
        }
        DOTween.To(() => playerHPBar.value, ratio => playerHPBar.value = ratio, hpRatio, duration);
        playerHpText.text = hp.ToString() + "/" + maxHP.ToString();
    }

    private void OnCharacterAPChanged()
    {
        for (int i = 0; i < aps.Length; i++) {
            aps[i].SetActive(i < character.ActionPoints);
        }
    }

    public void UpdateSkillSprites()
    {
        if (character.SkillHashes.Count < character.defaultSkillCount) return;
        for (int i = 0; i < character.defaultSkillCount; i++) {
            skills[i].sprite = character.SkillHashes[i].GetBaseSkill().sprite;
        }
    }

    public void SetSkillsUnknown()
    {
        for (int i = 0; i < character.defaultSkillCount; i++) {
            skills[i].sprite = unknownSkill;
        }

        endTurnButton.gameObject.SetActive(false);
        for (int i = 0; i < aps.Length; i++) {
            aps[i].SetActive(false);
        }
    }

    private void CastPreviewEvent(int skillID)
    {
        for (int i = 0; i < character.ActionPoints; i++) {
            aps[i].SetActive(i < character.ActionPoints - character.SkillHashes[skillID].GetBaseSkill().actionPointsCost);
        }
        skills[0].sprite = unknownSkill;
        for (int i = 1; i < skills.Length; i++) {
            skills[i].sprite = character.SkillHashes[i - (skillID >= i ? 1 : 0)].GetBaseSkill().sprite;
        }
    }

    private void MovePreviewEvent(int steps)
    {
        for (int i = 0; i < character.ActionPoints; i++) {
            aps[i].SetActive(i < character.ActionPoints - steps);
        }

        for (int i = 0; i < steps; i++) {
            if (i >= skills.Length) return;
            skills[i].sprite = unknownSkill;
        }

        for (int i = steps; i < skills.Length; i++) {
            skills[i].sprite = character.SkillHashes[i- steps].GetBaseSkill().sprite;
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
        yield return StartCoroutine(DisplayInfoAnim(isPlayerTurn ? "Your Turn" : "Enemy Turn", AudioManager.Instance.turnChangeClip));

        BattleManager.Instance.StartNewTurn(isPlayerTurn);
    }

    public IEnumerator BattleStart()
    {
        yield return StartCoroutine(DisplayInfoAnim("Battle Start"));
    }

    private IEnumerator DisplayInfoAnim(string text, AudioClip clip = null)
    {
        while (AnimationManager.Instance.IsPlaying) {
            yield return null;
        }
        IsDisplayInfoAnim = true;
        if (clip) AudioManager.Instance.PlaySEClip(clip);
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
        IsDisplayInfoAnim = false;
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

    public void DisplaySkillInfoBoard(int slotID)
    {
        if (character.SkillHashes.Count < character.defaultSkillCount) return;
        startTimer = true;
        selectSlot = slotID;
        if (timer > waitDuration) ShowSkillBoard();
    }

    public void DisplaySkillInfoBoardAtLearningPage(int slotID)
    {
        //float xOffset = Input.mousePosition.x > Screen.width - 370 ? -350 : 350;
        infoBoard.transform.position = skillLearningSlots[slotID].transform.position + new Vector3(0, -200, 0);
        infoBoard.UpdateToSkillBoard(AreaManager.Instance.RandomedSkillHashes[slotID]);
    }

    public void HideSkillInfoBoard()
    {
        startTimer = false;
        isSkillBoardDisplayed = false;
        selectSlot = -1;
        infoBoard.transform.position = new Vector3(Screen.width + 260, 0, 0);
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
        if (!skillLearningPage.activeSelf) {
            infoBoard.transform.position = skillSlots[selectSlot].transform.position + new Vector3(0, 160, 0);
            infoBoard.UpdateToSkillBoard(character.SkillHashes[selectSlot]);
        }
    }

    public void ShowCreatureBoard()
    {
        if (EntityManager.Instance.TryGetEntity(selectCreatureID, out Entity entity)) {
            var cc = (entity as CreatureController).Template;
            creatureSprite.sprite = cc.sprite;
            creatureName.text = cc.creatureName;
            if (entity.Intelligence <= character.Intelligence) {
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
            skillLearningSlots[i].icon.sprite = AreaManager.Instance.RandomedSkillHashes[i].GetBaseSkill().sprite;
        }
        skillLearningPage.SetActive(isActive);
    }
}
