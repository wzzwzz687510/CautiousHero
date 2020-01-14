using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Wing.RPGSystem
{
    public class TitleUIController : MonoBehaviour
    {
        [Header("UI Elements")]
        public Button continueButton;     
        public Text saveName;
        public Image slotIcon;
        public Button saveButton;
        public Sprite[] iconSprites;
        public InputField nameInputField;
        public Button infoConfirmButton;
        public Text infoText;

        [Header("Pages")]
        public GameObject startPage;
        public GameObject createPage;
        public GameObject savePage;
        public GameObject nameInputPage;
        public GameObject infoPage;

        [Header("CreatePage")]
        public Text raceText;
        public Text raceDesText;
        public Text classText;
        public InfoBoard infoBoard;
        public GameObject talentCover;
        public GameObject classCover;
        public IconSlot[] talentSlots;
        public IconSlot[] skillSlots;

        [Header("SavePage")]
        public SaveSlot[] saveSlots;

        private int selectClassID;
        private int selectRaceID;
        private float infoBoardOffsetY => infoBoard.m_rectT.sizeDelta.y / 2 + 40;

        private void Start()
        {
            for (int i = 0; i < talentSlots.Length; i++) {
                talentSlots[i].RegisterDisplayAction(DisplayTalentInfoBoard);
                talentSlots[i].RegisterHideAction(HideInfoBoard);
            }
            for (int i = 0; i < skillSlots.Length; i++) {
                skillSlots[i].RegisterDisplayAction(DisplaySkillInfoBoard);
                skillSlots[i].RegisterHideAction(HideInfoBoard);
            }
            if (Database.Instance.SelectSlot != -1) {
                saveName.text = Database.Instance.ActivePlayerData.name;
                ResetUI();               
            }
            else {
                Button_DisplaySaveSlot();
            }
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(1)) {
                savePage.SetActive(false);
                createPage.SetActive(false);
                startPage.SetActive(true);
            }
        }

        private void HideInfoBoard()
        {
            infoBoard.transform.position = new Vector3(Screen.width + 260, 0, 0);
        }

        private void DisplaySkillInfoBoard(int slotID)
        {
            TRace tRace = Database.Instance.ActivePlayerData.unlockedRaces[selectRaceID].GetTRace();
            TClass tClass = Database.Instance.ActivePlayerData.unlockedClasses[selectClassID].GetTClass();
            infoBoard.UpdateToSkillBoard(slotID == 2 ? tRace.skill.Hash : tClass.skillSet[slotID].Hash);
            infoBoard.transform.position = skillSlots[slotID].transform.position + new Vector3(0, infoBoardOffsetY, 0);
        }

        private void DisplayTalentInfoBoard(int slotID)
        {
            TRace tRace = Database.Instance.ActivePlayerData.unlockedRaces[selectRaceID].GetTRace();
            TClass tClass = Database.Instance.ActivePlayerData.unlockedClasses[selectClassID].GetTClass();
            infoBoard.UpdateToRelicBoard(slotID == 0 ? tRace.relic.Hash : tClass.relic.Hash);
            infoBoard.transform.position = talentSlots[slotID].transform.position + new Vector3(0, infoBoardOffsetY, 0);
        }

        public void ResetUI()
        {
            startPage.SetActive(true);
            createPage.SetActive(false);
            nameInputPage.SetActive(false);
            savePage.SetActive(false);

            saveButton.gameObject.SetActive(true);
            continueButton.interactable = !Database.Instance.ActivePlayerData.isNewGame;
            selectRaceID = 0;
            selectClassID = 0;
            slotIcon.sprite = iconSprites[Database.Instance.SelectSlot];
        }

        public void Button_SelectSlot(int slotID)
        {
            
            Database.Instance.ChangeSelectSaveSlot(slotID);
            if (saveSlots[slotID].emptyText.enabled) {
                nameInputField.text = null;
                nameInputPage.SetActive(true);
            }
            else {
                Database.Instance.SavePlayerData();
                Database.Instance.LoadAll();
                continueButton.interactable = !Database.Instance.ActivePlayerData.isNewGame;
                savePage.SetActive(false);
                saveName.text = Database.Instance.GetPlayerData(slotID).name;
                slotIcon.sprite = iconSprites[slotID];
            }           
        }

        public void Button_NewSave()
        {
            if (nameInputField.text != null) {
                Database.Instance.CreateNewPlayer(nameInputField.text);
                saveName.text = nameInputField.text;                
                ResetUI();
            }          
        }

        public void Button_DisplaySaveSlot()
        {
            foreach (var slot in saveSlots) {
                slot.UpdateUI();
            }
            savePage.SetActive(true);
        }

        public void Button_StartNewGame()
        {
            startPage.SetActive(true);
            createPage.SetActive(false);
            gameObject.SetActive(false);
            Database.Instance.CreateNewGame(selectRaceID, selectClassID);
            WorldMapManager.Instance.StartNewGame();
        }

        public void Button_Continue()
        {
            gameObject.SetActive(false);
            WorldMapManager.Instance.ContinueGame();
        }

        public void Button_Summon()
        {
            if (continueButton.interactable) {
                infoConfirmButton.onClick.AddListener(() => {
                    Button_ConfirmNewGame();
                    infoConfirmButton.onClick.RemoveAllListeners();
                    infoPage.SetActive(false);
                });
                infoText.text = "Summon will create a new world and delete saved data.";
                infoPage.SetActive(true);
            }
            else Button_ConfirmNewGame();
        }

        public void Button_ConfirmNewGame()
        {
            TRace tRace = Database.Instance.ActivePlayerData.unlockedRaces[selectRaceID].GetTRace();
            TClass tClass = Database.Instance.ActivePlayerData.unlockedClasses[selectClassID].GetTClass();
            raceText.text = tRace.name;
            classText.text = tClass.name;
            raceDesText.text = tRace.description;

            talentSlots[0].icon.sprite = tRace.relic.sprite;
            talentSlots[1].icon.sprite = tClass.relic.sprite;           
            skillSlots[0].icon.sprite = tClass.skillSet[0].sprite;
            skillSlots[1].icon.sprite = tClass.skillSet[1].sprite;
            skillSlots[2].icon.sprite = tRace.skill.sprite;

            startPage.SetActive(false);
            createPage.SetActive(true);
        }

        public void Button_Setting()
        {

        }

        public void Button_Quit()
        {
            Application.Quit();
        }

        public void Button_Race(bool isPrev)
        {
            selectRaceID += isPrev ? -1 : 1;
            if (selectRaceID < 0) selectRaceID = TRace.Dict.Count - 1;
            else if (selectRaceID >= TRace.Dict.Count) selectRaceID = 0;

            if (selectRaceID < Database.Instance.ActivePlayerData.unlockedRaces.Count) {
                TRace tRace = Database.Instance.ActivePlayerData.unlockedRaces[selectRaceID].GetTRace();
                TClass tClass = Database.Instance.ActivePlayerData.unlockedClasses[selectClassID].GetTClass();
                raceText.text = tRace.name;
                raceDesText.text = tRace.description;
                talentSlots[0].icon.sprite = tRace.relic.sprite;
                talentSlots[1].icon.sprite = tClass.relic.sprite;
                talentCover.SetActive(false);
            }
            else {
                raceText.text = "LOCKED";
                raceDesText.text = "...";
                talentCover.SetActive(true);
            }
        }

        public void Button_Class(bool isPrev)
        {
            selectClassID += isPrev ? -1 : 1;
            if (selectClassID < 0) selectClassID = TClass.Dict.Count - 1;
            else if (selectClassID >= TClass.Dict.Count) selectClassID = 0;

            if (selectClassID < Database.Instance.ActivePlayerData.unlockedClasses.Count) {
                TRace tRace = Database.Instance.ActivePlayerData.unlockedRaces[selectRaceID].GetTRace();
                TClass tClass = Database.Instance.ActivePlayerData.unlockedClasses[selectClassID].GetTClass();
                classText.text = tClass.name;              
                skillSlots[0].icon.sprite = tClass.skillSet[0].sprite;
                skillSlots[1].icon.sprite = tClass.skillSet[1].sprite;
                skillSlots[2].icon.sprite = tRace.skill.sprite;

                classCover.SetActive(false);
            }
            else {
                classText.text = "LOCKED";
                classCover.SetActive(true);
            }
        }

    }
}