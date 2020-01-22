using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wing.RPGSystem
{
    [CreateAssetMenu(fileName = "Skill", menuName = "Wing/Scriptable Skills/PushSkill", order = 11)]
    public class PushSkill : BaseSkill
    {
        public int backSteps;
        public BaseBuff[] impactBlockGainedBuff;

        public override void ApplyEffect(int casterHash, Location casterLoc, Location selecLoc, bool anim)
        {
            Location cp = selecLoc - casterLoc;
            foreach (var el in GetSubEffectZone(casterLoc, cp)) {
                for (int i = 0; i < backSteps; i++) {
                    Location targetLoc = el + cp * (backSteps - i);
                    if (targetLoc.IsEmpty()) {
                        Entity target = el.GetTileController().StayEntity;
                        target.MoveToTile(targetLoc,0);
                        if (i != 0) {
                            foreach (var buff in impactBlockGainedBuff) {
                                target.EntityBuffManager.AddBuff(new BuffHandler(casterHash, target.Hash, buff.Hash));
                            }
                        }
                        break;
                    }
                }
            }
            
            base.ApplyEffect(casterHash, casterLoc, selecLoc, anim);
        }
    }
}

