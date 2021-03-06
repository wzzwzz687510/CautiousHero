﻿using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wing.RPGSystem
{
    public class AbioticController : Entity
    {
        public BaseAbiotic Template { get; protected set; }

        public void InitAbioticEntity(BaseAbiotic template,Location loc)
        {
            Template = template;
            EntityName = Template.abioticName;
            Hash = EntityManager.Instance.AddEntity(this);
            EntityBuffManager = new BuffManager(Hash);
            foreach (var buff in Template.buffs) {
                EntityBuffManager.AddBuff(new BuffHandler(Hash, Hash, buff.Hash));
            }
            m_spriteRenderer.sprite = Template.sprite[template.sprite.Length.Random()];
            m_attribute = Template.attribute;
            HealthPoints = MaxHealthPoints;

            //EntitySprite.transform.localScale = new Vector3(0.1f / EntitySprite.size.x, 0.2f / EntitySprite.size.y);       

            MoveToTile(loc, MoveCost,true);
            transform.localPosition = Vector3.zero;
            DropAnimation();
        }

        public override void DropAnimation()
        {
            transform.GetChild(0).localPosition = new Vector3(0, 0.15f, 0);
            EntitySprite.DOFade(0, 0);
        }

    }
}

