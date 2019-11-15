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
        protected override void Awake()
        {
            m_healthPoints = 100;
            m_attribute = new EntityAttribute(1, 100, 3, 1, 1, 1);
            base.Awake();
        }

        public override void MoveToTile(TileController targetTile, Stack<Location> path, bool anim = false)
        {
            if (targetTile == locateTile)
                return;
                
            if (!anim) {
                transform.GetChild(0).localPosition = new Vector3(0, 5, 0);
                transform.GetChild(0).DOLocalMoveY(0.2f, 0.5f);
            }
            base.MoveToTile(targetTile, path, anim);
        }
    }
}
