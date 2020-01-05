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

        public Dictionary<Location, AreaController> AreaDic { get; private set; }
        private Location selectedArea;

        private Dictionary<Location, PreAreaInfo> preDic;
        private List<Location> stageAreaLocs;

        private void Awake()
        {
            if (!Instance)
                Instance = this;
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
            RelocateAreaPosition();

        }

        private void RelocateAreaPosition()
        {
            int cnt = Database.Instance.ActiveGameData.worldMap.Count - areaHolder.childCount;
            if (cnt > 0) {
                for (int i = 0; i < cnt; i++) {
                    Instantiate(areaPrefab, areaHolder);
                }
            }
            for (int i = 0; i < areaHolder.childCount; i++) {
                Location loc = Database.Instance.ActiveGameData.worldMap[i];
                AreaController ac = areaHolder.GetChild(i).GetComponent<AreaController>();
                ac.transform.position = new Vector3(0.524f * (loc.x - loc.y), -0.262f * (loc.x + loc.y), 0);
                ac.Init_SpriteRenderer(loc);
            }
        }


        #region Generate New World
        public void StartNewGame()
        {
            StartCoroutine(GenerateNewWorld());
        }

        private IEnumerator GenerateNewWorld()
        {
            WorldMapUIController.Instance.SetLoadingPage(true);
            var worldConfig = WorldConfig.Dict[Database.Instance.ActiveGameData.worldConfigHash];
            int areaNumber = 0;
            for (int i = 0; i < worldConfig.stages.Length; i++) {
                areaNumber += worldConfig.stages[i].stageLength;
            }
            Database.Instance.InitAreaChunk(areaNumber);
            for (int i = 0; i < worldConfig.stages.Length; i++) {
                yield return StartCoroutine(GenerateStage(worldConfig.stages[i]));
            }
            WorldMapUIController.Instance.SetLoadingPage(false);
        }

        private IEnumerator GenerateStage(AdventureStage stage)
        {
            Location dir = Location.Up;
            Location loc = Location.Zero;
            
            preDic = new Dictionary<Location, PreAreaInfo>();
            stageAreaLocs = new List<Location> {
                loc
            };
            preDic.Add(loc, new PreAreaInfo() { loc = loc });
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
                AddAreaInfo(i, dir, loc, (i == stage.stageLength - 1) ? -3 : -1);
                loc += dir;
                yield return null;
            }

            // Add extra standard area
            int spareMainAreaNumber = Mathf.Min(stage.mainAreaNumber - stage.stageLength, stageAreaLocs.Count);
            for (int i = 0, j = 0; i < stageAreaLocs.Count; i++) {
                PreAreaInfo info = preDic[stageAreaLocs[i]];
                while (info.connectionDP.Count != 4 && 100.Random() < stage.complexity) {
                    if (j > spareMainAreaNumber) break;
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
                    if (j > stage.specialConfigs.Length) break;
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
            SubArea[,] subArea = new SubArea[4, 4];
            for (int x = 0; x < 4; x++) {
                for (int y = 0; y < 4; y++) {
                    if (x == 0 || x == 3) {
                        if (y == 0 || y == 3) {
                            subArea[x, y].coordinateValues = config.cornerAreas[config.cornerAreas.Length.Random()].GetInverseValues(x == 3, y == 3);
                        }
                        else {
                            subArea[x, y].coordinateValues = config.vEdgeAreas[config.vEdgeAreas.Length.Random()].GetInverseValues(x == 3, false);
                        }
                        continue;
                    }
                    else if (y == 0 || y == 3) {
                        subArea[x, y].coordinateValues = config.hEdgeAreas[config.hEdgeAreas.Length.Random()].GetInverseValues(false, y == 3);
                        continue;
                    }
                    else {
                        subArea[x, y].coordinateValues = config.centreAreas[config.centreAreas.Length.Random()].coordinateValues;
                    }
                }                
            }

            List<Location> spawnLocs = new List<Location>();
            AreaInfo info = new AreaInfo {
                templateHash = config.Hash,
                loc = preInfo.loc,
                map = new TileInfo[32, 32]
            };
            for (int x = 0; x < 32; x++) {
                for (int y = 0; y < 32; y++) {
                    int value = subArea[x / 8, y / 8].coordinateValues[x % 8, y % 8];
                    TileType type = value.Random() < 100 ? (TileType)(value % 100) : TileType.Plain;
                    if (type == TileType.SpawnZone) spawnLocs.Add(new Location(x, y));
                    info.map[x, y] = new TileInfo(TemplateTile.Dict[type]);
                }
            }

            foreach (var passage in preInfo.connectionDP) {
                if (passage == Location.Up) {
                    int x = 30.Random() + 1, y = 31;
                    while (info.map[x, y].template.type != TileType.Plain) {
                        info.map[x, y].template = TemplateTile.Dict[TileType.Plain];
                        y--;
                    }
                }
                else if(passage == Location.Down) {
                    int x = 30.Random() + 1, y = 0;
                    while (info.map[x, y].template.type != TileType.Plain) {
                        info.map[x, y].template = TemplateTile.Dict[TileType.Plain];
                        y++;
                    }
                }
                else if(passage == Location.Left) {
                    int y = 30.Random() + 1, x = 0;
                    while (info.map[x, y].template.type != TileType.Plain) {
                        info.map[x, y].template = TemplateTile.Dict[TileType.Plain];
                        x++;
                    }
                }
                else if (passage == Location.Right) {
                    int y = 30.Random() + 1, x = 31;
                    while (info.map[x, y].template.type != TileType.Plain) {
                        info.map[x, y].template = TemplateTile.Dict[TileType.Plain];
                        x--;
                    }
                }
            }

            // TODO multiple creature set
            CreatureSet set = config.creatureSets.GetRandomSet(preInfo.isHardSet);
            Location setLoc = spawnLocs[spawnLocs.Count.Random()];
            info.creatureSetHashDic.Add(setLoc, set.Hash);
            foreach (var ce in set.creatures) {
                info.map[setLoc.x + ce.pattern.x, setLoc.y + ce.pattern.y].SetEntity(ce.tCreature.Hash);
            }
                            
            int chunkIndex = Database.Instance.ActiveGameData.worldMap.Count / Database.Instance.areaChunkSize;
            Database.Instance.AreaChunks[chunkIndex].areaInfo.Add(info.loc, info);
            Database.Instance.ActiveGameData.worldMap.Add(preInfo.loc);
        }

        private void AddAreaInfo(int index, Location dir,Location loc,int typeID)
        {
            Location newLoc = loc + dir;
            var info = new PreAreaInfo() { loc = newLoc, typeID = typeID, isHardSet = typeID == -2 };

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

