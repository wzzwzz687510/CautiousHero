using System.Collections;
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
        private int lastActionPoints;

        public delegate void SkillShiftAnimation(float duration);
        public SkillShiftAnimation ssAnimEvent;

        public void InitPlayer(EntityAttribute attributes)
        {
            
            m_attribute = attributes;           
            EntityName = "Player";
            Hash = EntityManager.Instance.AddEntity(this);            
            BuffManager = new BuffManager(Hash);
            InitSkillDeck();

            HealthPoints = MaxHealthPoints;
        }

        public void InitSkillDeck()
        {
            SkillDeck = new List<int>(WorldData.ActiveData.learnedSkills);
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

        public void MoveToLocation(Location targetLoc,bool isWorldMap, bool isInstance)
        {
            if (targetLoc == Loc)  return;

            if (!isInstance) {
                TileNavigation nav = isWorldMap ? WorldMapManager.Instance.Nav : GridManager.Instance.Nav;
                if (!nav.HasPath(Loc, targetLoc)) return;
                Stack<Location> path = nav.GetPath(Loc, targetLoc);

                Vector3[] sortedPath = new Vector3[path.Count];
                if (isWorldMap) {
                    for (int i = 0; i < sortedPath.Length; i++) {
                        sortedPath[i] = path.Pop().ToWorldView();
                    }
                }
                else {
                    for (int i = 0; i < sortedPath.Length; i++) {
                        sortedPath[i] = path.Pop().ToAreaView();
                    }
                }

                MovePath = sortedPath;
            }

            if(isWorldMap) Loc = targetLoc;
            else {
                if (Loc.TryGetTileController(out TileController leaveTile)) {
                    leaveTile.OnEntityLeaving();
                }
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
