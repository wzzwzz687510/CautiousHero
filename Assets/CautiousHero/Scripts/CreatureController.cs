using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace Wing.RPGSystem {
    public class CreatureController : Entity
    {
        [Header("Creature Settings")]
        public SpriteRenderer hpBar;
        public SpriteMask mask_hp;
        public SpriteMask mask_hpEffect;

        private BaseCreature scriptableCreature;

        protected override void Awake()
        {
            OnHPDropped += OnCreatureHpChanged;
            base.Awake();
        }

        public override void MoveToTile(TileController targetTile, bool anim = true)
        {
            base.MoveToTile(targetTile, anim);
            hpBar.sortingOrder = m_sprite.sortingOrder + 1;
        }

        public IEnumerator InitCreature(BaseCreature creature, TileController tile)
        {           
            scriptableCreature = creature;
            m_sprite.sprite = scriptableCreature.sprite;
            m_attribute = scriptableCreature.attribute;
            HealthPoints = MaxHealthPoints;
            Skills = scriptableCreature.skills;
            ActiveSkills = new InstanceSkill[Skills.Length];
            for (int i = 0; i < Skills.Length; i++) {
                ActiveSkills[i] = new InstanceSkill(Skills[i]);
            }

            foreach (var buff in scriptableCreature.buffs) {
                BuffManager.AddBuff(new BuffHandler(this,this,buff));
            }

            hpBar.enabled = false;
            mask_hpEffect.alphaCutoff = 1;
            mask_hp.alphaCutoff = 1;
            MoveToTile(tile, false);
            DropAnimation();
            yield return new WaitForSeconds(0.5f);
            hpBar.enabled = true;
            OnCreatureHpChanged(false);
        }

        private void OnCreatureHpChanged(bool isDrop)
        {
            float tmp = 1 - 1.0f * HealthPoints / MaxHealthPoints;
            if (isDrop) {
                mask_hp.alphaCutoff = tmp;
                DOTween.To(() => mask_hpEffect.alphaCutoff, alpha => mask_hpEffect.alphaCutoff = alpha, tmp, 1);
            }
            else
            {
                DOTween.To(() => mask_hp.alphaCutoff, alpha => mask_hp.alphaCutoff = alpha, tmp, 1.5f);
                DOTween.To(() => mask_hpEffect.alphaCutoff, alpha => mask_hpEffect.alphaCutoff = alpha, tmp, 1.5f);
            }          
        }
    }
}
