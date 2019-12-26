using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using SpriteGlow;

namespace Wing.RPGSystem
{
    public class PlayerController : Entity
    {
        private TileController lastTile;
        private int lastActionPoints;

        public delegate void SkillShiftAnimation(float duration);
        public SkillShiftAnimation ssAnimEvent;

        public void InitPlayer(EntityAttribute attributes, BaseSkill[] skills)
        {
            
            m_attribute = attributes;           
            EntityName = "Player";
            Hash = EntityManager.Instance.AddEntity(this);            
            BuffManager = new BuffManager(Hash);

            HealthPoints = MaxHealthPoints;
            Skills = skills;
            ActiveSkills = new InstanceSkill[Skills.Length];
            for (int i = 0; i < Skills.Length; i++) {

                ActiveSkills[i] = new InstanceSkill(Skills[i]);
            }
        }

        public override void OnTurnStarted()
        {
            base.OnTurnStarted();

            lastTile = LocateTile;
            lastActionPoints = ActionPoints;
        }

        public override void CastSkill(int skillID, Location castLoc)
        {
            base.CastSkill(skillID, castLoc);
            lastTile = LocateTile;
            lastActionPoints = ActionPoints;
        }

        public void CancelMove()
        {
            if (!lastTile)
                return;

            MoveToTile(lastTile, true);
            ActionPoints = lastActionPoints;
            OnAPChanged?.Invoke();
        }

        public override void DropAnimation()
        {
            transform.GetChild(0).localPosition = new Vector3(0, 5, 0);
            transform.GetChild(0).DOLocalMoveY(0.3f, 0.5f);
        }

        public void SetActionPoints(int value)
        {
            ActionPoints = value;
        }
    }
}
