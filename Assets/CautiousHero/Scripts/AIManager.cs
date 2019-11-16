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

        public void PrepareDecisionMaking()
        {
            playerAffectZone = GridManager.Instance.CalculateEntityEffectZone(player);
        }

        public void DecisionMaking(int index)
        {
            var agent = creatures[index];

            if()
        }

        private bool HasControlSkill(int index)
        {
            bool hasCS = false;
            foreach (var skill in creatures[index].skills) {
                if(skill.labels.Contains(Label.HardControll))
            }
        }

        
    }
}
