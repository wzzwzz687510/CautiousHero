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
        public MagicalElement resist;
        public int Fire { get { return resist.fire; } }
        public int Water { get { return resist.water; } }
        public int Earth { get { return resist.earth; } }
        public int Air { get { return resist.air; } }
        public int Light { get { return resist.light; } }
        public int Dark { get { return resist.dark; } }

        public ElementResistance(MagicalElement me)
        {
            resist = me;
        }

        public ElementResistance(int fire = 0, int water = 0, int earth = 0, int air = 0, int light = 0, int dark = 0)
        {
            resist.fire = fire;
            resist.water = water;
            resist.earth = earth;
            resist.air = air;
            resist.light = light;
            resist.dark = dark;
        }

        public static ElementResistance operator -(ElementResistance a) =>
            new ElementResistance(-a.Fire, -a.Water, -a.Earth, -a.Air, -a.Light, -a.Dark);
        public static ElementResistance operator +(ElementResistance a, ElementResistance b) =>
            new ElementResistance(a.Fire + b.Fire, a.Water + b.Water, a.Earth + b.Earth, 
                a.Air + b.Air, a.Light + b.Light, a.Dark + b.Dark);
        public static ElementResistance operator -(ElementResistance a, ElementResistance b) => a + (-b);
    }

    [System.Serializable]
    public struct ElementMana
    {
        public MagicalElement mana;
        public int Fire { get { return mana.fire; } }
        public int Water { get { return mana.water; } }
        public int Earth { get { return mana.earth; } }
        public int Air { get { return mana.air; } }
        public int Light { get { return mana.light; } }
        public int Dark { get { return mana.dark; } }

        public ElementMana(MagicalElement me)
        {
            mana = me;
        }

        public ElementMana(int fire = 0, int water = 0, int earth = 0, int air = 0, int light = 0, int dark = 0)
        {
            mana.fire = fire;
            mana.water = water;
            mana.earth = earth;
            mana.air = air;
            mana.light = light;
            mana.dark = dark;
        }

        public static ElementMana operator -(ElementMana a) =>
            new ElementMana(-a.Fire, -a.Water, -a.Earth, -a.Air, -a.Light, -a.Dark);
        public static ElementMana operator +(ElementMana a, ElementMana b) =>
            new ElementMana(a.Fire + b.Fire, a.Water + b.Water, a.Earth + b.Earth,
                a.Air + b.Air, a.Light + b.Light, a.Dark + b.Dark);
        public static ElementMana operator -(ElementMana a, ElementMana b) => a + (-b);
    }

    [System.Serializable]
    public struct MagicalElement
    {
        public int fire;
        public int water;
        public int earth;
        public int air;
        public int light;
        public int dark;

        public MagicalElement(int fire = 0, int water = 0, int earth = 0, int air = 0, int light = 0, int dark = 0)
        {
            this.fire = fire;
            this.water = water;
            this.earth = earth;
            this.air = air;
            this.light = light;
            this.dark = dark;
        }
    }

    // Obsolete
    public class InstanceSkill
    {
        public BaseSkill TSkill { get; private set; }
        public int Cooldown { get; private set; }
        public bool Castable => Cooldown <= 0;

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
        public int MaxHealthPoints       => Attribute.maxHealth;
        public int MaxActionPoints       => Attribute.maxAction; 
        public int ActionPointsPerTurn   => Attribute.actionPerTurn;
        public int Strength              => Attribute.strength;
        public int Intelligence          => Attribute.intelligence; 
        public int Agility               => Attribute.agility; 
        public int MoveCost              => Attribute.moveCost; 

        public SpriteRenderer EntitySprite => m_spriteRenderer; 

        protected SpriteRenderer m_spriteRenderer;
        protected SpriteGlowEffect m_glowEffect;
        protected BoxCollider2D m_collider;

        public delegate void SortingOrderChange(int sortingOrder);
        public SortingOrderChange OnSortingOrderChanged;
        public delegate void PointsChange(float ratio, float duraion);
        public PointsChange HPChangeAnimation;
        public delegate void ArmourChange(bool isPhysical,int remainedNumber);
        public ArmourChange ArmourPointsChangeAnimation;

        [HideInInspector] public UnityEvent OnTurnStartedEvent;
        [HideInInspector] public UnityEvent OnTurnEndedEvent;
        [HideInInspector] public UnityEvent OnHPChanged;
        [HideInInspector] public UnityEvent OnPhysicalAPChanged;
        [HideInInspector] public UnityEvent OnMagicalAPChanged;
        [HideInInspector] public UnityEvent OnCancelArmourEvent;
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
            CancelArmour();
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
        public virtual int MoveToTile(Location targetLoc, bool isInstance = false)
        {
            if (targetLoc == Loc) {
                return 0;
            }

            if (!isInstance) {
                Stack<Location> path = GridManager.Instance.Astar.GetPath(Loc, targetLoc);

                if (path.Count * MoveCost > ActionPoints) {
                    return 0;
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

            return isInstance ? 1 : MovePath.Length;
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

        public virtual void CancelArmour()
        {
            PhysicalArmourPoints = 0;
            MagicalArmourPoints = 0;
            OnCancelArmourEvent?.Invoke();
        }

        public virtual bool DealDamage(int value, DamageType damageType)
        {
            int damage = value;

            if(damageType == DamageType.Physical && PhysicalArmourPoints!=0) {
                damage -= ImpactArmour(damage, true, true);
            }
            else if(damageType == DamageType.Magical && MagicalArmourPoints != 0) {
                damage -= ImpactArmour(damage, false, true);
            }
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
            AnimationManager.Instance.AddAnimClip(
                new ArmourPChangeAnimClip(Hash, isPhysical, isPhysical? PhysicalArmourPoints : MagicalArmourPoints));
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
