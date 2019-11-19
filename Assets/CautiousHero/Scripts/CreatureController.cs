using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Wing.TileUtils;

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
            OnHPChanged += OnCreatureHpChanged;
            base.Awake();
        }

        public override void MoveToTile(TileController targetTile, bool isInstant = false)
        {
            base.MoveToTile(targetTile, isInstant);
            hpBar.sortingOrder = m_spriteRenderer.sortingOrder + 1;
        }

        public IEnumerator InitCreature(BaseCreature creature, TileController tile)
        {           
            scriptableCreature = creature;
            EntityName = scriptableCreature.creatureName;
            EntityHash = EntityManager.Instance.AddEntity(this);
            BuffManager = new BuffManager(EntityHash);
            foreach (var buff in scriptableCreature.buffs) {
                BuffManager.AddBuff(new BuffHandler(this, this, buff));
            }
            m_spriteRenderer.sprite = scriptableCreature.sprite;
            m_attribute = scriptableCreature.attribute;
            HealthPoints = MaxHealthPoints;
            Skills = scriptableCreature.skills;
            ActiveSkills = new InstanceSkill[Skills.Length];
            for (int i = 0; i < Skills.Length; i++) {
                ActiveSkills[i] = new InstanceSkill(Skills[i]);
            }    

            hpBar.enabled = false;
            mask_hpEffect.alphaCutoff = 1;
            mask_hp.alphaCutoff = 1;
            MoveToTile(tile, true);
            DropAnimation();
            yield return new WaitForSeconds(0.5f);
            hpBar.enabled = true;
            OnCreatureHpChanged(1,1);
        }

        private void OnCreatureHpChanged(float hpRatio, float duraion)
        {
            if (1 - mask_hp.alphaCutoff > hpRatio) {
                mask_hp.alphaCutoff = 1 - hpRatio;
                DOTween.To(() => mask_hpEffect.alphaCutoff, alpha => mask_hpEffect.alphaCutoff = alpha, 1 - hpRatio, 1);
            }
            else {
                DOTween.To(() => mask_hp.alphaCutoff, alpha => mask_hp.alphaCutoff = alpha, 1 - hpRatio, 1.5f);
                DOTween.To(() => mask_hpEffect.alphaCutoff, alpha => mask_hpEffect.alphaCutoff = alpha, 1 - hpRatio, 1.5f);
            }
        }
    }
}
