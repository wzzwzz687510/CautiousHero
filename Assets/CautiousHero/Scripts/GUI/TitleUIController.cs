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

        public Text raceText;
        public Text raceDesText;
        public Image[] raceSprites;
        public Text classText;
        public Image[] classSprites;

        public GameObject talentCover;
        public GameObject classCover;
        public SaveSlot[] saveSlots;

        private int selectClassID;
        private int selectRaceID;

        private void Start()
        {
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
                infoText.text = "Summon will create a new world and delete the old world.";
                infoPage.SetActive(true);
            }
            else Button_ConfirmNewGame();
        }

        public void Button_ConfirmNewGame()
        {
            TRace tRace = Database.Instance.ActivePlayerData.unlockedRaces[selectRaceID].GetTRace();
            raceText.text = tRace.name;
            raceDesText.text = tRace.description;
            raceSprites[0].sprite = tRace.buffSet[0].sprite;
            raceSprites[1].sprite = tRace.buffSet[1].sprite;
            TClass tClass = Database.Instance.ActivePlayerData.unlockedClasses[selectClassID].GetTClass();
            classText.text = tClass.name;
            classSprites[0].sprite = tClass.skillSet[0].sprite;
            classSprites[1].sprite = tClass.skillSet[1].sprite;
            classSprites[2].sprite = tClass.skillSet[2].sprite;

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
                raceText.text = tRace.name;
                raceDesText.text = tRace.description; 
                raceSprites[0].sprite = tRace.buffSet[0].sprite;
                raceSprites[1].sprite = tRace.buffSet[1].sprite;
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
                TClass tClass = Database.Instance.ActivePlayerData.unlockedClasses[selectClassID].GetTClass();
                classText.text = tClass.name;              
                classSprites[0].sprite = tClass.skillSet[0].sprite;
                classSprites[1].sprite = tClass.skillSet[1].sprite;
                classSprites[2].sprite = tClass.skillSet[2].sprite;

                classCover.SetActive(false);
            }
            else {
                classText.text = "LOCKED";
                classCover.SetActive(true);
            }
        }

    }
}