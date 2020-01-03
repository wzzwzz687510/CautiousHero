using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wing.RPGSystem
{
    [System.Serializable]
    public struct PreAreaInfo
    {
        public Location loc;
        public int typeID;
        public List<Location> connectionDP;// Connection direction pattern
    }

    [System.Serializable]
    public struct AreaInfo
    {
        public int templateHash;
        public Location loc;
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
        public void Init_SpriteRenderer(Location location)
        {
            Loc = location;
            m_spriteRenderer.sortingOrder = Loc.x + Loc.y * 8;

                m_spriteRenderer.sprite = AreaInfo.templateHash.GetAreaConfig().sprite;

            StartCoroutine(PlayAnimation(Random.Range(0.01f, 1f)));
        }

        IEnumerator PlayAnimation(float delayTime)
        {
            yield return new WaitForSeconds(delayTime);
            m_animator.Play("tile_fall");
        }
    }
}