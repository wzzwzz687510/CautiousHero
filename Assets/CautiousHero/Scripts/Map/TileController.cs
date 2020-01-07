using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace Wing.RPGSystem
{
    public enum TileState
    {
        Normal,
        MoveZone,
        MoveSelected,
        CastZone,
        CastSelected
    }

    public enum TileType
    {
        Accessible = 0,
        Obstacle = 1,
        Entrance = 2,
        SpawnZone = 3, // 3x3
        Teleport = 4
    }

    [System.Serializable]
    public struct TileInfo
    {
        public int tTileHash;
        public ElementMana mana;
        public bool isObstacle;
        public bool isExplored;
        public bool isEmpty;
        public int stayEntityHash;
        public bool Blocked => isObstacle || !isEmpty;

        public TileInfo(int tTileHash)
        {
            this.tTileHash = tTileHash;
            TTile template = tTileHash.GetTTile();
            mana = template.mana;
            isObstacle = template.type == TileType.Obstacle;
            isExplored = false;
            isEmpty = true;
            stayEntityHash = 0;
        }
        public TileInfo(TTile template)
        {
            this.tTileHash = template.Hash;
            mana = template.mana;
            isObstacle = template.type == TileType.Obstacle;
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

        public void SetTemplate(TTile template)
        {
            this.tTileHash = template.Hash;
            mana = template.mana;
            isObstacle = template.type == TileType.Obstacle;
        }
    }

    public class TileController : MonoBehaviour
    {
        [Header("Components")]
        public SpriteRenderer m_bSpriteRenderer;
        public SpriteRenderer m_fSpriteRenderer;
        public SpriteRenderer m_cover;
        public Animator m_animator;
        public Transform m_archor;

        [Header("Colours")]
        public Color moveColor = new Color(0.36f, 1.0f, 0.36f);
        public Color unreachableColor = new Color(1.0f, 0.2f, 0.0f);
        public Color castColor = new Color(1.0f, 0.3f, 0.0f);
        
        public Vector3 Archor { get { return m_archor.position; } }
        public TileInfo Info => AreaManager.Instance.TempData.map[Loc.x,Loc.y];
        
        public Location Loc { get; private set; }
        public Location CastLoc { get; private set; }
        public int StayEntityHash { get { return Info.stayEntityHash; } }
        public Entity StayEntity { get { return StayEntityHash.GetEntity(); } }
        public bool IsEmpty { get { return Info.isEmpty; } }
        public bool IsBind { get; private set; }

        public int SortOrder { get { return m_bSpriteRenderer.sortingOrder; } }

        // para sort order, sprite ID and animation delay time
        public void Init(Location location)
        {
            Loc = location;
            m_bSpriteRenderer.sortingOrder = Loc.x + Loc.y * 32 - 32 * 32;            
        }

        public void UpdateSprite()
        {
            m_bSpriteRenderer.sprite = Info.tTileHash.GetTTile().bSprite;
            m_fSpriteRenderer.sprite = Info.tTileHash.GetTTile().fSprite;
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
                case TileState.MoveZone:
                    SetCoverColor(moveColor.SetAlpha(0.3f));
                    break;
                case TileState.CastZone:
                    SetCoverColor(castColor.SetAlpha(0.3f));
                    SetStayEntityOutline(Color.black);
                    break;
                case TileState.MoveSelected:
                    if(!Info.Blocked) SetCoverColor(moveColor.SetAlpha(0.7f));
                    else SetCoverColor(unreachableColor.SetAlpha(0.7f));
                    break;
                case TileState.CastSelected:
                    SetCoverColor(castColor.SetAlpha(0.7f));
                    SetStayEntityOutline(Color.red);
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
            GridManager.Instance.Nav.SetTileWeight(Loc, 0);
        }

        public void OnEntityLeaving()
        {
            // Do something to entity;
            Info.ClearEntity();
            GridManager.Instance.Nav.SetTileWeight(Loc, 1);
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