using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace Wing.RPGSystem
{
    public enum TileState
    {
        Normal,
        PlaceZone,
        PlaceSelected,
        MoveZone,
        MoveSelected,
        CastZone,
        CastSelected
    }

    public enum TileType
    {
        Plain = 0,
        Forest = 1,
        Water = 2,
        Mountain = 3,
        Passage = 4
    }

    [System.Serializable]
    public struct TileInfo
    {
        public TileType type;
        public ElementMana mana;
        public Location dirPattern; // if not (0,0) means this is a passage
        public bool isExplored;
        public bool isEmpty;
        public int stayEntityHash;

        public TileInfo(TileType type, ElementMana mana)
        {
            this.type = type;
            this.mana = mana;
            dirPattern = new Location(0, 0);
            isExplored = false;
            isEmpty = true;
            stayEntityHash = 0;
        }

        public void SetEntity(int hash)
        {
            isEmpty = false;
            stayEntityHash = hash;
        }
        public void ClearEntity()
        {
            isEmpty = true;
            stayEntityHash = 0;
        }
        public void SetMana(ElementMana mana)
        {
            this.mana = mana;
        }
    }

    public class TileController : MonoBehaviour
    {
        public SpriteRenderer m_spriteRenderer;
        public SpriteRenderer m_cover;
        public Animator m_animator;
        public Transform m_archor;
        
        public Vector3 Archor { get { return m_archor.position; } }

        public TileInfo Info { get => _info; private set => _info = value; }
        private TileInfo _info;
        public Location Loc { get; private set; }
        public Location CastLoc { get; private set; }
        public int StayEntityHash { get { return Info.stayEntityHash; } }
        public Entity StayEntity { get { return StayEntityHash.GetEntity(); } }
        public bool IsEmpty { get { return Info.isEmpty; } }
        public bool IsBind { get; private set; }

        public int SortOrder { get { return m_spriteRenderer.sortingOrder; } }


        // para sort order, sprite ID and animation delay time
        public void Init_SpriteRenderer(Location location, int sortOrder, int spriteID, float AnimDelayTime)
        {
            Loc = location;
            m_spriteRenderer.sortingOrder = sortOrder;
            if (spriteID >= 0)
                m_spriteRenderer.sprite = GridManager.Instance.tileSprites[spriteID];
            else {
                m_spriteRenderer.sprite = GridManager.Instance.darkArea[GridManager.Instance.darkArea.Length.Random()];
            }

            StartCoroutine(PlayAnimation(AnimDelayTime));
        }

        IEnumerator PlayAnimation(float delayTime)
        {
            yield return new WaitForSeconds(delayTime);
            m_animator.Play("tile_fall");
        }

        public void ChangeTileState(TileState state)
        {
            switch (state) {
                case TileState.Normal:
                    SetCoverColor(new Color(0, 0, 0, 0));
                    SetStayEntityOutline(Color.black);
                    break;
                case TileState.PlaceZone:
                    SetCoverColor(new Color(1.0f, 1.0f, 0.0f, 0.3f));
                    break;
                case TileState.MoveZone:
                    SetCoverColor(new Color(0.36f, 1.0f, 0.36f, 0.3f));
                    break;
                case TileState.CastZone:
                    SetCoverColor(new Color(1.0f, 0.3f, 0.0f, 0.3f));
                    SetStayEntityOutline(Color.black);
                    break;
                case TileState.MoveSelected:
                    SetCoverColor(new Color(0.36f, 1.0f, 0.36f, 0.7f));
                    break;
                case TileState.CastSelected:
                    SetCoverColor(new Color(1.0f, 0.1f, 0.1f, 0.7f));
                    SetStayEntityOutline(Color.red);
                    break;
                case TileState.PlaceSelected:
                    SetCoverColor(new Color(1.0f, 1.0f, 0.0f, 0.7f));
                    break;
                default:
                    break;
            }
        }

        public void SetStayEntityOutline(Color c)
        {
            if (!IsEmpty) {
                StayEntity.ChangeOutlineColor(c);
            }

        }

        public void EntityPassBy(Entity entity)
        {
            //Do something to entity;
        }

        public void OnEntityEntering(int hash)
        {
            // Do something to entity;
            Info.SetEntity(hash);
            GridManager.Instance.Astar.SetTileWeight(Loc, 0);
        }

        public void OnEntityLeaving()
        {
            // Do something to entity;
            Info.ClearEntity();
            GridManager.Instance.Astar.SetTileWeight(Loc, 1);
        }

        public void BindCastLocation(Location from)
        {
            CastLoc = from;
            IsBind = true;
        }

        public void DebindCastLocation()
        {
            IsBind = false;
        }

        private void SetCoverColor(Color c)
        {
            m_cover.DOColor(c, 0.2f);
        }

    }
}