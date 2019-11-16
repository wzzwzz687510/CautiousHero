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
            ActionPoints = 3;
            base.Awake();
        }

        public override void MoveToTile(TileController targetTile, bool anim = true)
        {
            if (targetTile == locateTile)
                return;

            if (anim) {
                Stack<Location> path = GridManager.Instance.Astar.GetPath(Loc, targetTile.Loc);

                if (path.Count * MoveCost > ActionPoints)
                    return;

                Location[] sortedPath = new Location[path.Count];
                for (int i = 0; i < sortedPath.Length; i++) {
                    sortedPath[i] = path.Pop();
                }
                paths.Add(sortedPath);
                MoveAnimation();
            }
            else {
                transform.position = targetTile.transform.position;
            }
            if (locateTile) {
                locateTile.OnEntityLeaving();
                lastTile = locateTile;
            }
            locateTile = targetTile;
            targetTile.OnEntityEntering(this);
            lastActionPoints = ActionPoints;
            ActionPoints -= paths.Count * MoveCost;
            m_sprite.sortingOrder = targetTile.Loc.x + targetTile.Loc.y * 8;
        }

        public void CancelMove()
        {
            if (!lastTile)
                return;

            transform.position = lastTile.transform.position;
            ActionPoints = lastActionPoints;
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
