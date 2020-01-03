using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Wing.RPGSystem
{
    public class TitleUIController : MonoBehaviour
    {
        public Button continueButton;
        public GameObject startPage;
        public GameObject createPage;
        public Canvas worldMap;

        public Text raceText;
        public Text classText;
        public Text raceDesText;
        public GameObject talentCover;
        public GameObject classCover;

        private void Start()
        {
            continueButton.interactable = Database.Instance.saveName != "";
            
        }

        public void Button_Continue()
        {
            worldMap.enabled = true;
        }

        public void Button_Summon()
        {
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
            // Test
            bool locked = raceText.text == "LOCKED";
            raceText.text = locked ? "Human": "LOCKED";
            raceDesText.text = locked ? "long ago took to the seas and rivers in longboats, first to" +
                " pillage and terrorize, then to settle. Yet there was an energy," +
                " a love of adventure, that sang from every page. Long into the" +
                " night Liriel read, lighting candle after precious candle." : "...";
            talentCover.SetActive(!locked);
        }

        public void Button_Class(bool isPrev)
        {
            // Test
            bool locked = classText.text == "LOCKED";
            classText.text = locked ? "Saber" : "LOCKED";
            classCover.SetActive(!locked);
        }
    }
}