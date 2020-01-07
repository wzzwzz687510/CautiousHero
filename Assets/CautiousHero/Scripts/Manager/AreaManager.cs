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

        public LayerMask tileLayer;
        public PlayerController player;
        public BattleUIController m_uIController;
        public Camera areaCamera;
        public CinemachineVirtualCamera vCamera;
        public Transform viewPin;

        public Location CurrentAreaLoc { get; private set; }
        public int CurrentAreaIndex { get; private set; }
        public int ChunkIndex { get; private set; }
        public AreaInfo TempData { get; private set; }

        private Location highlightTile;
        private bool hasHighlighted;

        private void Awake()
        {
            if (!Instance)
                Instance = this;
        }

        private void Update()
        {
            CameraAdjustment();
            if (BattleManager.Instance.IsInBattle || WorldMapManager.Instance.IsWorldView) return;
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
                vCamera.m_Lens.OrthographicSize = Mathf.Clamp(areaCamera.orthographicSize - 10 * Time.deltaTime * Input.mouseScrollDelta.y, 3, 4);
            }
            float x = Input.GetAxis("Horizontal");
            float y = Input.GetAxis("Vertical");
            Vector2 axis = new Vector2(x, y);
            viewPin.position = new Vector3(Mathf.Clamp(viewPin.position.x + x * moveSpeed, 96, 138), Mathf.Clamp(viewPin.position.y + y * moveSpeed, 96, 138), 0);
        }

        private void MoveToTile(Location tileLoc)
        {
            if (!tileLoc.IsValid()) return;
            player.MoveToTile(tileLoc);
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

        public void InitArea(Location areaLoc,Location spawnLoc)
        {
            CurrentAreaLoc = areaLoc;
            CurrentAreaIndex = WorldData.ActiveData.worldMap.IndexOf(CurrentAreaLoc);
            ChunkIndex = CurrentAreaIndex / Database.AreaChunkSize;
            TempData = AreaInfo.GetActiveAreaInfo(ChunkIndex, CurrentAreaLoc);
            player.InitPlayer(WorldData.ActiveData.attribute);
            player.MoveToTile(spawnLoc, true);
            m_uIController.Init();
            GridManager.Instance.LoadMap();
        }

        public void SaveAreaInfo()
        {
            AreaInfo.SaveToDatabase(ChunkIndex, TempData);
        }
    }
}