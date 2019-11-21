using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace Wing.RPGSystem {
    public class CreatureController : Entity
    {
        [Header("Creature Settings")]
        public SpriteRenderer hpBar;
        public SpriteRenderer hpSR;
        public SpriteRenderer hpESR;
        public SpriteMask mask_hp;
        public SpriteMask mask_hpEffect;

        public BaseCreature Template { get; protected set; }

        protected override void Awake()
        {
            OnHPChanged += OnCreatureHpChanged;
            base.Awake();
        }

        public IEnumerator InitCreature(BaseCreature creature, TileController tile)
        {           
            Template = creature;
            EntityName = Template.creatureName;
            EntityHash = EntityManager.Instance.AddEntity(this);
            BuffManager = new BuffManager(EntityHash);
            foreach (var buff in Template.buffs) {
                BuffManager.AddBuff(new BuffHandler(this, this, buff));
            }
            m_spriteRenderer.sprite = Template.sprite;
            m_attribute = Template.attribute;
            HealthPoints = MaxHealthPoints;
            Skills = Template.skills;
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

        protected override void OnSortingOrderChangedEvent(int sortingOrder)
        {
            base.OnSortingOrderChangedEvent(sortingOrder);
            hpBar.sortingOrder = sortingOrder + 1;
            hpESR.sortingOrder = sortingOrder + 2;
            mask_hpEffect.frontSortingOrder = sortingOrder + 2;
            mask_hpEffect.backSortingOrder = sortingOrder + 1;
            hpSR.sortingOrder = sortingOrder + 3;
            mask_hp.frontSortingOrder = sortingOrder + 3;
            mask_hp.backSortingOrder = sortingOrder + 2;
        }

        private void OnCreatureHpChanged(float hpRatio, float duraion)
        {
            if (hpRatio == 0) {
                hpBar.enabled = false;
                mask_hp.alphaCutoff = 1;
                mask_hpEffect.alphaCutoff = 1;
                Sprite.DOColor(Color.black, 1);
                m_glowEffect.GlowColor = Color.black;
                return;
            }

            if (1 - mask_hp.alphaCutoff > hpRatio) {
                mask_hp.alphaCutoff = 1 - hpRatio;
                DOTween.To(() => mask_hpEffect.alphaCutoff, alpha => mask_hpEffect.alphaCutoff = alpha, 1 - hpRatio, 1);
            }
            else {
                DOTween.To(() => mask_hp.alphaCutoff, alpha => mask_hp.alphaCutoff = alpha, 1 - hpRatio, 1.5f);
                DOTween.To(() => mask_hpEffect.alphaCutoff, alpha => mask_hpEffect.alphaCutoff = alpha, 1 - hpRatio, 1.5f);
            }
        }

        protected override void Death()
        {
            base.Death();
            
            LocateTile.OnEntityLeaving();
        }
    }
}
