using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace Wing.RPGSystem {
    public class CreatureController : Entity
    {
        public SpriteMask mask_hp;

        private BaseCreature scriptableCreature;

        protected override void Awake()
        {
            OnHpChanged += OnCreatureHpChanged;
            base.Awake();
        }


        public void InitCreature(BaseCreature creature, TileController tile)
        {           
            scriptableCreature = creature;
            m_attribute = scriptableCreature.attribute;
            m_healthPoints = MaxHealthPoints;
            m_manaPoints = MaxManaPoints;

            foreach (var buff in scriptableCreature.buffs) {
                buffManager.AddBuff(new BuffHandler(this,this,buff));
            }

            MoveToTile(tile, null);
        }

        private void OnCreatureHpChanged(int value)
        {
            DOTween.To(() => mask_hp.alphaCutoff, alpha => mask_hp.alphaCutoff = alpha, 1 - 1.0f * value / MaxHealthPoints, 1);
        }
    }
}
