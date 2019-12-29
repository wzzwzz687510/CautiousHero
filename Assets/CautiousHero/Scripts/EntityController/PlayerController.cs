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
        public List<int> SkillDeadwood { get; private set; }

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

            HealthPoints = MaxHealthPoints;
            SkillDeck = new List<int>(Database.Instance.ActiveData.learnedSkills);
            SkillDeadwood = new List<int>();
            for (int i = 0; i < defaultSkillCount; i++) {
                ShiftASkill();
            }
        }

        public override void OnTurnStarted()
        {
            base.OnTurnStarted();

            lastLoc = Loc;
            lastActionPoints = ActionPoints;
        }

        public override void CastSkill(int skillID, Location castLoc)
        {
            base.CastSkill(skillID, castLoc);
            lastLoc = Loc;
            lastActionPoints = ActionPoints;
        }

        public override void DropAnimation()
        {
            transform.GetChild(0).localPosition = new Vector3(0, 5, 0);
            transform.GetChild(0).DOLocalMoveY(0.3f, 0.5f);
        }

        public void ShiftASkill()
        {
            if(SkillDeck.Count==0) {
                //SkillDeadwood.Sort();

                // might be new List<int>(SkillDeadwood)
                SkillDeck = SkillDeadwood;
                SkillDeadwood = new List<int>();
            }

            int r = SkillDeck.Count.Random();
            SkillHashes.Insert(0, SkillDeck[r]);
            ssAnimEvent?.Invoke(ssAnimDuration);
            if (SkillHashes.Count <= defaultSkillCount) return;

            SkillDeadwood.Add(SkillHashes[defaultSkillCount]);
            SkillHashes.RemoveAt(defaultSkillCount);            
        }

        public void CancelMove()
        {
            MoveToTile(lastLoc, true);
            ActionPoints = lastActionPoints;
            OnAPChanged?.Invoke();
        }
    }
}
