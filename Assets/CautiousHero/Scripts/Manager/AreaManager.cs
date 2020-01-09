using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.EventSystems;

namespace Wing.RPGSystem
{

    public class AreaManager : MonoBehaviour
    {
        public static AreaManager Instance { get; private set; }

        public float moveSpeed = 0.1f;

        [Header("Test")]
        public BaseSkill[] skills;

        [Header("Components")]
        public LayerMask tileLayer;
        public PlayerController player;
        public AreaUIController m_areaUIController;
        public RewardUIController m_lootUIController;
        public RewardUIController m_chestUIController;
        public GameObject creaturePrefab;
        public GameObject chestPrefab;

        [Header("View")]
        public Camera areaCamera;
        public CinemachineVirtualCamera vCamera;
        public Transform viewPin;

        public Location CurrentAreaLoc { get; private set; }
        public int CurrentAreaIndex { get; private set; }
        public int ChunkIndex { get; private set; }
        public AreaInfo TempData { get; private set; }
        public bool MoveCheck { get; private set; }
        public Dictionary<Location, List<int>> RemainedCreatures { get; private set; } // key - spawn point, value - creature set
        public List<Location> InBatlleCreatureSets { get; private set; }
        public List<int> RandomedSkillHashes { get; private set; }

        private Transform creatureHolder;
        private Transform chestHolder;
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
            if (!MoveCheck || BattleManager.Instance.IsInBattle || WorldMapManager.Instance.IsWorldView) return;
            if (EventSystem.current.IsPointerOverGameObject()) return;
            var ray = areaCamera.ViewportPointToRay(new Vector3(Input.mousePosition.x / Screen.width,
            Input.mousePosition.y / Screen.height, Input.mousePosition.z));
            var hit = Physics2D.Raycast(ray.origin, ray.direction, 20, tileLayer);
            if (hit) {
                if (hit.transform.CompareTag("Chest")) {
                    if (Input.GetMouseButtonUp(0)) {
                        hit.transform.GetComponent<Animator>().Play("Wooden");
                        m_chestUIController.gameObject.SetActive(true);
                        int chestID = int.Parse(hit.transform.name);
                        ChestEntity ce = TempData.chests[chestID];
                        m_chestUIController.SetChestID(chestID);
                        m_chestUIController.AddContent(LootType.Coin, ce.coin);
                        foreach (var relic in ce.relicHashes) {
                            m_chestUIController.AddContent(LootType.Relic, relic);
                        }
                    }
                }
                else {
                    var ac = hit.transform.parent.GetComponent<TileController>();
                    HighlightVisual(ac.Loc);

                    if (Input.GetMouseButtonDown(0)) {
                        MoveToTile(ac.Loc);
                    }
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
                BattleManager.Instance.NewBattle(RemainedCreatures[spawnLoc]);
                RemainedCreatures.Remove(spawnLoc);
                InBatlleCreatureSets.Add(spawnLoc);
                MoveCheck = false;
                return;
            }
            player.MoveToLocation(loc, false, false);
        }

        private void InstantiateCreatures()
        {
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

        private void InstantiateAbotics()
        {
            // Init Chests
            if (TempData.chests.Count != 0) {
                for (int i = 0; i < TempData.chests.Count; i++) {
                    Instantiate(chestPrefab, chestHolder).name = (i).ToString();
                }
            }
        }

        private void SaveAreaInfo()
        {
            AreaInfo.SaveToDatabase(ChunkIndex, TempData);
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

        public void SetMoveCheck(bool isCheck)
        {
            MoveCheck = isCheck;
        }

        public void InitArea(Location from, Location to)
        {
            if (from == to) return;
            // Fetch world data
            CurrentAreaLoc = to;
            CurrentAreaIndex = WorldData.ActiveData.worldMap.IndexOf(CurrentAreaLoc);
            ChunkIndex = CurrentAreaIndex / Database.AreaChunkSize;
            TempData = AreaInfo.GetActiveAreaInfo(ChunkIndex, CurrentAreaLoc);
            Location spawnLoc = TempData.entranceDic[from - to];

            // Reset holders
            if (chestHolder) Destroy(chestHolder.gameObject);
            if (creatureHolder) Destroy(creatureHolder.gameObject);
            chestHolder = new GameObject("Chest Holder").transform;
            creatureHolder = new GameObject("Creature Holder").transform;

            // Reset lists and dictionaries
            RemainedCreatures = new Dictionary<Location, List<int>>();
            InBatlleCreatureSets = new List<Location>();
            EntityManager.Instance.ResetEntityDicionary();
            RandomedSkillHashes = new List<int>();

            // Init player
            player.InitPlayer(WorldData.ActiveData.attribute);
            player.MoveToTile(spawnLoc, true);

            // Init battle system
            BattleManager.Instance.PrepareBattle();
            InstantiateCreatures();
            InstantiateAbotics();
        }

        public void LeaveArea()
        {
            player.Loc.GetTileController().OnEntityLeaving();
            SaveAreaInfo();
        }

        public void CompleteBattle()
        {
            m_areaUIController.SetSkillsUnknown();
            AudioManager.Instance.PlayPeacefulClip();
            int coin = 0, exp = 0;
            foreach (var setID in InBatlleCreatureSets) {
                CreatureSet set = TempData.creatureSetHashDic[setID].GetCreatureSet();
                TempData.creatureSetHashDic.Remove(setID);
                coin += set.coin;
                exp += set.exp;
                if (set.chest != null) {
                    Instantiate(chestPrefab, chestHolder).name = (TempData.chests.Count + 1).ToString();
                    TempData.chests.Add(new ChestEntity(setID, set.chest));
                }
                m_lootUIController.AddContent(LootType.Skill, 1);
            }
            m_lootUIController.AddContent(LootType.Coin, coin);
            m_lootUIController.AddContent(LootType.Exp, exp);
            m_lootUIController.gameObject.SetActive(true);
            SaveAreaInfo();
        }

        public void CompleteExploration()
        {
            GridManager.Instance.SaveExplorationState();
            foreach (var dp in TempData.entranceDic.Keys) {
                if(GridManager.Instance.CheckEntrance(TempData.entranceDic[dp])) {
                    WorldMapManager.Instance.DiscoverArea(TempData.loc + dp);
                }
            }
            WorldMapManager.Instance.CompleteAnArea();
        }

        public void SetExploration(Location loc)
        {
            TempData.map[loc.x, loc.y].isExplored = true;
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

        public void PrepareChooseSkill()
        {
            RandomedSkillHashes.Clear();
            for (int i = 0; i < 3; i++) {
                RandomedSkillHashes.Add(skills[skills.Length.Random()].Hash);
            }
            m_areaUIController.ShowSkillLearningPage(true);
        }

        public void Button_ChooseSkill(int id)
        {
            Database.Instance.LearnASkill(RandomedSkillHashes[id]);
            m_areaUIController.ShowSkillLearningPage(false);
            m_lootUIController.CloseCheck();         
        }

        public void RemoveChestCoin(int chestID)
        {
            Debug.Log("Before: " + TempData.chests[chestID].coin);
            TempData.chests[chestID].RemoveCoin();
            Debug.Log("After: " + TempData.chests[chestID].coin);
            SaveAreaInfo();
        }

        public void RemoveChestRelic(int chestID, int relicHash)
        {
            TempData.chests[chestID].relicHashes.Remove(relicHash);
            SaveAreaInfo();
        }
    }
}