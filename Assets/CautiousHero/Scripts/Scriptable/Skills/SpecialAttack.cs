using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wing.RPGSystem
{
    [CreateAssetMenu(fileName = "Skill", menuName = "Wing/Scriptable Skills/SpecialAttack", order = 11)]
    public class SpecialAttack : BasicAttackSkill
    {
        public BaseBuff[] impactBuffs;

        public override void ApplyEffect(int casterHash, Location castLoc)
        {
            base.ApplyEffect(casterHash, castLoc);
            TileController tc = castLoc.GetTileController();
            if (!tc.IsEmpty) {
                foreach (var buff in impactBuffs) {
                    tc.StayEntity.BuffManager.AddBuff(new BuffHandler(casterHash, tc.StayEntity.Hash, buff.Hash));
                }
            }
        }
    }
}

