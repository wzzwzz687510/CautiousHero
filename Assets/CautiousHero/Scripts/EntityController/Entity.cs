using DG.Tweening;
using SpriteGlow;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Wing.RPGSystem
{
    [System.Serializable]
    public struct EntityAttribute
    {
        public int maxHealth;
        public int action;
        public int strength;
        public int intelligence;
        public int agility;
        public int moveCost;

        public EntityAttribute(int maxHp, int act, int str, int inte, int agi, int mvCost)
        {
            maxHealth = maxHp;
            action = act;
            strength = str;
            intelligence = inte;
            agility = agi;
            moveCost = mvCost;
        }

        public static EntityAttribute operator -(EntityAttribute a) =>
            new EntityAttribute(-a.maxHealth, -a.action, -a.strength, -a.intelligence, -a.agility, -a.moveCost);
        public static EntityAttribute operator +(EntityAttribute a, EntityAttribute b) =>
            new EntityAttribute(a.maxHealth + b.maxHealth, a.action + b.action,
                a.strength + b.strength, a.intelligence + b.intelligence, a.agility + b.agility, a.moveCost + b.moveCost);
        public static EntityAttribute operator -(EntityAttribute a, EntityAttribute b) => a + -(b);
    }

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
        public string EntityName { get; protected set; }
        public int Hash { get; protected set; }
        public int HealthPoints { get; protected set; }
        public int ArmorPoints { get; protected set; }
        public int ActionPoints { get; protected set; }
        public bool IsDeath { get; protected set; }
        public BaseSkill[] Skills { get; protected set; }
        public InstanceSkill[] ActiveSkills { get; protected set; }
        public BuffManager BuffManager { get; protected set; }

        public TileController LocateTile { get; protected set; }
        public Location Loc { get { return LocateTile.Loc; } }
        public Vector3[] MovePath { get; protected set; }

        protected EntityAttribute m_attribute;
        public EntityAttribute Attribute {
            get {
                return m_attribute + BuffManager.GetAttributeAdjustment();
            }
        }
        public int MaxHealthPoints { get { return Attribute.maxHealth; } }
        public int ActionPointsPerTurn { get { return Attribute.action; } }
        public int Strength { get { return Attribute.strength; } }
        public int Intelligence { get { return Attribute.intelligence; } }
        public int Agility { get { return Attribute.agility; } }
        public int MoveCost { get { return Attribute.moveCost; } }

        public SpriteRenderer EntitySprite { get { return m_spriteRenderer; } }

        protected SpriteRenderer m_spriteRenderer;
        protected SpriteGlowEffect m_glowEffect;
        protected BoxCollider2D m_collider;

        public delegate void SortingOrderChange(int sortingOrder);
        public SortingOrderChange OnSortingOrderChanged;
        public delegate void PointsChange(float ratio, float duraion);
        public PointsChange HPChangeAnimation;
        public PointsChange ArmorPointChangeAnimation;
        public delegate void SkillUpdat(int index,int cooldown);
        public event SkillUpdat OnSkillUpdated;

        [HideInInspector] public UnityEvent OnTurnStartedEvent;
        [HideInInspector] public UnityEvent OnTurnEndedEvent;
        [HideInInspector] public UnityEvent OnHPChanged;
        [HideInInspector] public UnityEvent OnArmorPointsChanged;
        [HideInInspector] public UnityEvent OnAPChanged;
        [HideInInspector] public UnityEvent OnSkillChanged;
        [HideInInspector] public UnityEvent OnDead;
        [HideInInspector] public UnityEvent OnAnimationCompleted;

        protected virtual void Awake()
        {
            m_spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            m_glowEffect = GetComponentInChildren<SpriteGlowEffect>();
            m_collider = GetComponentInChildren<BoxCollider2D>();
            IsDeath = false;
            OnSortingOrderChanged += OnSortingOrderChangedEvent;
        }

        public virtual void OnTurnStarted()
        {
            OnTurnStartedEvent?.Invoke();
            BuffManager.UpdateBuffs();
            for (int i = 0; i < ActiveSkills.Length; i++) {
                ActiveSkills[i].UpdateSkill();
                OnSkillUpdated?.Invoke(i, ActiveSkills[i].Cooldown);
            }

            ActionPoints = Mathf.Min(ActionPoints + ActionPointsPerTurn, 8);
            OnAPChanged?.Invoke();
        }

        public virtual void OnTurnEnded()
        {
            OnTurnEndedEvent?.Invoke();
        }

        /// <summary>
        /// set anim false generally means move entity to one place instantly without move cost (e.g. by skill effect).
        /// </summary>
        /// <param name="targetTile"></param>
        /// <param name="isInstance"></param>
        public virtual void MoveToTile(TileController targetTile, bool isInstance = false)
        {
            if (targetTile == LocateTile) {
                return;
            }

            if (!isInstance) {
                Stack<Location> path = GridManager.Instance.Astar.GetPath(Loc, targetTile.Loc);

                if (path.Count * MoveCost > ActionPoints) {

                    return;
                }

                Vector3[] sortedPath = new Vector3[path.Count];
                for (int i = 0; i < sortedPath.Length; i++) {
                    sortedPath[i] = path.Pop();
                }
                MovePath = sortedPath;

                // AP cost and invoke event
                ActionPoints -= sortedPath.Length * MoveCost;
                OnAPChanged?.Invoke();
                // Animation move
            }

            if (LocateTile) {
                LocateTile.OnEntityLeaving();
            }
            LocateTile = targetTile;
            targetTile.OnEntityEntering(this);


            if (isInstance) {               
                AnimationManager.Instance.AddAnimClip(new MoveInstantAnimClip(Hash, targetTile.Loc, 0.2f));
                AnimationManager.Instance.PlayOnce();
            }
            else {

                AnimationManager.Instance.AddAnimClip(new MovePathAnimClip(Hash, MovePath, 0.2f));
            }
                
        }

        public virtual void CastSkill(int skillID, Location castLoc)
        {
            var tSkill = Skills[skillID];
            if (ActionPoints < tSkill.actionPointsCost)
                return;
            ActionPoints -= tSkill.actionPointsCost;
            OnAPChanged?.Invoke();
            ActiveSkills[skillID].SetCooldown(tSkill.cooldownTime);
            OnSkillUpdated?.Invoke(skillID, tSkill.cooldownTime);

            tSkill.ApplyEffect(Hash, castLoc);
        }

        public virtual void DropAnimation()
        {
            transform.GetChild(0).localPosition = new Vector3(0, 5, 0);
            transform.GetChild(0).DOLocalMoveY(0.35f, 0.5f);
        }

        public virtual void ChangeOutlineColor(Color c)
        {
            m_glowEffect.GlowColor = c;
        }

        public virtual void SetActiveCollider(bool bl)
        {
            m_collider.enabled = bl;
        }

        public virtual bool DealDamage(int value)
        {
            int reducedDamage = CalculateFinalDamage(value);
            if (ArmorPoints>=reducedDamage) {
                DamageArmor(value);
                return true;
            }

            reducedDamage -= ArmorPoints;
            return DamageHP(reducedDamage, true);
        }

        public virtual void DamageArmor(int value, bool ignoreDefense = false)
        {
            int damage = ignoreDefense? value: CalculateFinalDamage(value);
            ArmorPoints -= damage;
            ArmorPoints = Mathf.Min(0, ArmorPoints);
            AnimationManager.Instance.AddAnimClip(new ArmorPointsChangeAnimClip(Hash, 1.0f * ArmorPoints / MaxHealthPoints));
            if (BattleManager.Instance.IsPlayerTurn)
                AnimationManager.Instance.PlayOnce(false);
            OnArmorPointsChanged?.Invoke();
        }

        /// <summary>
        /// HP -= adjusted value. Should calculate adjusted damage in function so that the direct damage to hp can be correct, such as Piercing Attack;
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual bool DamageHP(int value, bool ignoreDefense = false)
        {
            //Debug.Log("Entity: "+EntityName+", HP: " + HealthPoints);
            int damage = ignoreDefense? value: CalculateFinalDamage(value);
            int tmpHP = HealthPoints;
            HealthPoints -= damage;
            HealthPoints = Mathf.Clamp(HealthPoints, 0, MaxHealthPoints);
            AnimationManager.Instance.AddAnimClip(new HPChangeAnimClip(Hash, 1.0f * HealthPoints / MaxHealthPoints));
            if (BattleManager.Instance.IsPlayerTurn)
                AnimationManager.Instance.PlayOnce(false);
            OnHPChanged?.Invoke();
            if (HealthPoints > 0) return true;

            Death();
            return false;
        }

        public virtual int CalculateFinalDamage(int value)
        {
            return Mathf.RoundToInt((value - BuffManager.GetConstDefense()) *
                (1 - BuffManager.GetCofDefense()));
        }

        protected virtual void Death()
        {            
            IsDeath = true;
            OnDead?.Invoke();
        }

        protected virtual void OnSortingOrderChangedEvent(int sortingOrder)
        {
            EntitySprite.sortingOrder = sortingOrder;
        }
    }
}
