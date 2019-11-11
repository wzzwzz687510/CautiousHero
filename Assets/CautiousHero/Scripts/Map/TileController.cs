using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wing.TileUtils;

public class TileController : MonoBehaviour
{
    public SpriteRenderer m_spriteRenderer;
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


}
