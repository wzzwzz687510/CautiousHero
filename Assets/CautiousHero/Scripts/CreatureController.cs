﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace Wing.RPGSystem {
    public class CreatureController : Entity
    {
        public SpriteMask mask_hp;
        public SpriteMask mask_hpEffect;

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
            HealthPoints = MaxHealthPoints;
            Skills = scriptableCreature.skills;
            ActiveSkills = new InstanceSkill[Skills.Length];
            for (int i = 0; i < Skills.Length; i++) {
                ActiveSkills[i] = new InstanceSkill(Skills[i]);
            }

            foreach (var buff in scriptableCreature.buffs) {
                buffManager.AddBuff(new BuffHandler(this,this,buff));
            }

            MoveToTile(tile, false);
            DropAnimation();
        }

        private void OnCreatureHpChanged(int value)
        {
            float tmp = 1 - 1.0f * value / MaxHealthPoints;
            mask_hp.alphaCutoff = tmp;
            DOTween.To(() => mask_hpEffect.alphaCutoff, alpha => mask_hpEffect.alphaCutoff = alpha, tmp, 1);
        }
    }
}