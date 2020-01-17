using DG.Tweening;
using SpriteGlow;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        public Resistance resistance;

        public EntityAttribute(int maxHp, int maxAct, int act, int str, int inte, int agi, int mvCost, Resistance rst)
        {
            maxHealth = maxHp;
            maxAction = maxAct;
            actionPerTurn = act;
            strength = str;
            intelligence = inte;
            agility = agi;
            moveCost = mvCost;
            resistance = rst;
        }

        public static EntityAttribute operator -(EntityAttribute a) =>
            new EntityAttribute(-a.maxHealth, -a.maxAction, -a.actionPerTurn, -a.strength, -a.intelligence, -a.agility, -a.moveCost,-a.resistance);
        public static EntityAttribute operator +(EntityAttribute a, EntityAttribute b) =>
            new EntityAttribute(a.maxHealth + b.maxHealth, a.maxAction + b.maxAction, a.actionPerTurn + b.actionPerTurn,
                a.strength + b.strength, a.intelligence + b.intelligence, a.agility + b.agility, a.moveCost + b.moveCost, a.resistance + b.resistance);
        public static EntityAttribute operator -(EntityAttribute a, EntityAttribute b) => a + -(b);
        public static EntityAttribute operator *(EntityAttribute a, float scale) =>
            new EntityAttribute((int)(a.maxHealth * scale), (int)(a.maxAction * scale), (int)(a.actionPerTurn * scale), (int)(a.strength * scale), 
                (int)(a.intelligence * scale), (int)(a.agility * scale), (int)(a.moveCost * scale), (a.resistance * scale));
        public static EntityAttribute operator /(EntityAttribute a, float scale) => a * (1 / scale);
    }

    [System.Serializable]
    public struct Resistance
    {
        public int physcialResistance;
        public MagicalElement elementResistance;
        public int Fire { get { return elementResistance.fire; } }
        public int Water { get { return elementResistance.water; } }
        public int Earth { get { return elementResistance.earth; } }
        public int Air { get { return elementResistance.air; } }
        public int Light { get { return elementResistance.light; } }
        public int Dark { get { return elementResistance.dark; } }

        public Resistance(int pResist,MagicalElement me)
        {
            physcialResistance = pResist;
            elementResistance = me;
        }

        public Resistance(int pResist, int fire = 0, int water = 0, int earth = 0, int air = 0, int light = 0, int dark = 0)
        {
            physcialResistance = pResist;
            elementResistance.fire = fire;
            elementResistance.water = water;
            elementResistance.earth = earth;
            elementResistance.air = air;
            elementResistance.light = light;
            elementResistance.dark = dark;
        }

        public static Resistance operator -(Resistance a) => new Resistance(-a.physcialResistance, -a.elementResistance);
        public static Resistance operator +(Resistance a, Resistance b)
            => new Resistance(a.physcialResistance + b.physcialResistance, a.elementResistance + b.elementResistance);
        public static Resistance operator -(Resistance a, Resistance b) => a + (-b);
        public static Resistance operator *(Resistance a, float scale) => new Resistance(-a.physcialResistance, a.elementResistance * scale);
        public static Resistance operator /(Resistance a, float scale) => a * (1 / scale);
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

        public static ElementMana operator -(ElementMana a) => new ElementMana(-a.mana);
        public static ElementMana operator +(ElementMana a, ElementMana b) => new ElementMana(a.mana + b.mana);
        public static ElementMana operator -(ElementMana a, ElementMana b) => a + (-b);
        public static ElementMana operator *(ElementMana a, float scale) => new ElementMana(a.mana * 2);
        public static ElementMana operator /(ElementMana a, float scale) => a * (1 / scale);
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

        public static MagicalElement operator -(MagicalElement a) =>
            new MagicalElement(-a.fire, -a.water, -a.earth, -a.air, -a.light, -a.dark);
        public static MagicalElement operator +(MagicalElement a, MagicalElement b) =>
            new MagicalElement(a.fire + b.fire, a.water + b.water, a.earth + b.earth,
                a.air + b.air, a.light + b.light, a.dark + b.dark);
        public static MagicalElement operator *(MagicalElement a, float scale) 
            => new MagicalElement((int)(a.fire*scale), (int)(a.water * scale), (int)(a.earth * scale), 
                (int)(a.air * scale), (int)(a.light * scale), (int)(a.dark * scale));
        public static MagicalElement operator /(MagicalElement a, float scale) => a * (1 / scale);
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
        public bool IsCostAP { get; private set; }
        public bool IsDeath { get; protected set; }
        public List<int> SkillHashes { get; protected set; }
        public BuffManager EntityBuffManager { get; protected set; }

        public Location Loc { get; protected set; }
        public Location[] MovePath { get; protected set; }

        protected EntityAttribute m_attribute;
        protected EntityAttribute tempAttribute;
        public EntityAttribute Attribute {
            get {
                return m_attribute + EntityBuffManager.GetAttributeAdjustment() + tempAttribute;
            }
        }
        public int MaxHealthPoints       => Attribute.maxHealth;
        public int MaxActionPoints       => Attribute.maxAction; 
        public int ActionPointsPerTurn   => Attribute.actionPerTurn;
        public int Strength              => Attribute.strength;
        public int Intelligence          => Attribute.intelligence; 
        public int Agility               => Attribute.agility; 
        public int MoveCost              => Attribute.moveCost;
        public Resistance Resistance     => Attribute.resistance;

        public SpriteRenderer EntitySprite => m_spriteRenderer; 

        protected SpriteRenderer m_spriteRenderer;
        protected SpriteGlowEffect m_glowEffect;
        protected BoxCollider2D m_collider;

        public delegate void IntDelegate(int value);
        public IntDelegate OnMovedEvent;
        public IntDelegate OnSortingOrderChanged;
        public delegate void PointsChange(int hp,int maxHP, float duraion);
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
            IsCostAP = true;
            IsDeath = false;
            SkillHashes = new List<int>();
            OnSortingOrderChanged += OnSortingOrderChangedEvent;
        }

        public virtual void OnTurnStarted()
        {
            OnTurnStartedEvent?.Invoke();
            EntityBuffManager.UpdateBuffs();
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
        public virtual int MoveToTile(Location targetLoc,int moveCost, bool isInstance = false)
        {
            if (targetLoc == Loc) {
                return 0;
            }
            // Calculate movement path
            if (!isInstance) {
                Stack<Location> path = GridManager.Instance.Nav.GetPath(Loc, targetLoc);

                if (path.Count * moveCost > ActionPoints) {
                    return 0;
                }

                MovePath = path.ToArray();
            }

            if (isInstance) {
                Loc.GetTileController().OnEntityLeaving();
                Loc = targetLoc;
                Loc.GetTileController().OnEntityEntering(Hash);
                AnimationManager.Instance.AddAnimClip(new MoveInstantAnimClip(Hash, targetLoc, 0.2f));
                AnimationManager.Instance.PlayOnce();
            }
            else {
                int stepID = 0, passedCount = 0;
                for (int i = 0; i < MovePath.Length; i++) {
                    if (MovePath[i].GetTileController().HasImpacts || i == MovePath.Length - 1) {
                        if (stepID * moveCost > ActionPoints) {
                            OnMovedEvent?.Invoke(i);
                            return i;
                        }
                        SubMoveToTile(passedCount, stepID);

                        stepID = 0;
                        passedCount = i + 1;
                        continue;
                    }
                    stepID++;
                }
                // AP cost
                ImpactActionPoints(passedCount * moveCost, true);
            }
            int movementSteps = isInstance ? 0 : MovePath.Length;
            // Pass move condition
            OnMovedEvent?.Invoke(movementSteps);
            return movementSteps;
        }

        public virtual void SubMoveToTile(int passedCount,int stepID)
        {
            //Debug.Log("pass: " + passedCount + ", step: " + stepID);
            Location[] subPath = MovePath.Skip(passedCount).Take(stepID+1).ToArray();
            //Debug.Log("mov: " + MovePath.Length + ", sub: " + subPath.Length);

            Loc.GetTileController().OnEntityLeaving();
            Loc = subPath[stepID];
            Loc.GetTileController().OnEntityEntering(Hash);

            AnimationManager.Instance.AddAnimClip(new MovePathAnimClip(Hash, subPath, 0.2f));
        }

        public virtual bool CastSkill(int skillID, Location castLoc)
        {
            BaseSkill tSkill = SkillHashes[skillID].GetBaseSkill();
            if (ActionPoints < tSkill.actionPointsCost)
                return false;
            ImpactActionPoints(tSkill.actionPointsCost, true);

            tSkill.ApplyEffect(Hash, Loc, castLoc, true);
            tempAttribute = new EntityAttribute();
            return true;
        }

        public virtual void SetTempAttribute(EntityAttribute temp)
        {
            tempAttribute = temp;
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

        public virtual void SetVisual(bool isVisible)
        {
            gameObject.SetActive(isVisible);
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
            if (EntityBuffManager.CheckIsInvincible()) return true;
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
            if(!IsCostAP) {
                IsCostAP = true;
                return;
            }
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
            if (value == 0) return 0;
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
            if (value == 0) return true;
            //Debug.Log("Entity: "+EntityName+", HP: " + HealthPoints);
            int tmpHP = HealthPoints;
            HealthPoints = Mathf.Clamp(HealthPoints + (damage ? -1 : 1) * value, 0, MaxHealthPoints);
            AnimationManager.Instance.AddAnimClip(new HPChangeAnimClip(Hash, HealthPoints,MaxHealthPoints));
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
