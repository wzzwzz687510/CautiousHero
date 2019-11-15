using System.Collections;
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

        private void CalculatePlayerAffectZone()
        {
            //for (int i = 0; i < player.ActionPoints; i++) {
            //    foreach (var skill in player.skills) {
            //        if (skill.actionPointsCost <= player.ActionPoints - i) {
            //            foreach (var item in collection) {

            //            }
            //            playerAffectZone.Add
            //        }
            //    }
            //}
        }
    }
}
