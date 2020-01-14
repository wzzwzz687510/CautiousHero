using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Cinemachine;
using DG.Tweening;
using System.Linq;

namespace Wing.RPGSystem
{
    public class WorldMapUIController : MonoBehaviour
    {
        public static WorldMapUIController Instance { get; private set; }

        [Header("View")]
        public Image worldViewBG;
        public RawImage worldView;
        public PlayerController worldPlayer;
        public PlayerController areaPlayer;
        public float switchTime = 0.3f;

        [Header("UI Elements")]
        public Button infoConfirmButton;
        public Text infoText;
        public Text playerName;
        public Text hpText;
        public Text coinText;
        public Text expText;

        [Header("Skill Book Elements")]
        //public GameObject infoPrefab;
        //public Transform contentL;
        //public Transform contentR;
        public InfoBoard infoBoard;        
        public IconSlot[] skillElements;

        [Header("Pages")]
        public GameObject titlePage;
        public GameObject loadingPage;
        public GameObject infoPage;
        public GameObject endPage;
        public GameObject bookPage;

        public PlayerData ActivePlayerData => Database.Instance.ActivePlayerData;
        public WorldData ActiveWorldData => WorldData.ActiveData;

        private List<BaseSkill> skillDeck;

        private void Awake()
        {
            if (!Instance)
                Instance = this;

            Database.Instance.WorldDataChangedEvent.AddListener(UpdateUI);
            AreaManager.Instance.character.HPChangeAnimation+=UpdateHP;
            for (int i = 0; i < skillElements.Length; i++) {
                skillElements[i].slotID = i;
                skillElements[i].RegisterDisplayAction(DisplaySkillInfoBoard);
                skillElements[i].RegisterHideAction(HideSkillInfoBoard);
            }
        }

        public void UpdateUI()
        {
            playerName.text = ActivePlayerData.name;
            hpText.text = ActiveWorldData.HealthPoints + "/" + ActiveWorldData.attribute.maxHealth;
            coinText.text = ActiveWorldData.coins.ToString();
            expText.text = ActiveWorldData.exp.ToString();

            skillDeck = (from skillHash in WorldData.ActiveData.learnedSkills select skillHash.GetBaseSkill()).ToList();
            skillDeck.Sort(BaseSkill.CompareByName);
            for (int i = 0; i < skillElements.Length; i++) {               
                if(i < skillDeck.Count) {
                    skillElements[i].gameObject.SetActive(true);
                    skillElements[i].icon.sprite = skillDeck[i].sprite;
                }
                else {
                    skillElements[i].gameObject.SetActive(false);
                }
            }
        }

        public void UpdateHP(int hp, int maxHP, float duration)
        {
            hpText.DOText(hp + "/" + maxHP, duration);
        }

        public void SetLoadingPage(bool isShow)
        {
            loadingPage.SetActive(isShow);
        }

        public void SwitchToWorldView()
        {
            bookPage.SetActive(false);
            WorldMapManager.Instance.SetWorldView(true);
            worldView.gameObject.SetActive(true);
            worldViewBG.gameObject.SetActive(true);
            worldView.DOFade(1, switchTime);
            worldViewBG.DOFade(1, switchTime).OnComplete(() => {
                //SetLoadingPage(false);                
                AreaManager.Instance.SetMoveCheck(true);
            });
        }

        public void SwitchToAreaView()
        {
            bookPage.SetActive(false);
            WorldMapManager.Instance.SetWorldView(false);
            worldView.DOFade(0, switchTime);
            worldViewBG.DOFade(0, switchTime).OnComplete(() => {
                //SetLoadingPage(false);
                worldView.gameObject.SetActive(false);
                worldViewBG.gameObject.SetActive(false);
                AreaManager.Instance.SetMoveCheck(true);
            });
        }

        public void DisplayEndPage()
        {
            endPage.SetActive(true);
        }

        public void DisplaySkillInfoBoard(int slotID)
        {
            float xOffset = Input.mousePosition.x > Screen.width - 330 ? -320 : 320;
            infoBoard.transform.position = skillElements[slotID].transform.position + new Vector3(xOffset, 0, 0);
            infoBoard.UpdateToSkillBoard(skillDeck[slotID].Hash);
        }

        public void HideSkillInfoBoard()
        {
            infoBoard.transform.position = new Vector3(Screen.width + 260, 0, 0);
        }

        public void Button_CompleteAWorld()
        {
            endPage.SetActive(false);
            titlePage.SetActive(true);
            WorldMapManager.Instance.CompleteWorld();
        }

        public void Button_SkillBook()
        {
            if (bookPage.activeSelf) {
                bookPage.SetActive(false);
            }
            else {
                bookPage.SetActive(true);
            }
        }

        public void Button_WorldMap()
        {
            if (bookPage.activeSelf) {
                bookPage.SetActive(false);
                if(!WorldMapManager.Instance.IsWorldView) AreaManager.Instance.CompleteExploration();
            }
            else {
                if (WorldMapManager.Instance.IsWorldView) {
                    WorldMapManager.Instance.EnterArea(AreaManager.Instance.CurrentAreaLoc);
                }
                else {
                    AreaManager.Instance.CompleteExploration();
                }
            }
        }

        public void Button_System()
        {
            infoConfirmButton.onClick.AddListener(() => {
                SwitchToWorldView();
                Database.Instance.SaveAll();
                infoConfirmButton.onClick.RemoveAllListeners();
                infoPage.SetActive(false);
                
                AudioManager.Instance.PlayTitleClip();
                titlePage.SetActive(true);
            });
            infoText.text = "Progress is automatically saved.";
            infoPage.SetActive(true);
            
        }
    }
}

