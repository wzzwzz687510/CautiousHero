using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wing.RPGSystem
{
    public class WorldMapUIController : MonoBehaviour
    {
        public static WorldMapUIController Instance { get; private set; }

        private void Awake()
        {
            if (!Instance)
                Instance = this;
        }

        public void ShowAreaInteractionBoard()
        {

        }
    }
}

