using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wing.TileUtils;
using DG.Tweening;

public enum TileState
{
    Normal,
    PlaceZone,
    PlaceSelected,
    MoveZone,
    MoveSelected,
    AttackZone,    
    AttackSelected
}

public class TileController : MonoBehaviour
{
    public SpriteRenderer m_spriteRenderer;
    public SpriteRenderer m_cover;
    public Animator m_animator;
    public Transform m_archor;    
    public Vector3 Archor { get { return m_archor.position; } }

    public Location Loc { get; private set; }


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
                SetCoverColor(new Color(1.0f, 1.0f, 0.0f, 0.36f));
                break;
            case TileState.MoveZone:
                SetCoverColor(new Color(0.36f, 1.0f, 0.36f, 0.36f));
                break;
            case TileState.AttackZone:
                SetCoverColor(new Color(1.0f, 0.3f, 0.0f, 0.36f));
                break;
            case TileState.MoveSelected:
                SetCoverColor(new Color(0.36f, 1.0f, 0.36f, 0.5f));
                break;
            case TileState.AttackSelected:
                SetCoverColor(new Color(1.0f, 0.3f, 0.0f, 0.5f));
                break;
            case TileState.PlaceSelected:
                SetCoverColor(new Color(1.0f, 1.0f, 0.0f, 0.5f));
                break;
            default:
                break;
        }
    }

    private void SetCoverColor(Color c)
    {
        m_cover.DOColor(c, 0.2f);
    }
}
