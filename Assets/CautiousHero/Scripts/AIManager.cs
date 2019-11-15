using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wing.RPGSystem
{
    public class AIManager : MonoBehaviour
    {
        public GameObject creaturePrefab;

        private GameObject creatureHolder;

        public PlayerController player;
        public CreatureController[] creatures;

    }
}
