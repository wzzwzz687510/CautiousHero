using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.EventSystems;
using DG.Tweening;
using Priority_Queue;
using System.Linq;

namespace Wing.RPGSystem
{

    public class AreaManager : MonoBehaviour
    {
        public static AreaManager Instance { get; private set; }

        public float moveSpeed = 0.1f;

        [Header("Test")]
        public BaseSkill[] skills;
        public int enemyViewDistance = 5;
        public int playerViewDistance = 5;

        [Header("Components")]
        public LayerMask tileLayer;
        public PlayerController character;
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
        public Dictionary<Location, HashSet<Location>> AlertZone { get; private set; }
        public List<Location> InBatlleCreatureSets { get; private set; }
        public int[] RandomedSkillHashes { get; private set; }

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
                    Time.deltaTime * Input.mouseScrollDelta.y, 3, 6);
            }
            float x = Input.GetAxisRaw("Horizontal");
            float y = Input.GetAxisRaw("Vertical");
            Vector2 axis = new Vector2(x, y);
            viewPin.position = new Vector3(Mathf.Clamp(viewPin.position.x + x * moveSpeed, 96, 138),
                Mathf.Clamp(viewPin.position.y + y * moveSpeed, 96, 138), 0);
        }

        private IEnumerator CompleteBattleAfterAnim()
        {
            while (AnimationManager.Instance.IsPlaying || m_areaUIController.IsDisplayInfoAnim) {
                yield return null;
            }
            yield return new WaitForSeconds(0.3f);
            AudioManager.Instance.ChangeBGM(TempData.templateHash.GetAreaConfig().bgm, 0);
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
            SaveData();
        }

        private IEnumerator CompleteExplorationAfterAnim()
        {
            while (AnimationManager.Instance.IsPlaying) {
                yield return null;
            }
            
            yield return new WaitForSeconds(0.1f);
            AudioManager.Instance.PlayEnterClip();
            CompleteExploration();
        }

        private void MoveToTile(Location tileLoc)
        {
            if (!tileLoc.IsValid() || AnimationManager.Instance.IsPlaying) return;
            viewPin.localPosition = Vector3.zero;
            GridManager.Instance.DiscoverTiles(BattleCheck(tileLoc));
            if (TempData.map[tileLoc.x, tileLoc.y].GetTileType() == TileType.Entrance)
                StartCoroutine(CompleteExplorationAfterAnim());
        }

        private Location BattleCheck(Location loc)
        {
            foreach (var spawnLoc in RemainedCreatures.Keys) {
                var orderedHashes = new SimplePriorityQueue<int>();
                foreach (var creatureHash in RemainedCreatures[spawnLoc]) {
                    Entity creature = creatureHash.GetEntity();
                    int distance = character.Loc.Distance(creature.Loc);
                    creature.SetVisual(distance <= playerViewDistance);
                    orderedHashes.Enqueue(creatureHash, distance);                 
                }

                Location des = character.Loc.HasPath(loc) ? loc : loc.GetNearestUnblockedLocation(character.Loc);
                foreach (var step in character.Loc.GetPath(des)) {
                    if (AlertZone[spawnLoc].Contains(step)) {                        
                        character.MoveToLocation(step, false, false);
                        foreach (var hash in RemainedCreatures[spawnLoc]) hash.GetEntity().SetVisual(true);
                        BattleManager.Instance.NewBattle(RemainedCreatures[spawnLoc]);
                        RemainedCreatures.Remove(spawnLoc);
                        InBatlleCreatureSets.Add(spawnLoc);
                        MoveCheck = false;
                        return step;
                    }
                }
            }
            character.MoveToLocation(loc, false, false);
            return loc;
        }

        private void InstantiateCreatures()
        {
            foreach (var spawnLoc in TempData.creatureSetHashDic.Keys) {
                List<int> creatureHashes = new List<int>();
                int rotTimes = TempData.creatureSetRotTimesDic[spawnLoc];
                int cosine = (int)Mathf.Cos(rotTimes * 90 * Mathf.Deg2Rad);
                int sine = (int)Mathf.Sin(rotTimes * 90 * Mathf.Deg2Rad);
                int patternX, patternY;
                foreach (var ce in TempData.creatureSetHashDic[spawnLoc].GetCreatureSet().creatures) {
                    var cc = Instantiate(creaturePrefab, creatureHolder).GetComponent<CreatureController>();
                    patternX = ce.pattern.x * cosine + ce.pattern.y * sine;
                    patternY = -ce.pattern.x * sine + ce.pattern.y * cosine;
                    cc.InitCreature(ce.tCreature, new Location(patternX, patternY) + spawnLoc);
                    creatureHashes.Add(cc.Hash);
                }
                RemainedCreatures.Add(spawnLoc, creatureHashes);

                var alertZone = new HashSet<Location>();
                foreach (var loc in spawnLoc.GetGivenDistancePoints(enemyViewDistance)) {
                    alertZone.Add(loc);
                }
                AlertZone.Add(spawnLoc, alertZone);
            }

            if(RemainedCreatures.Count == 0 && TempData.GetAreaInfoType() == AreaType.Boss) {
                m_areaUIController.endStageButton.gameObject.SetActive(true);
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

        private void SaveData()
        {
            ClearEntity(character.Loc);
            Database.Instance.SetCharacterData(character.HealthPoints);
            AreaInfo.SaveToDatabase(ChunkIndex, TempData);
            Database.Instance.SaveWorldData();
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

        public void InitArea(Location to, Location directionPattern)
        {
            // Save enter info
            Database.Instance.EnterAnArea(to, directionPattern);

            // Fetch world data
            CurrentAreaLoc = to;
            CurrentAreaIndex = WorldData.ActiveData.worldMap.IndexOf(CurrentAreaLoc);
            ChunkIndex = CurrentAreaIndex / Database.AreaChunkSize;
            TempData = AreaInfo.GetActiveAreaInfo(ChunkIndex, CurrentAreaLoc);
            Location spawnLoc = TempData.entranceDic[directionPattern];

            // Reset holders
            if (chestHolder) Destroy(chestHolder.gameObject);
            if (creatureHolder) Destroy(creatureHolder.gameObject);
            chestHolder = new GameObject("Chest Holder").transform;
            creatureHolder = new GameObject("Creature Holder").transform;

            // Reset lists and dictionaries
            RemainedCreatures = new Dictionary<Location, List<int>>();
            AlertZone = new Dictionary<Location, HashSet<Location>>();
            InBatlleCreatureSets = new List<Location>();
            EntityManager.Instance.ResetEntityDicionary();

            // Init player
            character.InitCharacter("AreaPlayer",WorldData.ActiveData.attribute, WorldData.ActiveData.HealthPoints);
            character.MoveToLocation(spawnLoc,false, true);
            character.transform.position = spawnLoc.ToPosition();
            m_areaUIController.BindBuffManager();

            // Init battle system
            BattleManager.Instance.Init();
            InstantiateCreatures();
            InstantiateAbotics();
            //SaveAreaInfo();
        }

        public void CompleteBattle()
        {
            m_areaUIController.SetSkillsUnknown();

            if (RemainedCreatures.Count == 0 && TempData.GetAreaInfoType() == AreaType.Boss) {
                m_areaUIController.DisplayInfo("Stage Clear");
                m_areaUIController.endStageButton.gameObject.SetActive(true);
            }

            StartCoroutine(CompleteBattleAfterAnim());
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

        public void SetMana(Location loc, ElementMana mana)
        {
            TempData.map[loc.x, loc.y].mana = mana;
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
            RandomedSkillHashes = Database.Instance.defaultWorldConfig.RandomBattleSkill(3);
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
            SaveData();
        }

        public void RemoveChestRelic(int chestID, int relicHash)
        {
            TempData.chests[chestID].relicHashes.Remove(relicHash);
            SaveData();
        }

        public void LeaveArea()
        {
            character.Loc.GetTileController().OnEntityLeaving();
            SaveData();
        }

        public void SetMoveCheck(bool isCheck) => MoveCheck = isCheck;
    }
}