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

        public void InitPlayer(EntityAttribute attributes, BaseSkill[] skills)
        {
            
            m_attribute = attributes;           
            EntityName = "Player";
            EntityHash = EntityManager.Instance.AddEntity(this);            
            BuffManager = new BuffManager(EntityHash);

            HealthPoints = MaxHealthPoints;
            Skills = skills;
            ActiveSkills = new InstanceSkill[Skills.Length];
            for (int i = 0; i < Skills.Length; i++) {

                ActiveSkills[i] = new InstanceSkill(Skills[i]);
            }
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

            MoveToTile(lastTile, true);
            ActionPoints = lastActionPoints;
            OnAPChanged?.Invoke();
        }
    }
}
