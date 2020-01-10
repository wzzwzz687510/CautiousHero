using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Cinemachine;
using DG.Tweening;

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

        [Header("Pages")]
        public GameObject titlePage;
        public GameObject loadingPage;
        public GameObject infoPage;
        public GameObject endPage;

        public PlayerData ActivePlayerData => Database.Instance.ActivePlayerData;
        public WorldData ActiveWorldData => Database.Instance.ActiveWorldData;

        private void Awake()
        {
            if (!Instance)
                Instance = this;

            Database.Instance.WorldDataChangedEvent.AddListener(UpdateUI);
        }

        public void UpdateUI()
        {
            playerName.text = ActivePlayerData.name;
            hpText.text = ActiveWorldData.HealthPoints + "/" + ActiveWorldData.attribute.maxHealth;
            coinText.text = ActiveWorldData.coins.ToString();
            expText.text = ActiveWorldData.exp.ToString();
        }

        public void SetLoadingPage(bool isShow)
        {
            loadingPage.SetActive(isShow);
        }

        public void SwitchToWorldView()
        {
            //SetLoadingPage(true);
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
            //SetLoadingPage(true);
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

        public void Button_CompleteAWorld()
        {
            endPage.SetActive(false);
            AudioManager.Instance.PlayTitleClip();
            titlePage.SetActive(true);
        }

        public void Button_System()
        {
            infoConfirmButton.onClick.AddListener(() => {
                Database.Instance.SaveAll();
                infoConfirmButton.onClick.RemoveAllListeners();
                infoPage.SetActive(false);
                //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                AudioManager.Instance.PlayTitleClip();
                titlePage.SetActive(true);
            });
            infoText.text = "Progress is automatically saved.";
            infoPage.SetActive(true);
            
        }
    }
}

