﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wing.TileUtils;

namespace Wing.RPGSystem
{
    public class AIManager : MonoBehaviour
    {
        public GameObject creaturePrefab;

        private GameObject creatureHolder;

        public PlayerController player;
        public CreatureController[] creatures;

        private HashSet<Location> playerAffectZone;

        public void DecisionMaking(int index)
        {

        }

        
    }
}
