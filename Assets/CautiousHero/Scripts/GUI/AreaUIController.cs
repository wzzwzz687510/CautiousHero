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
    public GameObject arrowAnim;

    [Header("AP Visual")]
    public Animation[] aps;

    [Header("Turn Switch Visual")]
    public Image turnBG;
    public Text turnText;

    [Header("Battle End Visual")]
    public Image image_blackBG;
    public Image image_die;

    [Header("Deck Visual")]
    public GameObject deckPage;
    public IconSlot[] deckSkillSlots;
    private bool isDeckInfo;

    [Header("Buff Visual")]
    public IconSlot buffSlotPrefab;
    public Transform buffSlotHolder;
    public List<IconSlot> buffSlots;
    //public IconSlot[] buffSlots;

    [Header("Skill Board")]
    public float waitDuration;
    public IconSlot[] skillSlots;
    public InfoBoard infoBoard;
    private int selectSlot = -1;
    private bool isInfoBoardDisplayed;
    private bool isSkillInfo;

    [Header("Skill Learning Page")]
    public GameObject skillLearningPage;
    public IconSlot[] skillLearningSlots; 

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
        if (startTimer && !isInfoBoardDisplayed && !isCreatureBoardDisplayed) {
            timer += Time.deltaTime;
            if (timer > waitDuration) {
                if (selectSlot != -1) {
                    if (isSkillInfo) DisplaySkillBoard();
                    else DisplayBuffBoard();
                    isInfoBoardDisplayed = true;
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
        buffSlots = new List<IconSlot>();
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

    public void BindBuffManager()
    {
        character.BuffManager.OnBuffChangedEvent += OnCharacterBuffChanged;
    }

    private void BindEvent()
    {        
        character.OnStartMovedEvent += OnCharacterStartMovedEvent;
        character.HPChangeAnimation += CharacterHPChangeAnimation;
        character.ArmourPointsChangeAnimation += CharacterArmourPointsChangeAnimation;
        character.OnCancelArmourEvent.AddListener(OnCharacterCancelArmourEvent);

        character.ssAnimEvent += CharacterSkillShiftAnimation;
        character.OnAPChanged.AddListener(OnCharacterAPChanged);
        for (int i = 0; i < deckSkillSlots.Length; i++) {
            deckSkillSlots[i].slotID = i;
            deckSkillSlots[i].RegisterDisplayAction(OnShowDeckSkillInfoEvent);
            deckSkillSlots[i].RegisterHideAction(OnHideInfoBoardEvent);
            deckSkillSlots[i].gameObject.SetActive(false);
        }
        for (int i = 0; i < skillSlots.Length; i++) {
            skillSlots[i].RegisterDisplayAction(OnShowSkillInfoBoardEvent);
            skillSlots[i].RegisterHideAction(OnHideInfoBoardEvent);
        }
        for (int i = 0; i < skillLearningSlots.Length; i++) {
            skillLearningSlots[i].RegisterDisplayAction(OnShowLearnSkillInfoBoardEvent);
            skillLearningSlots[i].RegisterHideAction(OnHideInfoBoardEvent);
        }

        BattleManager.Instance.OnTurnSwitched += OnTurnSwitched;
        BattleManager.Instance.CreatureBoardEvent += OnCreatureBoardEvent;
        BattleManager.Instance.MovePreviewEvent += MovePreviewEvent;
        BattleManager.Instance.CastPreviewEvent += CastPreviewEvent;
        AnimationManager.Instance.OnGameoverEvent.AddListener(Gameover);
    }

    private void OnShowDeckSkillInfoEvent(int slotID)
    {
        float xOffset = Input.mousePosition.x > Screen.width - 330 ? -320 : 320;
        infoBoard.transform.position = deckSkillSlots[slotID].transform.position + new Vector3(xOffset, 0, 0);
        infoBoard.UpdateToSkillBoard(isDeckInfo?character.SkillDeck[slotID]: character.SkillDiscardPile[slotID]);
    }

    private void OnShowSkillInfoBoardEvent(int slotID)
    {
        if (character.SkillHashes.Count < character.defaultSkillCount || !endTurnButton.gameObject.activeSelf) return;
        startTimer = true;
        isSkillInfo = true;
        selectSlot = slotID;
        if (timer > waitDuration) DisplaySkillBoard();
    }

    private void OnShowBuffInfoBoardEvent(int slotID)
    {
        startTimer = true;
        isSkillInfo = false;
        selectSlot = slotID;
        if (timer > waitDuration) DisplayBuffBoard();
    }

    private void OnShowLearnSkillInfoBoardEvent(int slotID)
    {
        //float xOffset = Input.mousePosition.x > Screen.width - 370 ? -350 : 350;
        infoBoard.transform.position = skillLearningSlots[slotID].transform.position + new Vector3(0, -200, 0);
        infoBoard.UpdateToSkillBoard(AreaManager.Instance.RandomedSkillHashes[slotID]);
    }

    private void OnHideInfoBoardEvent()
    {
        startTimer = false;
        isInfoBoardDisplayed = false;
        selectSlot = -1;
        infoBoard.transform.position = new Vector3(Screen.width + 260, 0, 0);
    }

    private void DisplayBuffBoard()
    {
        if (!skillLearningPage.activeSelf) {
            infoBoard.transform.position = buffSlots[selectSlot].transform.position + new Vector3(272, 0, 0);
            infoBoard.UpdateToBuffBoard(character.BuffManager.buffHashes[selectSlot]);
        }
    }

    private void DisplaySkillBoard()
    {
        if (!skillLearningPage.activeSelf) {
            infoBoard.transform.position = skillSlots[selectSlot].transform.position + new Vector3(0, 160, 0);
            infoBoard.UpdateToSkillBoard(character.SkillHashes[selectSlot]);
        }
    }

    private void OnCharacterStartMovedEvent(int steps)
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

    private void OnCharacterBuffChanged(int buffID,bool isAdding)
    {
        if (isAdding) {
            IconSlot buffSlot = Instantiate(buffSlotPrefab, buffSlotHolder);
            buffSlot.slotID = buffID;
            buffSlot.icon.sprite = character.BuffManager.buffHashes[buffID].GetBaseBuff().sprite;
            buffSlot.RegisterDisplayAction(OnShowBuffInfoBoardEvent);
            buffSlot.RegisterHideAction(OnHideInfoBoardEvent);
            buffSlots.Add(buffSlot);
        }
        else {
            for (int i = 0; i < buffSlots.Count; i++) {
                buffSlots[i].UpdateSlotID(buffID);
            }
            Destroy(buffSlotHolder.GetChild(buffID).gameObject);
        }
    }

    private void OnCharacterAPChanged()
    {
        for (int i = 0; i < aps.Length; i++) {
            aps[i].gameObject.SetActive(i < character.ActionPoints);
            aps[i].Play("apStatic");
        }
    }

    public void UpdateSkillSprites()
    {
        arrowAnim.SetActive(false);
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
            aps[i].gameObject.SetActive(false);
        }

        for (int i = 0; i < buffSlotHolder.childCount; i++) {
            Destroy(buffSlotHolder.GetChild(i).gameObject);
        }
        buffSlots.Clear();
    }

    private void CastPreviewEvent(int skillID)
    {
        arrowAnim.SetActive(true);
        int cost = character.SkillHashes[skillID].GetBaseSkill().actionPointsCost;
        for (int i = 0; i < character.ActionPoints; i++) {
            aps[i].Play("apStatic");
            if (i >= character.ActionPoints - cost) aps[i].Play("apBlink");
        }
        skills[0].sprite = unknownSkill;
        for (int i = 1; i < skills.Length; i++) {
            skills[i].sprite = character.SkillHashes[i - (skillID >= i ? 1 : 0)].GetBaseSkill().sprite;
        }
    }

    private void MovePreviewEvent(int steps)
    {
        arrowAnim.SetActive(steps != 0);
        for (int i = 0; i < character.ActionPoints; i++) {
            aps[i].Play("apStatic");
            if (i >= character.ActionPoints - steps) aps[i].Play("apBlink");
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

    public void Button_Deck()
    {
        if (!BattleManager.Instance.IsInBattle) return;
        deckPage.SetActive(true);
        isDeckInfo = true;
        for (int i = 0; i < deckSkillSlots.Length; i++) {
            if (i < character.SkillDeck.Count) {
                deckSkillSlots[i].gameObject.SetActive(true);
                deckSkillSlots[i].icon.sprite = character.SkillDeck[i].GetBaseSkill().sprite;
            }
            else {
                deckSkillSlots[i].gameObject.SetActive(false);
            }
        }
    }

    public void Button_DiscardPile()
    {
        if (!BattleManager.Instance.IsInBattle) return;
        deckPage.SetActive(true);
        isDeckInfo = false;
        for (int i = 0; i < deckSkillSlots.Length; i++) {
            if (i < character.SkillDiscardPile.Count) {
                deckSkillSlots[i].gameObject.SetActive(true);
                deckSkillSlots[i].icon.sprite = character.SkillDiscardPile[i].GetBaseSkill().sprite;
            }
            else {
                deckSkillSlots[i].gameObject.SetActive(false);
            }
        }
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

    public void Gameover()
    {
        DOTween.ToAlpha(() => image_blackBG.color, color => image_blackBG.color = color, 0.7f, 0.5f);
        DOTween.ToAlpha(() => image_die.color, color => image_die.color = color, 1, 2);
        image_blackBG.raycastTarget = true;
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
                    case ElementType.None:
                        creatureResistance.text = "None";
                        break;
                    case ElementType.Fire:
                        creatureResistance.text = "Fire";
                        break;
                    case ElementType.Water:
                        creatureResistance.text = "Water";
                        break;
                    case ElementType.Earth:
                        creatureResistance.text = "Earth";
                        break;
                    case ElementType.Air:
                        creatureResistance.text = "Air";
                        break;
                    case ElementType.Light:
                        creatureResistance.text = "Light";
                        break;
                    case ElementType.Dark:
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
