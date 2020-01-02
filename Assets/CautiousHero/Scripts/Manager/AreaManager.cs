using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wing.RPGSystem
{

    public class AreaManager : MonoBehaviour
    {
        public static AreaManager Instance { get; private set; }

        public AreaConfig Config { get; private set; }

        private void Awake()
        {
            if (!Instance)
                Instance = this;
        }

        public void EnterArea(Location loc)
        {
            if (!Database.Instance.TryGetAreaInfo(loc, out AreaInfo info)) Debug.LogError("area not exist");




        }
    }
}