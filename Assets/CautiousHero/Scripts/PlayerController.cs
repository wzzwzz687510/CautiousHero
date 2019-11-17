using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wing.TileUtils;
using DG.Tweening;
using SpriteGlow;

namespace Wing.RPGSystem
{
    public class PlayerController : Entity
    {
        private TileController lastTile;
        private int lastActionPoints;

        protected override void Awake()
        {
            HealthPoints = 100;
            m_attribute = new EntityAttribute(1, 100, 3, 1, 1, 1, 1);
            base.Awake();
        }

        public override void OnEntityTurnStart()
        {
            base.OnEntityTurnStart();

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

            MoveToTile(lastTile, false);
            ActionPoints = lastActionPoints;
            OnAPChanged?.Invoke();
        }

        public void InitPlayer(BaseSkill[] skills)
        {
            Skills = skills;
            ActiveSkills = new InstanceSkill[Skills.Length];
            for (int i = 0; i < Skills.Length; i++) {
                
ActiveSkills[i] = new InstanceSkill(Skills[i]);
            }
        }
    }
}
