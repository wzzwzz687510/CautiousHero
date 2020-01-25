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
        public EntityAttribute attribute;

        public override void ApplyEffect(int casterHash, Location casterLoc, Location selecLoc, bool anim)
        {
            TileController tc = selecLoc.GetTileController();
            if (!tc.IsEmpty) {
                AnimationManager.Instance.AddAnimClip(new CastAnimClip(castType,
                        Hash, casterHash.GetEntity().Loc, selecLoc, castEffect.animDuration));
                if (BattleManager.Instance.IsPlayerTurn)
                    AnimationManager.Instance.PlayOnce();
                Entity entity = tc.StayEntity;
                entity.ImpactHP(Mathf.Abs(healthPoints), healthPoints < 0);
                entity.ImpactArmour(Mathf.Abs(physicalArmourPoints), true, physicalArmourPoints < 0);
                entity.ImpactArmour(Mathf.Abs(magicalArmourPoints), false, magicalArmourPoints < 0);
                entity.ImpactActionPoints(Mathf.Abs(actionPoints), actionPoints < 0);
                entity.ImpactAttribute(attribute);
            }
            base.ApplyEffect(casterHash, casterLoc, selecLoc, false);
        }
    }
}

