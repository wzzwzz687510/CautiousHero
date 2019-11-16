using DG.Tweening;
using SpriteGlow;
using System.Collections.Generic;
using UnityEngine;
using Wing.TileUtils;

namespace Wing.RPGSystem
{
    public class Entity : MonoBehaviour
    {
        public int m_healthPoints { get; protected set; }
        public int ActionPoints { get; protected set; }
        public BaseSkill[] skills { get; protected set; }

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

        public virtual void MoveToTile(TileController targetTile, Stack<Location> path, bool anim = false)
        {
            if (anim) {
                /***************************************************************************
                 * improve by stopping at the nearest tile to the target tile.
                 ***************************************************************************/
                if (path.Count > ActionPoints)
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
            if (locateTile)
                locateTile.OnEntityLeaving();

            m_sprite.sortingOrder = targetTile.Loc.x + targetTile.Loc.y * 8;
            locateTile = targetTile;
            targetTile.OnEntityEntering(this);
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
            m_healthPoints -= Mathf.RoundToInt((value - buffManager.GetReduceConstant(BuffType.Defend)) * 
                (1 - buffManager.GetReduceCof(BuffType.Defend)));
            OnHpChanged?.Invoke(m_healthPoints);
            if (m_healthPoints > 0)
                return true;

            Death();
            return false;
        }

        public virtual void Death()
        {

        }
    }
}
