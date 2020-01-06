using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wing.RPGSystem
{
    public class WorldMapManager : MonoBehaviour
    {
        public static WorldMapManager Instance { get; private set; }

        public LayerMask areaLayer;
        public AreaController areaPrefab;
        public Transform areaHolder;
        public WorldMapUIController m_worldUIController;
        public TitleUIController titleUIController;
        public PlayerController player;

        public Dictionary<Location, AreaController> AreaDic { get; private set; }
        public TileNavigation Nav { get; private set; }
        private Location selectedArea;
        private Location currentLoc;

        private Dictionary<Location, PreAreaInfo> preDic;
        private List<Location> stageAreaLocs;


        private void Awake()
        {
            if (!Instance)
                Instance = this;
            AreaDic = new Dictionary<Location, AreaController>();
        }

        private void Update()
        {
            var ray = Camera.main.ViewportPointToRay(new Vector3(Input.mousePosition.x / Screen.width,
            Input.mousePosition.y / Screen.height, Input.mousePosition.z));
            var hit = Physics2D.Raycast(ray.origin, ray.direction, 20, areaLayer);
            if (hit) {
                var ac = hit.transform.GetComponent<AreaController>();
                if (Input.GetMouseButtonDown(0)) {
                    selectedArea = ac.Loc;
                    AreaInteraction();
                }
            }
        }

        public void ContinueGame()
        {
            if (!m_worldUIController.gameObject.activeSelf) m_worldUIController.gameObject.SetActive(true);

            // Init map visual
            RelocateAreaPosition();
            currentLoc = Location.Zero;
            AreaDic[currentLoc].Init(currentLoc);
            ExploreArea(currentLoc);

            // Init player
            player.transform.position = Vector3.zero;
        }

        private void ExploreArea(Location loc)
        {
            foreach (var dp in AreaDic[loc].AreaInfo.passageDic.Keys) {
                AreaDic[dp + loc].Init(dp + loc);
            }
        }

        private void MoveToArea(Location loc)
        {
            if (!AreaDic.ContainsKey(loc) || !AreaDic[loc].IsExplored) return;

            player.MoveToArea(loc);
        }

        private void RelocateAreaPosition()
        {
            int cnt = Database.Instance.ActiveGameData.worldMap.Count ;
            if (cnt - areaHolder.childCount > 0) {
                for (int i = 0; i < cnt - areaHolder.childCount; i++) {
                    Instantiate(areaPrefab, areaHolder);
                }
            }
            for (int i = 0; i < areaHolder.childCount; i++) {
                if (i == cnt) break;
                Location loc = Database.Instance.ActiveGameData.worldMap[i];
                Nav.SetTileWeight(loc, 1);
                AreaController ac = areaHolder.GetChild(i).GetComponent<AreaController>();
                ac.transform.position = new Vector3(0.524f * (loc.x - loc.y), -0.262f * (loc.x + loc.y), 0);
                //ac.Init_SpriteRenderer(loc);
                AreaDic.Add(loc, ac);
            }
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
            WorldMapUIController.Instance.SetLoadingPage(false);

            ContinueGame();
        }

        private IEnumerator GenerateStage(AdventureStage stage)
        {
            Location dir = Location.Up;
            Location loc = Location.Zero;
            
            preDic = new Dictionary<Location, PreAreaInfo>();
            stageAreaLocs = new List<Location> {
                loc
            };

            preDic.Add(loc, new PreAreaInfo() { loc = loc, connectionDP = new List<Location>(), typeID = -1 });
            // Generate main path
            for (int i = 0; i < stage.stageLength; i++) {
                if (100.Random() < stage.complexity) {
                    if(dir == Location.Up) {
                        dir = loc.x >= 3 ? Location.Left : 
                            (loc.x <= -4 ? Location.Right : 
                            (100.Random() > 50 ? Location.Right : Location.Left));// Limite horizontal range [-4,3]
                    }
                    else {
                        dir = Location.Up;
                    }
                }
                if(i == stage.stageLength - 1) {
                    AddAreaInfo(i, dir, loc, -3);
                    Nav = new TileNavigation(8, loc.y + 1,0);
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
            for (int i = 1,j=0; i < stageAreaLocs.Count; i++) {
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
            AreaInfo info = new AreaInfo {
                templateHash = config.Hash,
                loc = preInfo.loc,
                map = new TileInfo[32, 32],
                passageDic = new Dictionary<Location, Location>()
            };
            for (int x = 0; x < 32; x++) {
                for (int y = 0; y < 32; y++) {
                    int value = arrays[x / 8, y / 8].values[x % 8 + 8 * (y % 8)];
                    TileType type = 10.Random() < (value / 100) ? (TileType)(value % 100) : TileType.Plain;
                    if (type == TileType.SpawnZone) spawnLocs.Add(new Location(x, y));
                    info.map[x, y] = new TileInfo(TemplateTile.Dict[type]);
                }
            }

            foreach (var passage in preInfo.connectionDP) {
                if (passage == Location.Up) {
                    int x = 30.Random() + 1, y = 31;
                    info.map[x, y].template = TemplateTile.Dict[TileType.Passage];
                    info.passageDic.Add(Location.Up, new Location(x, y));
                    y--;
                    while (info.map[x, y].template.type != TileType.Plain) {
                        info.map[x, y].template = TemplateTile.Dict[TileType.Plain];
                        y--;
                    }
                }
                else if(passage == Location.Down) {
                    int x = 30.Random() + 1, y = 0;
                    info.map[x, y].template = TemplateTile.Dict[TileType.Passage];
                    info.passageDic.Add(Location.Down, new Location(x, y));
                    y++;
                    while (info.map[x, y].template.type != TileType.Plain) {
                        info.map[x, y].template = TemplateTile.Dict[TileType.Plain];
                        y++;
                    }
                }
                else if(passage == Location.Left) {
                    int y = 30.Random() + 1, x = 0;
                    info.map[x, y].template = TemplateTile.Dict[TileType.Passage];
                    info.passageDic.Add(Location.Left, new Location(x, y));
                    x++;
                    while (info.map[x, y].template.type != TileType.Plain) {
                        info.map[x, y].template = TemplateTile.Dict[TileType.Plain];
                        x++;
                    }
                }
                else if (passage == Location.Right) {
                    int y = 30.Random() + 1, x = 31;
                    info.map[x, y].template = TemplateTile.Dict[TileType.Passage];
                    info.passageDic.Add(Location.Right, new Location(x, y));
                    x--;
                    while (info.map[x, y].template.type != TileType.Plain) {
                        info.map[x, y].template = TemplateTile.Dict[TileType.Plain];
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
                    info.map[setLoc.x + patternX, setLoc.y + patternY].SetEntity(ce.tCreature.Hash);
                }
            }
                            
            int chunkIndex = Database.Instance.ActiveGameData.worldMap.Count / Database.Instance.areaChunkSize;
            Database.Instance.AreaChunks[chunkIndex].areaInfo.Add(info.loc, info);
            Database.Instance.ActiveGameData.worldMap.Add(info.loc);
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

        private void AreaInteraction()
        {
            WorldMapUIController.Instance.ShowAreaInteractionBoard();
        }

        public void Button_EnterArea()
        {
            AreaManager.Instance.EnterArea(selectedArea);
        }

    }
}

