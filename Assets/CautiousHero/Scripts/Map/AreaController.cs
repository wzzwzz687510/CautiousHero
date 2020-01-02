using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wing.RPGSystem
{
    [System.Serializable]
    public struct AreaInfo
    {
        public bool isInit;
        public int typeID;
        public Location loc;
        public List<Location> connectionDP; // first location is direction pattern

        // Generate map after area generation
        public TileInfo[,] map;
    }

    public class AreaController : MonoBehaviour
    {
        public SpriteRenderer m_spriteRenderer;
        public SpriteRenderer m_cover;
        public Animator m_animator;
        public Transform m_archor;
        public Vector3 Archor { get { return m_archor.position; } }

        public bool IsInit { get; private set; }
        public Location Loc { get; private set; }
        public AreaInfo AreaInfo { get { Database.Instance.TryGetAreaInfo(Loc, out AreaInfo info); return info; } }

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
    }
}