using DG.Tweening;
using SpriteGlow;
using System.Collections.Generic;
using UnityEngine;
using Wing.TileUtils;

namespace Wing.RPGSystem
{
    public class InstanceSkill
    {
        public BaseSkill TSkill { get; private set; }
        public int Cooldown { get; private set; }
        public bool Castable { get { return Cooldown == 0; } }

        public InstanceSkill(BaseSkill skill)
        {
            TSkill = skill;
            Cooldown = 0;
        }

        public void SetCooldown(int num)
        {
            Cooldown = num;
        }
    }

    public class Entity : MonoBehaviour
    {
        public int HealthPoints { get; protected set; }
        public int ActionPoints { get; protected set; }
        public BaseSkill[] Skills { get; protected set; }
        public InstanceSkill[] ActiveSkills { get; protected set; }

        
        protected EntityAttribute m_attribute;
        public EntityAttribute Attribute {
            get {
                return m_attribute + buffManager.GetAttributeAdjustment();
            }
        }
        public int Level { get { return Attribute.level; } }
        public int MaxHealthPoints { get { return Attribute.maxHealth; } }
        public int MaxActionPoints { get { return Attribute.maxAction; } }
        public int Strength { get { return Attribute.strength; } }
        public int Intelligence { get { return Attribute.intelligence; } }
        public int Agility { get { return Attribute.agility; } }
        public int MoveCost { get { return Attribute.moveCost; } }

        protected BuffManager buffManager = new BuffManager();

        public Location Loc { get { return locateTile.Loc; } }
        public TileController LocateTile { get { return locateTile; } }
        public SpriteRenderer Sprite { get { return m_sprite; } }

        protected SpriteRenderer m_sprite;
        protected SpriteGlowEffect m_glowEffect;
        protected BoxCollider2D m_collider;
        protected TileController locateTile;
        protected List<Location[]> paths = new List<Location[]>();

        public delegate void ValueChange(int value);
        public event ValueChange OnHpChanged;

        protected virtual void Awake()
        {
            m_sprite = GetComponentInChildren<SpriteRenderer>();
            m_glowEffect = GetComponentInChildren<SpriteGlowEffect>();
            m_collider = GetComponentInChildren<BoxCollider2D>();
        }

        public virtual void MoveToTile(TileController targetTile, bool anim = true)
        {
            if (targetTile == locateTile)
                return;

            if (anim) {
                Stack<Location> path = GridManager.Instance.Astar.GetPath(Loc, targetTile.Loc);

                if (path.Count * MoveCost > ActionPoints)
                    return;

                Location[] sortedPath = new Location[path.Count];
                for (int i = 0; i < sortedPath.Length; i++) {
                    sortedPath[i] = path.Pop();
                }
                paths.Add(sortedPath);
                MoveAnimation();
            }
            else {
                transform.position = targetTile.transform.position;
            }
            if (locateTile) {
                locateTile.OnEntityLeaving();
            }

            m_sprite.sortingOrder = targetTile.Loc.x + targetTile.Loc.y * 8;
            locateTile = targetTile;
            targetTile.OnEntityEntering(this);
        }

        public void CastSkill(int skillID, Location castLoc)
        {
            var tSkill = Skills[skillID];
            ActionPoints -= tSkill.actionPointsCost;
            ActiveSkills[skillID].SetCooldown(tSkill.cooldownTime);

            tSkill.ApplyEffect(this, castLoc);
            //Location effectLoc;
            //foreach (var pattern in tSkill.effectPatterns) {
            //    effectLoc = castLoc + pattern.loc;
            //    if (!GridManager.Instance.IsEmptyLocation(effectLoc))
            //        tSkill.ApplyEffect(this, GridManager.Instance.GetTileController(effectLoc).stayEntity, pattern.coefficient);
            //}
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

        protected virtual void MoveAnimation()
        {
            int end = paths.Count - 1;
            Vector3[] points = new Vector3[paths[end].Length];
            for (int i = 0; i < points.Length; i++) {
                points[i] = paths[end][i];
            }

            transform.DOPath(points, 0.3f);
        }

        public virtual bool DealDamage(int value)
        {
            HealthPoints -= Mathf.RoundToInt((value - buffManager.GetReduceConstant(BuffType.Defend)) * 
                (1 - buffManager.GetReduceCof(BuffType.Defend)));
            OnHpChanged?.Invoke(HealthPoints);
            if (HealthPoints > 0)
                return true;

            Death();
            return false;
        }

        public virtual void Death()
        {

        }


    }
}
