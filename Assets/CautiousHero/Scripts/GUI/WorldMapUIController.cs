using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wing.RPGSystem
{
    public class WorldMapUIController : MonoBehaviour
    {
        public static WorldMapUIController Instance { get; private set; }

        public GameObject loadingPage;

        private void Awake()
        {
            if (!Instance)
                Instance = this;
        }

        public void SetLoadingPage(bool isShow)
        {
            loadingPage.SetActive(isShow);
        }

        public void ShowAreaInteractionBoard()
        {

        }
    }
}

