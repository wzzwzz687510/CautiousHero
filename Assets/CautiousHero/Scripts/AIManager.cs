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

        private HashSet<Location> playerEffectZone;

        public void PrepareDecisionMaking()
        {
            playerEffectZone = GridManager.Instance.CalculateEntityEffectZone(player);
        }

        public void DecisionMaking(int index)
        {
            // if player can effect agent
            if (playerEffectZone.Contains(creatures[index].Loc)) {
                AvoidPlayerEffect(index);
            }
            else {
                ApplyStrategy(index);
            }
        }

        private void ApplyStrategy(int index)
        {
            HealAlley(index);

        }

        private void AttackPlayer(int index)
        {

        }

        private void HealAlley(int index)
        {
            if (!HasGivenLabelSkill(index, Label.Healing))
                return;

            int lowestHP = creatures[0].HealthPoints;
            int lowestID = -1;
            for (int i = 0; i < creatures.Length; i++) {
                if (creatures[i].HealthPoints < creatures[i].MaxHealthPoints / 2) {
                    if (lowestID == -1 || creatures[i].HealthPoints < lowestHP) {
                        lowestID = i;
                        lowestHP = creatures[i].HealthPoints;
                    }
                }
            }

            TryCastGivenLabelSkill(index, Label.Healing, creatures[lowestID], playerEffectZone);
        }

        private void AvoidPlayerEffect(int index)
        {
            if (!TryCastGivenLabelSkill(index,Label.HardControl,player)) {
                if (!TryCastGivenLabelSkill(index, Label.SoftControl, player)) {
                    if (!TryCastGivenLabelSkill(index, Label.Obstacle, player)) {
                        TileController tc;
                        if (GridManager.Instance.TryGetSafeTile(creatures[index], player, out tc)) {
                            creatures[index].MoveToTile(tc);
                        }
                        else {
                            TryCastGivenLabelSkill(index, Label.DefenseBuff, creatures[index]);
                        }
                    }
                }
            }

            ApplyStrategy(index);
        }

        private bool SkillCheck(InstanceSkill skill,Label label)
        {
            return skill.Castable && skill.TSkill.labels.Contains(label);
        }

        private bool HasGivenLabelSkill(int index, Label label)
        {
            var skills = creatures[index].ActiveSkills;
            bool hasHealingSkill = false;
            for (int i = 0; i < skills.Length; i++) {
                if (SkillCheck(skills[i], label))
                    hasHealingSkill = true;
            }

            return hasHealingSkill;
        }

        private bool TryCastGivenLabelSkill(int index, Label label,Entity target)
        {
            var skills = creatures[index].ActiveSkills;
            for (int i = 0; i < skills.Length; i++) {
                if (SkillCheck(skills[i],label)) {
                    TileController tc;
                    if (GridManager.Instance.CalculateCastSkillTile(creatures[index], i, target.Loc, out tc)) {
                        creatures[index].MoveToTile(tc);
                        //creatures[index].CastSkill(i, ???);
                        return true;
                    }
                }
            }
            return false;
        }

        private bool TryCastGivenLabelSkill(int index, Label label, Entity target, HashSet<Location> avoidZone)
        {
            var skills = creatures[index].ActiveSkills;
            for (int i = 0; i < skills.Length; i++) {
                if (SkillCheck(skills[i], label)) {
                    TileController tc;
                    if (GridManager.Instance.CalculateCastSkillTile(creatures[index], i, target.Loc, out tc) &&
                        !avoidZone.Contains(tc.Loc)) {
                        creatures[index].MoveToTile(tc);
                        //creatures[index].CastSkill(i, ???);
                        return true;
                    }
                }
            }
            return false;
        }


    }
}
