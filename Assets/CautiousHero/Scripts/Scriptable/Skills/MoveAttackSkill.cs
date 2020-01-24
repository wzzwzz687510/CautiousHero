using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wing.RPGSystem
{
    [CreateAssetMenu(fileName = "Skill", menuName = "Wing/Scriptable Skills/MoveAttackSkill", order = 11)]
    public class MoveAttackSkill : BasicAttackSkill
    {
        public bool isInstanceMovement;
        public bool isMovementFirst;

        public override void ApplyEffect(int casterHash, Location casterLoc, Location selecLoc, bool anim)
        {
            if(!isMovementFirst) base.ApplyEffect(casterHash,casterLoc, selecLoc, true);
            Entity caster = casterHash.GetEntity();
            if (selecLoc.IsUnblocked()) {
                caster.MoveToTile(selecLoc, 0, isInstanceMovement);
                if (BattleManager.Instance.IsPlayerTurn)
                    AnimationManager.Instance.PlayOnce();
            }

            if (isMovementFirst) {
                base.ApplyEffect(casterHash, casterLoc, selecLoc, true);
            }
        }
    }
}

