using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileController : MonoBehaviour
{
    public SpriteRenderer m_spriteRenderer;
    public Animator m_animator;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // para sort order, sprite ID and animation delay time
    public void Init_SpriteRenderer(int sortOrder, int spriteID,float AnimDelayTime)
    {
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
