using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace Wing.RPGSystem
{
    public class WorldMapManager : MonoBehaviour
    {
        public static WorldMapManager Instance { get; private set; }

        [Header("Control")]
        public Camera worldCamera;
        public PlayerController character;

        [Header("Area Elements")]
        public LayerMask areaLayer;
        public AreaController areaPrefab;
        public Transform areaHolder;

        [Header("UI")]
        public WorldMapUIController m_worldUIController;
        public TitleUIController titleUIController;       
        
        public Dictionary<Location, AreaController> AreaDic { get; private set; }
        public TileNavigation Nav { get; private set; }
        public bool IsWorldView { get; private set; }

        private Location currentLoc;
        private Location highlightArea;
        private bool hasHighlighted;

        private Dictionary<Location, PreAreaInfo> preDic;
        private List<Location> stageAreaLocs;


        private void Awake()
        {
            if (!Instance)
                Instance = this;
            AreaDic = new Dictionary<Location, AreaController>();
            character.EntitySprite.DOFade(0, 0);
            IsWorldView = true;
        }

        private void Update()
        {
            if (!IsWorldView) return;
            var ray = worldCamera.ViewportPointToRay(new Vector3(Input.mousePosition.x / Screen.width,
            Input.mousePosition.y / Screen.height, Input.mousePosition.z));
            var hit = Physics2D.Raycast(ray.origin, ray.direction, 20, areaLayer);
            if (hit) {
                var ac = hit.transform.parent.GetComponent<AreaController>();
                if (ac.IsExplored) {
                    HighlightVisual(ac.Loc);

                    if (Input.GetMouseButtonDown(0)) {
                        MoveToArea(ac.Loc);
                    }
                }
            }
            else if (hasHighlighted) {
                hasHighlighted = false;
                AreaDic[highlightArea].ChangeAreaState(AreaState.Selectable);
            }
        }

        public void SetWorldView(bool isWorldView)
        {
            IsWorldView = isWorldView;
        }

        public void EnterArea(Location areaLoc)
        {
            if (!AreaDic.ContainsKey(areaLoc) || areaLoc == new Location(4, 0)) return;
            m_worldUIController.SwitchToAreaView();
            if (areaLoc != currentLoc) {
                Location dir = Nav.GetDirectionPattern(currentLoc, areaLoc);
                AreaManager.Instance.InitArea(areaLoc, dir);
            }
            currentLoc = areaLoc;
        }

        public void DiscoverArea(Location loc)
        {
            AreaDic[loc].Init(loc);
            Database.Instance.SetAreaDiscovered(loc);
            //Debug.Log(string.Format("Area {0} discover: {1}",loc.ToString(),AreaDic[loc].AreaInfo.discovered));
        }

        public void CompleteAnArea()
        {
            m_worldUIController.SwitchToWorldView();
            Database.Instance.CompleteAnArea(currentLoc);
        }

        public void ContinueGame()
        {
            AudioManager.Instance.PlayPeacefulClip();
            m_worldUIController.gameObject.SetActive(true);
            m_worldUIController.UpdateUI();

            // Init map visual
            Location bound = WorldData.ActiveData.worldBound;
            Nav = new TileNavigation(bound.x, bound.y, 0);          
            InitAreas();

            // Init player
            StartCoroutine(DelayInitCharacter(1));
        }

        public void EnterNextStage()
        {
            m_worldUIController.SwitchToWorldView();
            if (WorldData.ReachLastStage) {
                WorldClear();
                return;
            }                

            Database.Instance.SetCurrentStage(WorldData.ActiveData.currentStage + 1);
            currentLoc = WorldData.ActiveData.stageLocations[WorldData.ActiveData.currentStage];
            DiscoverArea(currentLoc);
            m_worldUIController.SwitchToWorldView();
            character.MoveToLocation(currentLoc, true, true);
            foreach (var dp in AreaDic[currentLoc].AreaInfo.entranceDic.Keys) {
                DiscoverArea(dp + currentLoc);
            }
            Database.Instance.SaveAreaChunks();
        }

        private void InitAreas()
        {
            RelocateAreaPosition();
            currentLoc = WorldData.ActiveData.characterLocation;

            List<Location> locs = Database.Instance.GetDiscoveredAreaLocs();
            if (locs.Count == 0) {
                DiscoverArea(currentLoc);
                foreach (var dp in AreaDic[currentLoc].AreaInfo.entranceDic.Keys) {
                    DiscoverArea(dp + currentLoc);
                }
                Database.Instance.SaveAreaChunks();
            }

            foreach (var loc in locs) {
                AreaDic[loc].Init(loc);
            }
            if(!WorldData.ActiveData .stageLocations.Contains(currentLoc))
                AreaManager.Instance.InitArea(currentLoc, WorldData.ActiveData.enterAreaDirection);

        }

        private IEnumerator DelayInitCharacter(float time)
        {
            yield return new WaitForSeconds(time);
            character.InitCharacter("WorldCharacter",WorldData.ActiveData.attribute);
            character.MoveToLocation(currentLoc, true, true);
            character.EntitySprite.DOFade(1, 0.5f);
        }

        private IEnumerator WaitForMoveAnim(Location areaLoc)
        {
            while (AnimationManager.Instance.IsPlaying) {
                yield return null;
            }
            yield return new WaitForSeconds(0.1f);
            EnterArea(areaLoc);
        }

        private void MoveToArea(Location areaLoc)
        {
            if (!AreaDic.ContainsKey(areaLoc) || !AreaDic[areaLoc].IsExplored) return;
            character.MoveToLocation(areaLoc, true, false);
            StartCoroutine(WaitForMoveAnim(areaLoc));
            //ExploreArea(loc);
            
        }

        private void RelocateAreaPosition()
        {
            AreaDic.Clear();
            int cnt = WorldData.ActiveData.worldMap.Count ;
            if (cnt - areaHolder.childCount > 0) {
                for (int i = 0; i < cnt - areaHolder.childCount; i++) {
                    Instantiate(areaPrefab, areaHolder);
                }
            }
            for (int i = 0; i < areaHolder.childCount; i++) {
                if (i == cnt) break;
                Location loc = WorldData.ActiveData.worldMap[i];
                Nav.SetTileWeight(loc, 1);
                AreaController ac = areaHolder.GetChild(i).GetComponent<AreaController>();
                ac.transform.position = new Vector3(0.524f * (loc.y-loc.x), 0.262f * (loc.x + loc.y), 0);
                //ac.Init_SpriteRenderer(loc);
                AreaDic.Add(loc, ac);
            }
        }

        private void WorldClear()
        {
            // TODO: Add score statistics to unlock content and for player review
            Database.Instance.SetNewGame();
            m_worldUIController.DisplayEndPage();
        }

        #region Generate New World
        public void StartNewGame()
        {
            m_worldUIController.gameObject.SetActive(true);
            StartCoroutine(GenerateNewWorld());
        }

        private IEnumerator GenerateNewWorld()
        {
            WorldMapUIController.Instance.SetLoadingPage(true);
            var worldConfig = Database.Instance.defaultWorldConfig;
            int areaNumber = 0;
            for (int i = 0; i < worldConfig.stages.Length; i++) {
                areaNumber += worldConfig.stages[i].stageLength;
            }
            Database.Instance.InitAreaChunk(areaNumber);
            for (int i = 0; i < worldConfig.stages.Length; i++) {
                yield return StartCoroutine(GenerateStage(worldConfig.stages[i]));
            }
            Database.Instance.SaveAll();
            WorldMapUIController.Instance.SetLoadingPage(false);

            ContinueGame();
        }

        private IEnumerator GenerateStage(AdventureStage stage)
        {
            Location dir = Location.Up;
            Location loc = new Location(4, WorldData.ActiveData.worldBound.y + 1);
            WorldData.ActiveData.stageLocations.Add(loc);

            preDic = new Dictionary<Location, PreAreaInfo>();
            stageAreaLocs = new List<Location> {
                loc
            };

            preDic.Add(loc, new PreAreaInfo() { loc = loc, connectionDP = new List<Location>(), typeID = -1 });
            // Generate main path
            for (int i = 0; i < stage.stageLength; i++) {
                if (100.Random() < stage.complexity) {
                    if(dir == Location.Up) {
                        dir = loc.x >= 7 ? Location.Left : 
                            (loc.x <= 0 ? Location.Right : 
                            (100.Random() > 50 ? Location.Right : Location.Left));// Limite horizontal range [0,7]
                    }
                    else {
                        dir = Location.Up;
                    }
                }
                else if (loc.x >= 7 || loc.x <= 0) {
                    dir = Location.Up;
                }

                if(i == stage.stageLength - 1) {
                    AddAreaInfo(i, dir, loc, -3);
                    Nav = new TileNavigation(8, loc.y + 1,0);
                    Database.Instance.SetWorldBound(8, loc.y + 2);
                }
                else {
                    AddAreaInfo(i, dir, loc, -1);
                }
                
                loc += dir;
                yield return null;
            }

            // Add extra standard area
            int spareMainAreaNumber = Mathf.Min(stage.mainAreaNumber - stage.stageLength, stageAreaLocs.Count);
            for (int i = 0, j = 0; i < stageAreaLocs.Count; i++) {
                PreAreaInfo info = preDic[stageAreaLocs[i]];
                while (info.connectionDP.Count != 4 && 100.Random() < stage.complexity) {
                    if (j >= spareMainAreaNumber) break;
                    List<Location> dirs = new List<Location>() { Location.Up, Location.Down, Location.Left, Location.Right };
                    foreach (var dp in info.connectionDP) {
                        dirs.Remove(dp);
                    }
                    AddAreaInfo(i, dirs[dirs.Count.Random()], info.loc, -2);
                    j++;  
                }
                yield return null;
            }

            // Add special area
            List<AreaType> types = new List<AreaType>();
            foreach (var config in stage.specialConfigs) {
                types.Add(config.type);
            }
            for (int i = 1, j = 0; i < stageAreaLocs.Count; i++) {
                int areaIndex = stageAreaLocs.Count - i;
                PreAreaInfo info = preDic[stageAreaLocs[areaIndex]];
                while (info.connectionDP.Count != 4 && 100.Random() < stage.complexity) {
                    if (j >= stage.specialConfigs.Length) break;
                    List<Location> dirs = new List<Location>() { Location.Up, Location.Down, Location.Left, Location.Right };
                    foreach (var dp in info.connectionDP) {
                        dirs.Remove(dp);
                    }
                    int randomType = types.Count.Random();
                    AddAreaInfo(i, dirs[dirs.Count.Random()], info.loc, randomType);
                    types.RemoveAt(randomType);
                    j++;
                }
                yield return null;
            }


            // Generate each area's map
            for (int i = 0; i < stageAreaLocs.Count; i++) {
                PreAreaInfo info = preDic[stageAreaLocs[i]];
                if (info.typeID == -1|| info.typeID == -2) {
                    SaveToDatabase(info, stage.mainConfig);
                }
                if (info.typeID == -3) {
                    SaveToDatabase(info, stage.bossConfig);
                }
                else if (info.typeID >= 0) {
                    SaveToDatabase(info, stage.specialConfigs[info.typeID]);
                }
                yield return null;
            }
        }

        private void SaveToDatabase(PreAreaInfo preInfo,AreaConfig config)
        {
            SubAreaSet sets = config.subAreaSets;
            SubAreaArray[,] arrays = new SubAreaArray[4, 4];
            for (int x = 0; x < 4; x++) {
                for (int y = 0; y < 4; y++) {
                    if (x == 0 || x == 3) {
                        if (y == 0 || y == 3) {
                            int length = sets.cornerAreas.Length;
                            if (length == 0) Debug.LogError("Sub area set error, please check config: " + config.configName);
                            arrays[x, y].values = sets.cornerAreas[length.Random()].GetInverseValues(x == 3, y == 3);
                        }
                        else {
                            int length = sets.vEdgeAreas.Length;
                            if (length == 0) Debug.LogError("Sub area set error, please check config: " + config.configName);
                            arrays[x, y].values = sets.vEdgeAreas[length.Random()].GetInverseValues(x == 3, false);
                        }
                        continue;
                    }
                    else if (y == 0 || y == 3) {
                        int length = sets.hEdgeAreas.Length;
                        if (length == 0) Debug.LogError("Sub area set error, please check config: " + config.configName);
                        arrays[x, y].values = sets.hEdgeAreas[length.Random()].GetInverseValues(false, y == 3);
                        continue;
                    }
                    else {
                        int length = sets.centreAreas.Length;
                        if (length == 0) Debug.LogError("Sub area set error, please check config: " + config.configName);
                        arrays[x, y].values = sets.centreAreas[length.Random()].coordinateValues;
                    }
                }
            }

            List<Location> spawnLocs = new List<Location>();
            AreaInfo info = new AreaInfo(config.Hash, preInfo.loc);
            for (int x = 0; x < 32; x++) {
                for (int y = 0; y < 32; y++) {
                    int value = arrays[x / 8, y / 8].values[x % 8 + 8 * (y % 8)];
                    if (value / 100 == 0 || 10.Random() < (value / 100)) {
                        int selectTileID = value % 100;
                        if (config.tileSet.tTiles[selectTileID].type == TileType.SpawnZone) spawnLocs.Add(new Location(x, y));
                        info.map[x, y] = new TileInfo(TTile.Dict[config.tileSet.tTiles[selectTileID].Hash]);
                    }
                    else {
                        info.map[x, y] = new TileInfo(TTile.Dict[config.tileSet.defaultTile.Hash]);
                    }
                }
            }

            foreach (var passage in preInfo.connectionDP) {
                if (passage == Location.Up) {
                    int x = 30.Random() + 1, y = 31;
                    info.map[x, y].SetTemplate(config.tileSet.entranceTile);
                    info.entranceDic.Add(Location.Up, new Location(x, y));
                    y--;
                    while (info.map[x, y].IsBlocked) {
                        info.map[x, y].tTileHash = config.tileSet.defaultTile.Hash;
                        info.map[x, y].isObstacle = false;
                        y--;
                    }
                }
                else if(passage == Location.Down) {
                    int x = 30.Random() + 1, y = 0;
                    info.map[x, y].SetTemplate(config.tileSet.entranceTile);
                    info.entranceDic.Add(Location.Down, new Location(x, y));
                    y++;
                    while (info.map[x, y].IsBlocked) {
                        info.map[x, y].tTileHash = config.tileSet.defaultTile.Hash;
                        info.map[x, y].isObstacle = false;
                        y++;
                    }
                }
                else if(passage == Location.Left) {
                    int y = 30.Random() + 1, x = 0;
                    info.map[x, y].SetTemplate(config.tileSet.entranceTile);
                    info.entranceDic.Add(Location.Left, new Location(x, y));
                    x++;
                    while (info.map[x, y].IsBlocked) {
                        info.map[x, y].tTileHash = config.tileSet.defaultTile.Hash;
                        info.map[x, y].isObstacle = false;
                        x++;
                    }
                }
                else if (passage == Location.Right) {
                    int y = 30.Random() + 1, x = 31;
                    info.map[x, y].SetTemplate(config.tileSet.entranceTile);
                    info.entranceDic.Add(Location.Right, new Location(x, y));
                    x--;
                    while (info.map[x, y].IsBlocked) {
                        info.map[x, y].tTileHash = config.tileSet.defaultTile.Hash;
                        info.map[x, y].isObstacle = false;
                        x--;
                    }
                }
            }

            // TODO multiple creature set
            if (spawnLocs.Count != 0) {
                CreatureSet set = config.creatureSets.GetRandomSet(preInfo.isHardSet);
                Location setLoc = spawnLocs[spawnLocs.Count.Random()];
                info.creatureSetHashDic.Add(setLoc, set.Hash);
                int rotTimes = 4.Random();
                int cosine = (int)Mathf.Cos(rotTimes * 90 * Mathf.Deg2Rad);
                int sine = (int)Mathf.Sin(rotTimes * 90 * Mathf.Deg2Rad);
                int patternX, patternY;
                foreach (var ce in set.creatures) {
                    patternX = ce.pattern.x * cosine + ce.pattern.y * sine;
                    patternY = -ce.pattern.x * sine + ce.pattern.y * cosine;
                    info.map[setLoc.x + patternX, setLoc.y + patternY].isEmpty = false;
                }
                info.creatureSetRotTimesDic.Add(setLoc, rotTimes);
            }
                            
            int chunkIndex = WorldData.ActiveData.worldMap.Count / Database.AreaChunkSize;
            Database.Instance.AreaChunks[chunkIndex].areaInfo.Add(info.loc, info);
            WorldData.ActiveData.worldMap.Add(info.loc);
        }

        private void AddAreaInfo(int index, Location dir,Location loc,int typeID)
        {
            Location newLoc = loc + dir;
            var info = new PreAreaInfo() {
                loc = newLoc,
                typeID = typeID,
                isHardSet = typeID == -2,
                connectionDP = new List<Location>()
            };

            preDic[loc].connectionDP.Add(dir);

            if (!stageAreaLocs.Contains(newLoc)) {
                stageAreaLocs.Add(newLoc);
                preDic.Add(newLoc, info);
            }

            preDic[newLoc].connectionDP.Add(-dir);
        }
        #endregion

        private void HighlightVisual(Location loc)
        {
            if(!hasHighlighted) {
                hasHighlighted = true;
                highlightArea = loc;
                AreaDic[loc].ChangeAreaState(AreaState.Selecting);
            }

            if(hasHighlighted && highlightArea != loc) {
                AreaDic[highlightArea].ChangeAreaState(AreaState.Selectable);
                highlightArea = loc;
                AreaDic[loc].ChangeAreaState(AreaState.Selecting);
            }

        }
    }
}

