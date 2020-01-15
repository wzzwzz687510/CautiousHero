using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wing.RPGSystem
{
    [CreateAssetMenu(fileName = "Skill", menuName = "Wing/Scriptable Skills/ImpactSkill", order = 12)]
    public class ImpactSkill : BaseSkill
    {
        public int healthPoints;
        public int physicalArmourPoints;
        public int magicalArmourPoints;
        public int actionPoints;

        public override void ApplyEffect(int casterHash, Location casterLoc, Location selecLoc, bool anim)
        {
            TileController tc = selecLoc.GetTileController();
            if (!tc.IsEmpty) {
                AnimationManager.Instance.AddAnimClip(new CastAnimClip(castType,
                        Hash, casterHash.GetEntity().Loc, selecLoc, castEffect.animDuration));
                if (BattleManager.Instance.IsPlayerTurn)
                    AnimationManager.Instance.PlayOnce();
                tc.StayEntity.ImpactHP(Mathf.Abs(healthPoints), healthPoints < 0);
                tc.StayEntity.ImpactArmour(Mathf.Abs(physicalArmourPoints), true, physicalArmourPoints < 0);
                tc.StayEntity.ImpactArmour(Mathf.Abs(magicalArmourPoints), false, magicalArmourPoints < 0);
                tc.StayEntity.ImpactActionPoints(Mathf.Abs(actionPoints), actionPoints < 0);
            }
            base.ApplyEffect(casterHash, casterLoc, selecLoc, false);
        }
    }
}

