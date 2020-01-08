using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wing.RPGSystem
{
    [System.Serializable]
    public struct PreAreaInfo
    {
        public Location loc;
        public bool isHardSet;
        public int typeID;
        public List<Location> connectionDP;// Connection direction pattern
    }

    [System.Serializable]
    public struct ChestEntity
    {
        public Location loc;
        public int coin;
        public List<int> relicHashes;

        public ChestEntity(Location loc, BaseChest template)
        {
            this.loc = loc;
            coin = template.coins;
            relicHashes = new List<int>();
            foreach (var relic in template.relics) {
                relicHashes.Add(relic.Hash);
            }
        }

        public void RemoveCoin()
        {
            coin = 0;
        }
    }

    [System.Serializable]
    public struct AreaInfo
    {
        public int templateHash;
        public Location loc;
        // Generate map after area generation
        public TileInfo[,] map;
        public Dictionary<Location, Location> entranceDic;// First location is direction pattern
        public Dictionary<Location, int> creatureSetHashDic;
        public List<ChestEntity> chests;

        public AreaInfo(int templateHash, Location loc)
        {
            this.templateHash = templateHash;
            this.loc = loc;
            map = new TileInfo[32, 32];
            entranceDic = new Dictionary<Location, Location>();
            creatureSetHashDic = new Dictionary<Location, int>();
            chests = new List<ChestEntity>();
        }

        public static AreaInfo GetActiveAreaInfo(int chunkID, Location loc)
            => Database.Instance.AreaChunks[chunkID].areaInfo[loc];
        public static void SaveToDatabase(int chunkID, AreaInfo info)
            => Database.Instance.SaveAreaInfo(chunkID, info);
    }

    public enum AreaState
    {
        Default,
        Selectable,
        Selecting
    }

    public class AreaController : MonoBehaviour
    {
        [Header("Components")]
        public SpriteRenderer m_spriteRenderer;
        public SpriteRenderer m_cover;
        public Animator m_animator;
        public Transform m_archor;
        public PolygonCollider2D m_coll;
        
        [Header("Colours")]
        public Color moveColor = new Color(0.36f, 1.0f, 0.36f);

        public Vector3 Archor { get { return m_archor.position; } }
        public bool IsExplored { get; private set; }
        public Location Loc { get; private set; }
        public AreaInfo AreaInfo { get { Database.Instance.TryGetAreaInfo(Loc, out AreaInfo info); return info; } }

        public int SortOrder { get { return m_spriteRenderer.sortingOrder; } }

        // para sort order, sprite ID and animation delay time
        public void Init(Location location)
        {
            Loc = location;
            m_spriteRenderer.sortingOrder = -(Loc.x + Loc.y * 8);

            m_spriteRenderer.sprite = AreaInfo.templateHash.GetAreaConfig().sprite;

            m_animator.Play("tile_fall");
            IsExplored = true;
            m_coll.enabled = true;
            StartCoroutine(DelayChange(AreaState.Selectable, 1));
        }

        private IEnumerator DelayChange(AreaState state, float time)
        {
            yield return new WaitForSeconds(time);
            ChangeAreaState(state);
        }

        public void ChangeAreaState(AreaState state)
        {
            switch (state) {
                case AreaState.Default:
                    SetCoverColor(new Color(0, 0, 0, 0));
                    break;
                case AreaState.Selectable:
                    SetCoverColor(moveColor.SetAlpha(0.3f));
                    break;
                case AreaState.Selecting:
                    SetCoverColor(moveColor.SetAlpha(0.7f));
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
}