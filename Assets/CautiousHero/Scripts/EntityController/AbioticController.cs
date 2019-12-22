using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wing.RPGSystem
{
    public class AbioticController : Entity
    {
        public BaseAbiotic Template { get; protected set; }

        public void InitAbioticEntity(BaseAbiotic template,TileController tc)
        {
            Template = template;
            EntityName = Template.abioticName;
            EntityHash = EntityManager.Instance.AddEntity(this);
            BuffManager = new BuffManager(EntityHash);
            foreach (var buff in Template.buffs) {
                BuffManager.AddBuff(new BuffHandler(this, this, buff));
            }
            m_spriteRenderer.sprite = Template.sprite[template.sprite.Length.Random()];
            m_attribute = Template.attribute;
            HealthPoints = MaxHealthPoints;

            //EntitySprite.transform.localScale = new Vector3(0.1f / EntitySprite.size.x, 0.2f / EntitySprite.size.y);
            

            MoveToTile(tc, true);
            DropAnimation();
        }

        public override void DropAnimation()
        {
            transform.GetChild(0).localPosition = new Vector3(0, 0.15f, 0);
            EntitySprite.DOFade(0, 0);
            StartCoroutine(WaitDisplay());
        }

        IEnumerator WaitDisplay()
        {            
            while (!LocateTile.m_animator.GetCurrentAnimatorStateInfo(0).IsName("tile_fall")) {
                yield return null;
            }
            EntitySprite.DOFade(1, 0);
        }
    }
}

