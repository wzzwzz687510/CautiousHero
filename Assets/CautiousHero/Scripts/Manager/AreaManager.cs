using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

namespace Wing.RPGSystem
{

    public class AreaManager : MonoBehaviour
    {
        public static AreaManager Instance { get; private set; }

        public float moveSpeed = 0.1f;

        [Header("Components")]
        public LayerMask tileLayer;
        public PlayerController player;
        public BattleUIController m_battleUIController;
        public GameObject creaturePrefab;

        [Header("View")]
        public Camera areaCamera;
        public CinemachineVirtualCamera vCamera;
        public Transform viewPin;

        public Location CurrentAreaLoc { get; private set; }
        public int CurrentAreaIndex { get; private set; }
        public int ChunkIndex { get; private set; }
        public AreaInfo TempData { get; private set; }
        public bool IsMovable { get; private set; }
        public Dictionary<Location, List<int>> RemainedCreatures { get; private set; } // key - spawn point, value - creature set
        public List<Location> InBatlleCreatureSets { get; private set; }

        private Transform creatureHolder;
        private Location highlightTile;
        private bool hasHighlighted;

        private void Awake()
        {
            if (!Instance)
                Instance = this;
            vCamera.transform.rotation = Quaternion.identity;
        }

        private void Update()
        {
            CameraAdjustment();
            if (!IsMovable|| BattleManager.Instance.IsInBattle || WorldMapManager.Instance.IsWorldView) return;
            var ray = areaCamera.ViewportPointToRay(new Vector3(Input.mousePosition.x / Screen.width,
            Input.mousePosition.y / Screen.height, Input.mousePosition.z));
            var hit = Physics2D.Raycast(ray.origin, ray.direction, 20, tileLayer);
            if (hit) {
                var ac = hit.transform.parent.GetComponent<TileController>();
                HighlightVisual(ac.Loc);

                if (Input.GetMouseButtonDown(0)) {
                    MoveToTile(ac.Loc);
                }
            }
            else if (hasHighlighted) {
                hasHighlighted = false;
                GridManager.Instance.ChangeTileState(highlightTile, TileState.Normal);
            }
        }

        private void CameraAdjustment()
        {
            if (Input.mouseScrollDelta.y != 0) {
                vCamera.m_Lens.OrthographicSize = Mathf.Clamp(areaCamera.orthographicSize - 10 * 
                    Time.deltaTime * Input.mouseScrollDelta.y, 3, 4);
            }
            float x = Input.GetAxisRaw("Horizontal");
            float y = Input.GetAxisRaw("Vertical");
            Vector2 axis = new Vector2(x, y);
            viewPin.position = new Vector3(Mathf.Clamp(viewPin.position.x + x * moveSpeed, 96, 138), 
                Mathf.Clamp(viewPin.position.y + y * moveSpeed, 96, 138), 0);
        }

        private void MoveToTile(Location tileLoc)
        {
            if (!tileLoc.IsValid()) return;

            //player.MoveToLocation(tileLoc, false, false);
            viewPin.localPosition = Vector3.zero;
            BattleCheck(tileLoc);
        }

        private void BattleCheck(Location loc)
        {
            foreach (var spawnLoc in TempData.creatureSetHashDic.Keys) {
                int delta = 4 - spawnLoc.Distance(loc);
                // TODO: improve trigger condition
                if (delta < 0) continue;
                Location stopLoc = delta > 0 ?
                    player.Loc.GetLocationWithGivenStep(loc, player.Loc.Distance(loc) - delta) : loc;
                player.MoveToLocation(stopLoc, false, false);
                BattleManager.Instance.PrepareBattle(RemainedCreatures[spawnLoc]);
                RemainedCreatures.Remove(spawnLoc);
                InBatlleCreatureSets.Add(spawnLoc);
                return;
            }
            player.MoveToLocation(loc, false, false);
        }

        private void HighlightVisual(Location loc)
        {
            if (!hasHighlighted) {
                hasHighlighted = true;
                highlightTile = loc;
                GridManager.Instance.ChangeTileState(highlightTile, TileState.MoveSelected);
            }

            if (hasHighlighted && highlightTile != loc) {
                GridManager.Instance.ChangeTileState(highlightTile, TileState.Normal);
                highlightTile = loc;
                GridManager.Instance.ChangeTileState(highlightTile, TileState.MoveSelected);
            }

        }

        public void SetMovable(bool isMovable)
        {
            this.IsMovable = isMovable;
        }

        public void InitArea(Location areaLoc,Location spawnLoc)
        {
            CurrentAreaLoc = areaLoc;
            CurrentAreaIndex = WorldData.ActiveData.worldMap.IndexOf(CurrentAreaLoc);
            ChunkIndex = CurrentAreaIndex / Database.AreaChunkSize;
            TempData = AreaInfo.GetActiveAreaInfo(ChunkIndex, CurrentAreaLoc);
            RemainedCreatures = new Dictionary<Location, List<int>>();
            InBatlleCreatureSets = new List<Location>();

            player.InitPlayer(WorldData.ActiveData.attribute);
            player.MoveToTile(spawnLoc, true);

            BattleManager.Instance.Init();
            InstantiateCreature();
        }

        public void InstantiateCreature()
        {
            if (creatureHolder) Destroy(creatureHolder.gameObject);
            creatureHolder = new GameObject("Creature Holder").transform;

            foreach (var spawnLoc in TempData.creatureSetHashDic.Keys) {
                List<int> creatureHashes = new List<int>();
                foreach (var ce in TempData.creatureSetHashDic[spawnLoc].GetCreatureSet().creatures) {
                    var cc = Instantiate(creaturePrefab, creatureHolder).GetComponent<CreatureController>();
                    cc.InitCreature(ce.tCreature, ce.pattern + spawnLoc);
                    creatureHashes.Add(cc.Hash);
                    SetEntityHash(cc.Loc, cc.Hash);
                }
                RemainedCreatures.Add(spawnLoc, creatureHashes);
            } 
        }

        public void SaveAreaInfo()
        {
            AreaInfo.SaveToDatabase(ChunkIndex, TempData);
        }

        public void SetEntityHash(Location loc, int hash)
        {
            TempData.map[loc.x, loc.y].stayEntityHash = hash;
            TempData.map[loc.x, loc.y].isEmpty = false;
        }

        public void ClearEntity(Location loc)
        {
            TempData.map[loc.x, loc.y].stayEntityHash = 0;
            TempData.map[loc.x, loc.y].isEmpty = true;
        }
    }
}