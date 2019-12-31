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
        public int maxAction;
        public int actionPerTurn;
        public int strength;
        public int intelligence;
        public int agility;
        public int moveCost;

        public EntityAttribute(int maxHp, int maxAct, int act, int str, int inte, int agi, int mvCost)
        {
            maxHealth = maxHp;
            maxAction = maxAct;
            actionPerTurn = act;
            strength = str;
            intelligence = inte;
            agility = agi;
            moveCost = mvCost;
        }

        public static EntityAttribute operator -(EntityAttribute a) =>
            new EntityAttribute(-a.maxHealth, -a.maxAction, -a.actionPerTurn, -a.strength, -a.intelligence, -a.agility, -a.moveCost);
        public static EntityAttribute operator +(EntityAttribute a, EntityAttribute b) =>
            new EntityAttribute(a.maxHealth + b.maxHealth, a.maxAction + b.maxAction, a.actionPerTurn + b.actionPerTurn,
                a.strength + b.strength, a.intelligence + b.intelligence, a.agility + b.agility, a.moveCost + b.moveCost);
        public static EntityAttribute operator -(EntityAttribute a, EntityAttribute b) => a + -(b);
    }

    [System.Serializable]
    public struct ElementResistance
    {
        public int fire;
        public int water;
        public int earth;
        public int air;
        public int light;
        public int dark;

        public ElementResistance(int fire = 0, int water = 0, int earth = 0, int air = 0, int light = 0, int dark = 0)
        {
            this.fire = fire;
            this.water = water;
            this.earth = earth;
            this.air = air;
            this.light = light;
            this.dark = dark;
        }

        public static ElementResistance operator -(ElementResistance a) =>
            new ElementResistance(-a.fire, -a.water, -a.earth, -a.air, -a.light, -a.dark);
        public static ElementResistance operator +(ElementResistance a, ElementResistance b) =>
            new ElementResistance(a.fire + b.fire, a.water + b.water, a.earth + b.earth, 
                a.air + b.air, a.light + b.light, a.dark + b.dark);
        public static ElementResistance operator -(ElementResistance a, ElementResistance b) => a + (-b);
    }

    // Obsolete
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
        public int PhysicalArmourPoints { get; protected set; }
        public int MagicalArmourPoints { get; protected set; }
        public int ActionPoints { get; protected set; }
        public bool IsDeath { get; protected set; }
        public List<int> SkillHashes { get; protected set; }
        public BuffManager BuffManager { get; protected set; }

        public Location Loc { get; protected set; }
        public Vector3[] MovePath { get; protected set; }

        protected EntityAttribute m_attribute;
        public EntityAttribute Attribute {
            get {
                return m_attribute + BuffManager.GetAttributeAdjustment();
            }
        }
        public int MaxHealthPoints { get { return Attribute.maxHealth; } }
        public int MaxActionPoints { get { return Attribute.maxAction; } }
        public int ActionPointsPerTurn { get { return Attribute.actionPerTurn; } }
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
        public delegate void ArmourChange(bool isPhysical,bool show);
        public ArmourChange ArmourPointsChangeAnimation;

        [HideInInspector] public UnityEvent OnTurnStartedEvent;
        [HideInInspector] public UnityEvent OnTurnEndedEvent;
        [HideInInspector] public UnityEvent OnHPChanged;
        [HideInInspector] public UnityEvent OnPhysicalAPChanged;
        [HideInInspector] public UnityEvent OnMagicalAPChanged;
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
            SkillHashes = new List<int>();
            OnSortingOrderChanged += OnSortingOrderChangedEvent;
        }

        public virtual void OnTurnStarted()
        {
            OnTurnStartedEvent?.Invoke();
            BuffManager.UpdateBuffs();

            ImpactActionPoints(ActionPointsPerTurn,false);
        }

        public virtual void OnTurnEnded()
        {
            OnTurnEndedEvent?.Invoke();
        }

        /// <summary>
        /// set anim false generally means move entity to one place instantly without move cost (e.g. by skill effect).
        /// </summary>
        /// <param name="targetLoc"></param>
        /// <param name="isInstance"></param>
        public virtual void MoveToTile(Location targetLoc, bool isInstance = false)
        {
            if (targetLoc == Loc) {
                return;
            }

            if (!isInstance) {
                Stack<Location> path = GridManager.Instance.Astar.GetPath(Loc, targetLoc);

                if (path.Count * MoveCost > ActionPoints) {

                    return;
                }

                Vector3[] sortedPath = new Vector3[path.Count];
                for (int i = 0; i < sortedPath.Length; i++) {
                    sortedPath[i] = path.Pop();
                }
                MovePath = sortedPath;

                // AP cost and invoke event
                ImpactActionPoints(sortedPath.Length * MoveCost,true);
                // Animation move
            }

            if (Loc.TryGetTileController(out TileController leaveTile)) {
                leaveTile.OnEntityLeaving();
            }
            Loc = targetLoc;
            Loc.GetTileController().OnEntityEntering(Hash);

            if (isInstance) {
                AnimationManager.Instance.AddAnimClip(new MoveInstantAnimClip(Hash, targetLoc, 0.2f));
                AnimationManager.Instance.PlayOnce();
            }
            else {

                AnimationManager.Instance.AddAnimClip(new MovePathAnimClip(Hash, MovePath, 0.2f));
            }
        }

        public virtual void CastSkill(int skillID, Location castLoc)
        {
            BaseSkill tSkill = SkillHashes[skillID].GetBaseSkill();
            if (ActionPoints < tSkill.actionPointsCost)
                return;
            ImpactActionPoints(tSkill.actionPointsCost,true);

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

        public virtual bool DealDamage(int value, DamageType damageType)
        {
            int damage = value;

            damage -= ImpactArmour(damage, damageType == DamageType.Physical,true);
            if (damage == 0) return true;

            return ImpactHP(damage,true);
        }

        public virtual void ImpactActionPoints(int value, bool reduce)
        {
            ActionPoints += (reduce ? -1 : 1) * value;
            ActionPoints = Mathf.Clamp(ActionPoints, 0, MaxActionPoints);
            OnAPChanged?.Invoke();
        }

        /// <summary>
        /// Damage armour
        /// </summary>
        /// <param name="value"></param>
        /// <param name="isPhysical"></param>
        /// <returns>Absorbed damage</returns>
        public virtual int ImpactArmour(int value, bool isPhysical, bool damage)
        {
            int absorbDamage = isPhysical ? PhysicalArmourPoints : MagicalArmourPoints;
            if (absorbDamage > value)
                absorbDamage = value;

            if (isPhysical) PhysicalArmourPoints = Mathf.Max(0, PhysicalArmourPoints + (damage ? -1 : 1) * value);
            else MagicalArmourPoints = Mathf.Max(0, MagicalArmourPoints + (damage ? -1 : 1) * value);
            AnimationManager.Instance.AddAnimClip(new ArmourPChangeAnimClip(Hash, isPhysical));
            if (BattleManager.Instance.IsPlayerTurn)
                AnimationManager.Instance.PlayOnce(false);

            if (isPhysical) OnPhysicalAPChanged?.Invoke();
            else OnMagicalAPChanged?.Invoke();

            return absorbDamage;
        }

        /// <summary>
        /// HP -= adjusted value. Should calculate adjusted damage in function so that the direct damage to hp can be correct, such as Piercing Attack;
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual bool ImpactHP(int value, bool damage)
        {
            //Debug.Log("Entity: "+EntityName+", HP: " + HealthPoints);
            int tmpHP = HealthPoints;
            HealthPoints = Mathf.Clamp(HealthPoints + (damage ? -1 : 1) * value, 0, MaxHealthPoints);
            AnimationManager.Instance.AddAnimClip(new HPChangeAnimClip(Hash, 1.0f * HealthPoints / MaxHealthPoints));
            if (BattleManager.Instance.IsPlayerTurn)
                AnimationManager.Instance.PlayOnce(false);

            OnHPChanged?.Invoke();
            if (HealthPoints > 0) return true;

            Death();
            return false;
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
