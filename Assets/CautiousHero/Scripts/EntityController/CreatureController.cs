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
        public int NextCastSkillID { get; protected set; }
        public Entity NextSkillTarget { get { return nextSkillTargetHash.GetEntity(); } }        
        public BaseSkill NextSkill { get { return SkillHashes[NextCastSkillID].GetBaseSkill(); } }
        private int nextSkillTargetHash;

        protected override void Awake()
        {
            HPChangeAnimation += CreatureHpChangeAnimation;
            base.Awake();
        }

        public IEnumerator InitCreature(BaseCreature creature, Location loc)
        {           
            Template = creature;
            EntityName = Template.creatureName;
            Hash = EntityManager.Instance.AddEntity(this);
            BuffManager = new BuffManager(Hash);
            foreach (var buff in Template.buffs) {
                BuffManager.AddBuff(new BuffHandler(Hash, Hash, buff.Hash));
            }
            m_spriteRenderer.sprite = Template.sprite;
            m_attribute = Template.attribute;
            HealthPoints = MaxHealthPoints;

            foreach (var skill in Template.skills) {
                SkillHashes.Add(skill.Hash);
            }
            NextCastSkillID = 0;

            var h = EntitySprite.bounds.size.y;
            hpBar.transform.localPosition += new Vector3(0, h, 0);

            hpBar.enabled = false;
            mask_hpEffect.alphaCutoff = 1;
            mask_hp.alphaCutoff = 1;
            MoveToTile(loc, true);
            DropAnimation();
            yield return new WaitForSeconds(0.5f);
            hpBar.enabled = true;
            CreatureHpChangeAnimation(1,1);
        }

        public void SetNextSkillTarget(int hash)
        {
            nextSkillTargetHash = hash;
        }

        public override void CastSkill(int skillID, Location castLoc)
        {
            base.CastSkill(skillID, castLoc);
            NextCastSkillID = NextCastSkillID == SkillHashes.Count - 1 ? 0 : NextCastSkillID++;
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

        private void CreatureHpChangeAnimation(float hpRatio, float duraion)
        {
            if (hpRatio == 0) {
                hpBar.enabled = false;
                mask_hp.alphaCutoff = 1;
                mask_hpEffect.alphaCutoff = 1;
                EntitySprite.DOColor(Color.black, 1);
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
            
            Loc.GetTileController().OnEntityLeaving();
            AIManager.Instance.OnBotDeath(this);
        }
    }
}
