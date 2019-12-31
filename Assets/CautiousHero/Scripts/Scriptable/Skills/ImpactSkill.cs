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

        public override void ApplyEffect(int casterHash, Location castLoc)
        {           
            TileController tc = castLoc.GetTileController();
            if (!tc.IsEmpty) {
                tc.StayEntity.ImpactHP(Mathf.Abs(healthPoints), healthPoints < 0);
                tc.StayEntity.ImpactArmour(Mathf.Abs(physicalArmourPoints),true, physicalArmourPoints < 0);
                tc.StayEntity.ImpactArmour(Mathf.Abs(magicalArmourPoints), false, magicalArmourPoints < 0);
                tc.StayEntity.ImpactActionPoints(Mathf.Abs(actionPoints), actionPoints < 0);
            }
        }
    }
}

