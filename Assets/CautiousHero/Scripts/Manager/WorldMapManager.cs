using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wing.RPGSystem
{
    public class WorldMapManager : MonoBehaviour
    {
        public LayerMask areaLayer;

        public Dictionary<Location, AreaController> AreaDic { get; private set; }
        private Location selectedArea;

        public void Update()
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

        public IEnumerator Init()
        {
            var worldConfig = WorldConfig.Dict[Database.Instance.ActiveData.worldConfigHash];
            int areaNumber = 0;
            for (int i = 0; i < worldConfig.stages.Length; i++) {
                areaNumber += worldConfig.stages[i].stageLength;
            }
            Database.Instance.InitAreaChunk(areaNumber);
            for (int i = 0; i < worldConfig.stages.Length; i++) {
                yield return StartCoroutine(GenerateStage(worldConfig.stages[i]));
            }
        }

        private IEnumerator GenerateStage(AdventureStage stage)
        {
            Location dir = Location.Up;
            Location loc = Location.Zero;
            Database.Instance.ActiveData.worldMap.Add(loc);
            Database.Instance.AreaChunks[0].areaInfo.Add(loc, new AreaInfo() { loc = loc });
            // Generate main path
            for (int i = 0; i < stage.stageLength; i++) {
                if (100.Random() < stage.complexity) {
                    if(dir == Location.Up) {
                        dir = 100.Random() > 50 ? Location.Right : Location.Left;
                    }
                    else {
                        dir = Location.Up;
                    }
                }
                AddAreaInfo(i, dir, loc, (i == stage.stageLength - 1) ? -2 : -1);
                loc += dir;
                yield return null;
            }

            // Add extra standard area
            int spareMainAreaNumber = Mathf.Min(stage.mainAreaNumber - stage.stageLength, Database.Instance.ActiveData.worldMap.Count);
            for (int i = 0, j = 0; i < Database.Instance.ActiveData.worldMap.Count; i++) {
                int chunkIndex = i / Database.Instance.areaChunkSize;
                AreaInfo info = Database.Instance.AreaChunks[chunkIndex].areaInfo[Database.Instance.ActiveData.worldMap[i]];
                while (info.connectionDP.Count != 4 && 100.Random() < stage.complexity) {
                    if (j > spareMainAreaNumber) break;
                    List<Location> dirs = new List<Location>() { Location.Up, Location.Down, Location.Left, Location.Right };
                    foreach (var dp in info.connectionDP) {
                        dirs.Remove(dp);
                    }
                    AddAreaInfo(i, dirs[dirs.Count.Random()], info.loc, -1);
                    j++;  
                }
                yield return null;
            }

            // Add special area
            List<AreaType> types = new List<AreaType>();
            foreach (var config in stage.specialConfigs) {
                types.Add(config.type);
            }
            for (int i = 1,j=0; i < Database.Instance.ActiveData.worldMap.Count; i++) {
                int areaIndex = Database.Instance.ActiveData.worldMap.Count - i;
                int chunkIndex = i / Database.Instance.areaChunkSize;
                AreaInfo info = Database.Instance.AreaChunks[chunkIndex].areaInfo[Database.Instance.ActiveData.worldMap[areaIndex]];
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
            for (int i = 0; i < Database.Instance.ActiveData.worldMap.Count; i++) {
                int chunkIndex = i / Database.Instance.areaChunkSize;
                AreaInfo ai = Database.Instance.AreaChunks[chunkIndex].
                    areaInfo[Database.Instance.ActiveData.worldMap[i]];
                if (ai.isInit) continue;
                if (ai.typeID == -1) {
                    GenerateMap(ai, stage.mainConfig);
                }
                if (ai.typeID == -2) {
                    GenerateMap(ai, stage.bossConfig);
                }
                else if (ai.typeID >= 0) {
                    GenerateMap(ai, stage.specialConfigs[ai.typeID]);
                }
                yield return null;
            }
        }

        private void GenerateMap(AreaInfo info,AreaConfig config)
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

            for (int x = 0; x < 32; x++) {
                for (int y = 0; y < 32; y++) {
                    int value = subArea[x / 8, y / 8].coordinateValues[x % 8, y % 8];
                    TileType type = value.Random() < 100 ? (TileType)(value % 100) : TileType.Plain;
                    info.map[x, y] = new TileInfo(type, TemplateTile.Dict[type]);
                }
            }

            foreach (var passage in info.connectionDP) {
                if (passage == Location.Up) {
                    int x = 30.Random() + 1, y = 31;
                    while (info.map[x, y].type != TileType.Plain) {
                        info.map[x, y].type = TileType.Plain;
                        y--;
                    }
                }
                else if(passage == Location.Down) {
                    int x = 30.Random() + 1, y = 0;
                    while (info.map[x, y].type != TileType.Plain) {
                        info.map[x, y].type = TileType.Plain;
                        y++;
                    }
                }
                else if(passage == Location.Left) {
                    int y = 30.Random() + 1, x = 0;
                    while (info.map[x, y].type != TileType.Plain) {
                        info.map[x, y].type = TileType.Plain;
                        x++;
                    }
                }
                else if (passage == Location.Right) {
                    int y = 30.Random() + 1, x = 31;
                    while (info.map[x, y].type != TileType.Plain) {
                        info.map[x, y].type = TileType.Plain;
                        x--;
                    }
                }
            }
            info.isInit = true;
        }

        private AreaInfo AddAreaInfo(int index, Location dir,Location loc,int typeID)
        {
            int chunkIndex = index / Database.Instance.areaChunkSize;
            Location newLoc = loc + dir;
            var info = new AreaInfo() { loc = newLoc, typeID = typeID };

            Database.Instance.AreaChunks[chunkIndex].areaInfo[loc].connectionDP.Add(dir);

            if (!Database.Instance.ActiveData.worldMap.Contains(newLoc)) {
                Database.Instance.ActiveData.worldMap.Add(newLoc);
                Database.Instance.AreaChunks[chunkIndex].areaInfo.Add(newLoc, info);
            }

            info = Database.Instance.AreaChunks[chunkIndex].areaInfo[newLoc];
            info.connectionDP.Add(-dir);
            return info;
        }

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

