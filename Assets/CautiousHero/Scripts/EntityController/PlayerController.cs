﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using SpriteGlow;
using System.Linq;

namespace Wing.RPGSystem
{
    public class PlayerController : Entity
    {
        public float ssAnimDuration = 0.2f;
        public int defaultSkillCount = 5;

        public Animator m_animator;

        public List<int> SkillDeck { get; private set; }
        public List<int> SkillDiscardPile { get; private set; }

        private Location lastLoc;
        private int lastActionPoints;

        public delegate void SkillShiftAnimation(float duration);
        public SkillShiftAnimation ssAnimEvent;

        public void InitCharacter(string name, EntityAttribute attributes, int hp = 100)
        {
            m_attribute = attributes;
            EntityName = name;
            Hash = EntityManager.Instance.AddEntity(this);
            if (EntityBuffManager == null) EntityBuffManager = new BuffManager(Hash);
            HealthPoints = hp;
            PhysicalArmourPoints = 0;
            MagicalArmourPoints = 0;
            ActionPoints = 0;
            IsDeath = false;

            m_collider.enabled = true;
        }

        public void StartNewBattle()
        {
            EntityBuffManager.ResetManager();
            PhysicalArmourPoints = 0;
            MagicalArmourPoints = 0;
            ActionPoints = 0;
            foreach (var relicHash in WorldData.ActiveData.gainedRelicHashes) {
                relicHash.GetRelic().ApplyEffect(Hash);
            }

            ResetSkillDeck();
        }

        public void ResetSkillDeck()
        {
            SkillDeck = new List<int>(WorldData.ActiveData.learnedSkillHashes);
            SkillHashes = new List<int>();
            SkillDiscardPile = new List<int>();
            for (int i = 0; i < defaultSkillCount; i++) {
                ShiftASkill();
            }
        }

        public override int MoveToTile(Location targetLoc, int moveCost, bool isInstance = false)
        {
            int movesteps = base.MoveToTile(targetLoc, moveCost, isInstance);
            if (moveCost != 0)
                for (int i = 0; i < movesteps; i++) {
                    ShiftASkill();
                }

            return movesteps;
        }

        public void MoveToLocation(Location targetLoc,bool isWorldMap, bool isInstance)
        {
            if (targetLoc == Loc)  return;

            if (!isInstance) {
                TileNavigation nav = isWorldMap ? WorldMapManager.Instance.Nav : GridManager.Instance.Nav;
                if (!nav.HasPath(Loc, targetLoc)) return;
                Stack<Location> path = nav.GetPath(Loc, targetLoc);

                MovePath = path.ToArray();
            }

            if(isWorldMap) Loc = targetLoc;
            else {
                Loc.GetTileController().OnEntityLeaving();
                Loc = targetLoc;
                Loc.GetTileController().OnEntityEntering(Hash);
            }

            if (isInstance) {
                AnimationManager.Instance.AddAnimClip(new MoveInstantAnimClip(Hash, targetLoc, 0.2f));                
            }
            else {
                AnimationManager.Instance.AddAnimClip(new MovePathAnimClip(Hash, MovePath, 0.2f));
            }

            AnimationManager.Instance.PlayOnce();
        }

        public override void OnTurnStarted()
        {
            base.OnTurnStarted();
            SaveStatus();
        }

        public override void ImpactAttribute(EntityAttribute value)
        {
            base.ImpactAttribute(value);
            Database.Instance.SetCharacterData(m_attribute);
            Database.Instance.SetCharacterData(HealthPoints + value.maxHealth);
        }

        public override bool CastSkill(int skillID, Location castLoc)
        {
            bool res = base.CastSkill(skillID, castLoc);
            if (res) RemoveASkill(skillID);
            SaveStatus();
            return res;
        }

        public override void DropAnimation()
        {
            transform.GetChild(0).localPosition = new Vector3(0, 5, 0);
            transform.GetChild(0).DOLocalMoveY(0.3f, 0.5f);
        }

        public void RemoveASkill(int skillID)
        {
            if (SkillDeck.Count == 0) {
                SkillDeck = SkillDiscardPile;
                SkillDiscardPile = new List<int>();
            }
            int r = SkillDeck.Count.Random();
            SkillHashes.Insert(0, SkillDeck[r]);
            SkillDeck.RemoveAt(r);
            SkillDiscardPile.Add(SkillHashes[skillID+1]);
            SkillHashes.RemoveAt(skillID+1);
            ssAnimEvent?.Invoke(ssAnimDuration);
            //Debug.Log("skill: " + SkillDiscardPile[SkillDiscardPile.Count - 1] + " was removed");
        }

        public void ShiftASkill()
        {
            if(SkillDeck.Count==0) {
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
            //Debug.Log("skill: " + SkillDiscardPile[SkillDiscardPile.Count - 1] + " was shifted");
        }

        public void AddASkillToDeck(int skillHash)
        {
            SkillDeck.Add(skillHash);
        }

        public void AddASkillToDiscardPile(int skillHash)
        {
            SkillDiscardPile.Add(skillHash);
        }

        public void AddASkillToMind(int insertID,int skillHash)
        {
            SkillHashes.Insert(insertID, skillHash);
            SkillHashes.RemoveAt(defaultSkillCount);

        }

        private void SaveStatus()
        {
            lastLoc = Loc;
            lastActionPoints = ActionPoints;
        }
    }
}
