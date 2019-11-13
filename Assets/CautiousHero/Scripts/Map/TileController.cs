using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wing.TileUtils;
using DG.Tweening;
using Wing.RPGSystem;

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

public class TileController : MonoBehaviour
{
    public SpriteRenderer m_spriteRenderer;
    public SpriteRenderer m_cover;
    public Animator m_animator;
    public Transform m_archor;    

    public Vector3 Archor { get { return m_archor.position; } }
    public Location Loc { get; private set; }
    public Entity stayEntity { get; private set; }
    public bool isEmpty { get { return stayEntity == null; } }


    // para sort order, sprite ID and animation delay time
    public void Init_SpriteRenderer(Location location,int sortOrder, int spriteID,float AnimDelayTime)
    {
        Loc = location;
        m_spriteRenderer.sortingOrder = sortOrder;
        m_spriteRenderer.sprite = GridManager.Instance.tileSprites[spriteID];

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
                break;
            case TileState.PlaceZone:
                SetCoverColor(new Color(1.0f, 1.0f, 0.0f, 0.3f));
                break;
            case TileState.MoveZone:
                SetCoverColor(new Color(0.36f, 1.0f, 0.36f, 0.3f));
                break;
            case TileState.CastZone:
                SetCoverColor(new Color(1.0f, 0.3f, 0.0f, 0.3f));
                break;
            case TileState.MoveSelected:
                SetCoverColor(new Color(0.36f, 1.0f, 0.36f, 0.7f));
                break;
            case TileState.CastSelected:
                SetCoverColor(new Color(1.0f, 0.1f, 0.1f, 0.7f));
                break;
            case TileState.PlaceSelected:
                SetCoverColor(new Color(1.0f, 1.0f, 0.0f, 0.7f));
                break;
            default:
                break;
        }
    }

    public void PassBy(Entity entity)
    {
        //Do something to entity;
    }

    public void OnEntityEntering(Entity entity)
    {
        // Do something to entity;

        stayEntity = entity;
    }

    public void OnEntityLeaving()
    {
        // Do something to entity;

        stayEntity = null;
    }

    private void SetCoverColor(Color c)
    {
        m_cover.DOColor(c, 0.2f);
    }
}
