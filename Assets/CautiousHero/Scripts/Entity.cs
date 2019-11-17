using DG.Tweening;
using SpriteGlow;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Wing.TileUtils;

namespace Wing.RPGSystem
{
    public class InstanceSkill
    {
        public BaseSkill TSkill { get; private set; }
        public int Cooldown { get; private set; }
        public bool Castable { get { return Cooldown <= 0; } }

        public InstanceSkill(BaseSkill skill)
        {
            TSkill = skill;
            Cooldown = 0;
        }

        public void SetCooldown(int num)
        {
            Cooldown = num;
        }

        public bool UpdateSkill()
        {
            return Castable || --Cooldown <= 0;
        }
    }

    public class Entity : MonoBehaviour
    {
        public int HealthPoints { get; protected set; }
        public int ActionPoints { get; protected set; }
        public bool isDeath { get; protected set; }
        public BaseSkill[] Skills { get; protected set; }
        public InstanceSkill[] ActiveSkills { get; protected set; }
        public BuffManager BuffManager { get; protected set; }

        public TileController LocateTile { get; protected set; }
        public Location Loc { get { return LocateTile.Loc; } }

        protected EntityAttribute m_attribute;
        public EntityAttribute Attribute {
            get {
                return m_attribute + BuffManager.GetAttributeAdjustment();
            }
        }
        public int Level { get { return Attribute.level; } }
        public int MaxHealthPoints { get { return Attribute.maxHealth; } }
        public int ActionPointsPerTurn { get { return Attribute.action; } }
        public int Strength { get { return Attribute.strength; } }
        public int Intelligence { get { return Attribute.intelligence; } }
        public int Agility { get { return Attribute.agility; } }
        public int MoveCost { get { return Attribute.moveCost; } }

        public SpriteRenderer Sprite { get { return m_sprite; } }

        protected SpriteRenderer m_sprite;
        protected SpriteGlowEffect m_glowEffect;
        protected BoxCollider2D m_collider;
        protected Vector3[] movePath;

        public delegate void BCallback(bool isEligible);
        public delegate void ICallback(int index,int cooldown);
        public event BCallback OnHPDropped;
        public event ICallback OnSkillUpdated;
        [HideInInspector] public UnityEvent OnAPChanged;
        [HideInInspector] public UnityEvent OnAnimationCompleted;

        protected virtual void Awake()
        {
            m_sprite = GetComponentInChildren<SpriteRenderer>();
            m_glowEffect = GetComponentInChildren<SpriteGlowEffect>();
            m_collider = GetComponentInChildren<BoxCollider2D>();
            BuffManager = new BuffManager(this);
            isDeath = false;
        }

        public virtual void OnEntityTurnStart()
        {
            BuffManager.UpdateBuffs();
            for (int i = 0; i < ActiveSkills.Length; i++) {
                ActiveSkills[i].UpdateSkill();
                OnSkillUpdated?.Invoke(i, ActiveSkills[i].Cooldown);
            }

            ActionPoints = Mathf.Min(ActionPoints + ActionPointsPerTurn, 8);
            OnAPChanged?.Invoke();
        }

        /// <summary>
        /// set anim false generally means move entity to one place instantly without move cost (e.g. by skill effect).
        /// </summary>
        /// <param name="targetTile"></param>
        /// <param name="anim"></param>
        public virtual void MoveToTile(TileController targetTile, bool anim = true)
        {
            if (targetTile == LocateTile) {

                return;
            }
                

            if (anim) {
                Stack<Location> path = GridManager.Instance.Astar.GetPath(Loc, targetTile.Loc);

                if (path.Count * MoveCost > ActionPoints) {

                    return;
                }
                    

                Vector3[] sortedPath = new Vector3[path.Count];
                for (int i = 0; i < sortedPath.Length; i++) {
                    sortedPath[i] = path.Pop();
                }
                movePath = sortedPath;
                StartCoroutine(MoveAnimation());
                ActionPoints -= movePath.Length * MoveCost;
                OnAPChanged?.Invoke();
            }
            else {
                transform.position = targetTile.transform.position;
            }

            if (LocateTile) {
                LocateTile.OnEntityLeaving();
            }
            LocateTile = targetTile;
            targetTile.OnEntityEntering(this);

            m_sprite.sortingOrder = targetTile.Loc.x + targetTile.Loc.y * 8;           
        }

        public virtual void CastSkill(int skillID, Location castLoc)
        {
            var tSkill = Skills[skillID];
            ActionPoints -= tSkill.actionPointsCost;
            OnAPChanged?.Invoke();
            ActiveSkills[skillID].SetCooldown(tSkill.cooldownTime);
            OnSkillUpdated?.Invoke(skillID, tSkill.cooldownTime);

            tSkill.ApplyEffect(this, castLoc);
            StartCoroutine(CastAnimation());
        }

        public virtual void DropAnimation()
        {
            transform.GetChild(0).localPosition = new Vector3(0, 5, 0);
            transform.GetChild(0).DOLocalMoveY(0.2f, 0.5f);
        }

        public virtual void ChangeOutlineColor(Color c)
        {
            m_glowEffect.GlowColor = c;
        }

        public virtual void SetActiveCollider(bool bl)
        {
            m_collider.enabled = bl;
        }

        protected virtual IEnumerator CastAnimation()
        {
            yield return null;
            OnAnimationCompleted?.Invoke();
        }

        protected virtual IEnumerator MoveAnimation()
        {
            transform.DOPath(movePath, 0.5f);
            yield return new WaitForSeconds(0.5f);
            if (OnAnimationCompleted == null)
                Debug.Log(2);
            OnAnimationCompleted?.Invoke();
        }

        public int GetDefendReducedValue(int value)
        {
            return Mathf.RoundToInt((value - BuffManager.GetReduceConstant(BuffType.Defend)) *
                (1 - BuffManager.GetReduceCof(BuffType.Defend)));
        }

        public virtual bool DealDamage(int value)
        {
            HealthPoints -= GetDefendReducedValue(value);
            OnHPDropped?.Invoke(true);
            if (HealthPoints > 0)
                return true;

            Death();
            return false;
        }

        public virtual void Death()
        {
            isDeath = true;
            BattleManager.Instance.GameConditionCheck();
        }


    }
}
