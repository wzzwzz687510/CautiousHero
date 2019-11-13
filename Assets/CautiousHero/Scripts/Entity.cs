using DG.Tweening;
using SpriteGlow;
using System.Collections.Generic;
using UnityEngine;
using Wing.TileUtils;

namespace Wing.RPGSystem
{
    public class Entity : MonoBehaviour
    {
        protected int m_healthPoints;
        protected int m_manaPoints;

        protected EntityAttribute m_attribute;
        public EntityAttribute Attribute {
            get {
                var tmp = m_attribute;
                foreach (var buff in buffs) {
                    if (buff.ScriptableBuff.buffType == BuffType.AttributeAdjust)
                        tmp += (buff.ScriptableBuff as AttributeBuff).adjustValue;
                }
                return tmp;
            }
        }
        public int Level { get { return Attribute.level; } }
        public int MaxHealthPoints { get { return Attribute.maxHealth; } }
        public int MaxManaPoints { get { return Attribute.maxMana; } }
        public int MaxMovementPoints { get { return Attribute.maxMovement; } }
        public int Strength { get { return Attribute.strength; } }
        public int Intelligence { get { return Attribute.intelligence; } }
        public int Agility { get { return Attribute.agility; } }

        protected List<BuffHandler> buffs = new List<BuffHandler>();

        public Location Loc { get { return locateTile.Loc; } }
        public SpriteRenderer Sprite { get { return m_sprite; } }

        public int MovementPoint { get; protected set; }

        protected SpriteRenderer m_sprite;
        protected SpriteGlowEffect m_glowEffect;
        protected BoxCollider2D m_collider;
        protected TileController locateTile;
        protected List<Location[]> paths = new List<Location[]>();

        protected virtual void Awake()
        {
            m_sprite = GetComponentInChildren<SpriteRenderer>();
            m_glowEffect = GetComponentInChildren<SpriteGlowEffect>();
            m_collider = GetComponentInChildren<BoxCollider2D>();
        }

        public virtual void MoveToTile(TileController targetTile, Stack<Location> path, bool anim = false)
        {
            if (anim) {
                if (path.Count > MovementPoint)
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
    }
}
