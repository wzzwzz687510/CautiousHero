using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wing.RPGSystem {
    public class CreatureController : Entity
    {
        private BaseCreature scriptableCreature;
        
        public void InitCreature(BaseCreature creature, TileController tile)
        {
            scriptableCreature = creature;
            m_healthPoints = MaxHealthPoints;
            m_manaPoints = MaxManaPoints;

            foreach (var buff in scriptableCreature.buffs) {
                buffs.Add(new BuffHandler(buff));
            }

            MoveToTile(tile, null);
        }
    }
}
