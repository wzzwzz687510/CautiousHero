﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using SpriteGlow;

namespace Wing.RPGSystem
{
    public class PlayerController : Entity
    {
        public float ssAnimDuration = 0.2f;
        public int defaultSkillCount = 5;

        public List<int> SkillDeck { get; private set; }
        public List<int> SkillDiscardPile { get; private set; }

        private Location lastLoc;
        private List<int> lastSkills;
        private List<int> lastSkillDeck;
        private List<int> lastSkillDiscardPile;
        private int lastActionPoints;

        public delegate void SkillShiftAnimation(float duration);
        public SkillShiftAnimation ssAnimEvent;

        public void InitPlayer(EntityAttribute attributes)
        {
            
            m_attribute = attributes;           
            EntityName = "Player";
            Hash = EntityManager.Instance.AddEntity(this);            
            BuffManager = new BuffManager(Hash);

            HealthPoints = MaxHealthPoints;
            SkillDeck = new List<int>(Database.Instance.ActiveData.learnedSkills);
            SkillDiscardPile = new List<int>();
            for (int i = 0; i < defaultSkillCount; i++) {
                ShiftASkill();
            }
        }

        public override int MoveToTile(Location targetLoc, bool isInstance = false)
        {
            int movesteps = base.MoveToTile(targetLoc, isInstance);
            for (int i = 0; i < movesteps; i++) {
                ShiftASkill();
            }

            return movesteps;
        }

        public override void OnTurnStarted()
        {
            base.OnTurnStarted();
            SaveStatus();
        }

        public override void CastSkill(int skillID, Location castLoc)
        {
            base.CastSkill(skillID, castLoc);
            RemoveASkill(skillID);
            SaveStatus();
        }

        public override void DropAnimation()
        {
            transform.GetChild(0).localPosition = new Vector3(0, 5, 0);
            transform.GetChild(0).DOLocalMoveY(0.3f, 0.5f);
        }

        public void RemoveASkill(int skillID)
        {
            if (SkillDeck.Count == 0) {
                //SkillDeadwood.Sort();
                // might be new List<int>(SkillDeadwood)
                SkillDeck = SkillDiscardPile;
                SkillDiscardPile = new List<int>();
            }
            int r = SkillDeck.Count.Random();
            SkillHashes.Insert(0, SkillDeck[r]);
            SkillDeck.RemoveAt(r);
            SkillDiscardPile.Add(SkillHashes[skillID+1]);
            SkillHashes.RemoveAt(skillID+1);
            ssAnimEvent?.Invoke(ssAnimDuration);
        }

        public void ShiftASkill()
        {
            if(SkillDeck.Count==0) {
                //SkillDeadwood.Sort();
                // might be new List<int>(SkillDeadwood)
                SkillDeck = SkillDiscardPile;
                SkillDiscardPile = new List<int>();
            }

            int r = SkillDeck.Count.Random();
            SkillHashes.Insert(0, SkillDeck[r]);
            ssAnimEvent?.Invoke(ssAnimDuration);
            SkillDeck.RemoveAt(r);
            if (SkillHashes.Count <= defaultSkillCount) return;

            SkillDiscardPile.Add(SkillHashes[defaultSkillCount]);
            SkillHashes.RemoveAt(defaultSkillCount);            
        }

        public void CancelMove()
        {
            MoveToTile(lastLoc, true);
            SkillHashes = lastSkills;
            SkillDeck = lastSkillDeck;
            SkillDiscardPile = lastSkillDiscardPile;
            ActionPoints = lastActionPoints;
            OnAPChanged?.Invoke();
        }

        private void SaveStatus()
        {
            lastLoc = Loc;
            lastSkills = new List<int>(SkillHashes);
            lastSkillDeck = new List<int>(SkillDeck);
            lastSkillDiscardPile = new List<int>(SkillDiscardPile);
            lastActionPoints = ActionPoints;
        }
    }
}
