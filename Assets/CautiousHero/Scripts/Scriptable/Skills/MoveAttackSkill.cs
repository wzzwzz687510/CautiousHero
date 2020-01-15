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

        public override void ApplyEffect(int casterHash, Location castLoc, bool anim)
        {
            if(!isMovementFirst) base.ApplyEffect(casterHash, castLoc, true);
            Entity caster = casterHash.GetEntity();
            caster.MoveToTile(castLoc, 0, isInstanceMovement);
            if (BattleManager.Instance.IsPlayerTurn)
                AnimationManager.Instance.PlayOnce();

            if (isMovementFirst) base.ApplyEffect(casterHash, castLoc, true);
        }
    }
}

