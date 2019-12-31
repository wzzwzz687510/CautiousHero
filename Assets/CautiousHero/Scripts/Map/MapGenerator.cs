using System;
using System.Collections.Generic;
using UnityEngine;

namespace Wing.RPGSystem
{
    public class MapGenerator : MonoBehaviour
    {
        [Header("Map Parameters")]
        public bool hasEmptyTile = true;
        public bool connectIsland = true;
        public int width = 50;
        public int height = 50;
        public int AirThresholdSize = 10;
        public int blockThresholdSize = 10;
        public int passageWidth = 4;
        public int smoothTimes = 4;
        [Range(30, 60)]
        public int randomFillPercent = 45;
        public int[,] map { get; private set; }
        private int terrainTypeCount = 1;

        public void GenerateMap(int terrainTC)
        {
            terrainTypeCount = terrainTC;
            map = new int[width, height];
            RandomFillMap();

            for (int i = 0; i < smoothTimes; i++) {
                SmoothMap();
            }

            if (hasEmptyTile) {
                ProcessMap();
                RandomTerrainMap();
                //for (int i = 0; i < 1; i++) {
                //    SmoothMap(true);
                //}
            }

            if (hasEmptyTile) {
                Location evil = new Location(width.Random(), height.Random());
                int r = 5;
                for (int x = -r; x <= r; x++) {
                    for (int y = -r; y <= r; y++) {
                        if (x * x + y * y <= r * r) {
                            int drawX = evil.x + x;
                            int drawY = evil.y + y;
                            if (IsInMapRange(drawX, drawY)) {
                                map[drawX, drawY] = -1;
                            }
                        }
                    }
                }
            }

        }

        private void RandomTerrainMap()
        {
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    if (map[x, y] == 1) {
                        map[x, y] = Database.Instance.Random(1, terrainTypeCount + 1);
                    }
                }
            }
        }

        private void ProcessMap()
        {
            List<List<Location>> airRegions = GetRegions(0);

            foreach (List<Location> landRegion in airRegions) {
                if (landRegion.Count < AirThresholdSize) {
                    foreach (Location tile in landRegion) {
                        map[tile.x, tile.y] = 0;
                    }
                }
            }

            List<List<Location>> landRegions = GetRegions(1);
            List<LandBlock> survivingLandBlock = new List<LandBlock>();

            foreach (List<Location> landRegion in landRegions) {
                if (landRegion.Count < blockThresholdSize) {
                    foreach (Location tile in landRegion) {
                        map[tile.x, tile.y] = 0;
                    }
                }
                else {
                    survivingLandBlock.Add(new LandBlock(landRegion, map));
                }
            }

            survivingLandBlock.Sort();
            survivingLandBlock[0].isMainBlock = true;
            survivingLandBlock[0].isAccessibleFromMainBlock = true;

            //foreach (var block in survivingLandBlock) {
            //    Debug.Log("block cnt: " + block.blockSize);
            //}

            if (connectIsland)
                ConnectClosestBlocks(survivingLandBlock);
        }

        private void ConnectClosestBlocks(List<LandBlock> allBlocks, bool forceAccessibilityFromMainBlock = false)
        {
            List<LandBlock> blockListA = new List<LandBlock>();
            List<LandBlock> blockListB = new List<LandBlock>();

            if (forceAccessibilityFromMainBlock) {
                foreach (LandBlock block in allBlocks) {
                    if (block.isAccessibleFromMainBlock) {
                        blockListB.Add(block);
                    }
                    else {
                        blockListA.Add(block);
                    }
                }
            }
            else {
                blockListA = allBlocks;
                blockListB = allBlocks;
            }

            int bestDistance = 0;
            Location bestTileA = new Location();
            Location bestTileB = new Location();
            LandBlock bestBlockA = new LandBlock();
            LandBlock bestBlockB = new LandBlock();
            bool possibleConnectionFound = false;

            foreach (LandBlock blockA in blockListA) {
                if (!forceAccessibilityFromMainBlock) {
                    possibleConnectionFound = false;
                    if (blockA.connectedBlocks.Count > 0) {
                        continue;
                    }
                }

                foreach (LandBlock blockB in blockListB) {
                    if (blockA == blockB || blockA.IsConnected(blockB)) {
                        continue;
                    }

                    for (int tileIndexA = 0; tileIndexA < blockA.edgeTiles.Count; tileIndexA++) {
                        for (int tileIndexB = 0; tileIndexB < blockB.edgeTiles.Count; tileIndexB++) {
                            Location tileA = blockA.edgeTiles[tileIndexA];
                            Location tileB = blockB.edgeTiles[tileIndexB];
                            int distanceBetweenBlocks = (int)(Mathf.Pow(tileA.x - tileB.x, 2) + Mathf.Pow(tileA.y - tileB.y, 2));

                            if (distanceBetweenBlocks < bestDistance || !possibleConnectionFound) {
                                bestDistance = distanceBetweenBlocks;
                                possibleConnectionFound = true;
                                bestTileA = tileA;
                                bestTileB = tileB;
                                bestBlockA = blockA;
                                bestBlockB = blockB;
                            }
                        }
                    }
                }
                if (possibleConnectionFound && !forceAccessibilityFromMainBlock) {
                    CreatePassage(bestBlockA, bestBlockB, bestTileA, bestTileB);
                }
            }

            if (possibleConnectionFound && forceAccessibilityFromMainBlock) {
                CreatePassage(bestBlockA, bestBlockB, bestTileA, bestTileB);
                ConnectClosestBlocks(allBlocks, true);
            }

            if (!forceAccessibilityFromMainBlock) {
                ConnectClosestBlocks(allBlocks, true);
            }
        }

        private void CreatePassage(LandBlock blockA, LandBlock blockB, Location tileA, Location tileB)
        {
            LandBlock.ConnectBlocks(blockA, blockB);

            //Debug.DrawLine (CoordToWorldPoint (tileA), CoordToWorldPoint (tileB), Color.green, 3);

            List<Location> line = GetLine(tileA, tileB);
            foreach (Location c in line) {
                DrawCircle(c, passageWidth);
            }
        }

        public static Vector3 LocationToWorldPoint(Location tile)
        {
            return new Vector3((tile.x - tile.y) * 0.524f, (tile.x + tile.y) * -0.262f, 0);
        }

        public static Location WorldPointToLocation(Vector3 point)
        {
            float a = point.x / 0.524f;
            float b = point.y / -0.262f;
            return new Location((int)(a + b) / 2, (int)(b - a) / 2);
        }

        private void DrawCircle(Location c, int r)
        {
            for (int x = -r; x <= r; x++) {
                for (int y = -r; y <= r; y++) {
                    if (x * x + y * y <= r * r) {
                        int drawX = c.x + x;
                        int drawY = c.y + y;
                        if (IsInMapRange(drawX, drawY)) {
                            map[drawX, drawY] = 1;
                        }
                    }
                }
            }
        }

        private List<Location> GetLine(Location from, Location to)
        {
            List<Location> line = new List<Location>();

            int x = from.x;
            int y = from.y;

            int dx = to.x - from.x;
            int dy = to.y - from.y;

            bool inverted = false;
            int step = Math.Sign(dx);
            int gradientStep = Math.Sign(dy);

            int longest = Mathf.Abs(dx);
            int shortest = Mathf.Abs(dy);

            if (longest < shortest) {
                inverted = true;
                longest = Mathf.Abs(dy);
                shortest = Mathf.Abs(dx);

                step = Math.Sign(dy);
                gradientStep = Math.Sign(dx);
            }

            int gradientAccumulation = longest / 2;
            for (int i = 0; i < longest; i++) {
                line.Add(new Location(x, y));

                if (inverted) {
                    y += step;
                }
                else {
                    x += step;
                }

                gradientAccumulation += shortest;
                if (gradientAccumulation >= longest) {
                    if (inverted) {
                        x += gradientStep;
                    }
                    else {
                        y += gradientStep;
                    }
                    gradientAccumulation -= longest;
                }
            }

            return line;
        }

        private List<List<Location>> GetRegions(int tileType)
        {
            List<List<Location>> regions = new List<List<Location>>();
            int[,] mapFlags = new int[width, height];

            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    if (mapFlags[x, y] == 0 && map[x, y] == tileType) {
                        List<Location> newRegion = GetRegionTiles(x, y);
                        regions.Add(newRegion);

                        foreach (Location tile in newRegion) {
                            mapFlags[tile.x, tile.y] = 1;
                        }
                    }
                }
            }

            return regions;
        }

        List<Location> GetRegionTiles(int startX, int startY)
        {
            List<Location> tiles = new List<Location>();
            int[,] mapFlags = new int[width, height];
            int tileType = map[startX, startY];

            Queue<Location> queue = new Queue<Location>();
            queue.Enqueue(new Location(startX, startY));
            mapFlags[startX, startY] = 1;

            while (queue.Count > 0) {
                Location tile = queue.Dequeue();
                tiles.Add(tile);

                for (int x = tile.x - 1; x <= tile.x + 1; x++) {
                    for (int y = tile.y - 1; y <= tile.y + 1; y++) {
                        if (IsInMapRange(x, y) && (y == tile.y || x == tile.x)) {
                            if (mapFlags[x, y] == 0 && map[x, y] == tileType) {
                                mapFlags[x, y] = 1;
                                queue.Enqueue(new Location(x, y));
                            }
                        }
                    }
                }
            }
            return tiles;
        }

        private void SmoothMap(bool isSecSmooth = false)
        {
            if (!isSecSmooth && hasEmptyTile) {
                for (int x = 0; x < width; x++) {
                    for (int y = 0; y < height; y++) {
                        int neighbourAirTiles = GetSurroundingAirCnt(x, y);
                        if (neighbourAirTiles > 4)
                            map[x, y] = 0;
                        else if (neighbourAirTiles < 4)
                            map[x, y] = 1;
                    }
                }
            }
            else {
                // To Optimize
                for (int x = 0; x < width; x++) {
                    for (int y = 0; y < height; y++) {
                        map[x, y] = GetMaxSurroundingTypeIndex(x, y);
                    }
                }
            }
        }

        private int GetSurroundingAirCnt(int tileX, int tileY)
        {
            int neighbourAirCnt = 0;
            for (int neighbourX = tileX - 1; neighbourX <= tileX + 1; neighbourX++) {
                for (int neighbourY = tileY - 1; neighbourY <= tileY + 1; neighbourY++) {
                    if (IsInMapRange(neighbourX, neighbourY)) {
                        if (neighbourX != tileX || neighbourY != tileY) {
                            neighbourAirCnt += map[neighbourX, neighbourY] == 0 ? 1 : 0;
                        }
                    }
                    else {
                        neighbourAirCnt++;
                    }
                }
            }

            return neighbourAirCnt;
        }

        private int GetMaxSurroundingTypeIndex(int tileX, int tileY)
        {
            int surroundType = 1;
            int[] typeCnt = new int[terrainTypeCount + 1];
            for (int neighbourX = tileX - 1; neighbourX <= tileX + 1; neighbourX++) {
                for (int neighbourY = tileY - 1; neighbourY <= tileY + 1; neighbourY++) {
                    if (IsInMapRange(neighbourX, neighbourY)) {
                        if (neighbourX != tileX || neighbourY != tileY) {
                            typeCnt[map[neighbourX, neighbourY]]++;
                        }
                    }
                }
            }

            for (int i = 1; i < terrainTypeCount + 1; i++) {
                surroundType = typeCnt[surroundType] >= typeCnt[i] ? surroundType : i;
            }

            return surroundType;
        }

        private bool IsInMapRange(int x, int y)
        {
            return x >= 0 && x < width && y >= 0 && y < height;
        }

        private void RandomFillMap()
        {
            if (hasEmptyTile) {
                for (int x = 0; x < width; x++) {
                    for (int y = 0; y < height; y++) {
                        if (x == 0 || x == width - 1 || y == 0 || y == height - 1) {
                            map[x, y] = 0;
                        }
                        else {
                            map[x, y] = 100.Random() < randomFillPercent ? 0 : 1;
                        }
                    }
                }
            }
            else {
                for (int x = 0; x < width; x++) {
                    for (int y = 0; y < height; y++) {
                        map[x, y] = Database.Instance.Random(1, terrainTypeCount + 1);
                    }
                }
            }
        }
    }

    [System.Serializable]
    public struct Location
    {
        public int x, y;

        public Location(Location loc)
        {
            x = loc.x;
            y = loc.y;
        }

        public Location(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public override bool Equals(object obj)
        {
            if (this.GetHashCode() != obj.GetHashCode())
                return false;
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            unchecked {
                return (x * 413) ^ y;
            }
        }

        public override string ToString()
        {
            return "(" + x + ", " + y + ")";
        }

        public static Location operator -(Location a) => new Location(-a.x, -a.y);
        public static Location operator +(Location a, Location b) => new Location(a.x + b.x, a.y + b.y);
        public static Location operator -(Location a, Location b) => a + (-b);
        public static bool operator ==(Location a, Location b) => a.Equals(b);
        public static bool operator !=(Location a, Location b) => !a.Equals(b);
        public static implicit operator Vector3(Location a) => MapGenerator.LocationToWorldPoint(a);
        public static explicit operator Location(Vector3 a) => MapGenerator.WorldPointToLocation(a);

    }

    class LandBlock : IComparable<LandBlock>
    {
        public List<Location> tiles;
        public List<Location> edgeTiles;
        public List<LandBlock> connectedBlocks;
        public int blockSize;
        public bool isAccessibleFromMainBlock;
        public bool isMainBlock;

        public LandBlock()
        {
        }

        public LandBlock(List<Location> blockTiles, int[,] map)
        {
            tiles = blockTiles;
            blockSize = tiles.Count;
            connectedBlocks = new List<LandBlock>();

            edgeTiles = new List<Location>();
            foreach (Location tile in tiles) {
                for (int x = tile.x - 1; x <= tile.x + 1; x++) {
                    for (int y = tile.y - 1; y <= tile.y + 1; y++) {
                        if (x == tile.x || y == tile.y) {
                            if (map[x, y] == 0) {
                                edgeTiles.Add(tile);
                            }
                        }
                    }
                }
            }
        }

        public void SetAccessibleFromMainBlock()
        {
            if (!isAccessibleFromMainBlock) {
                isAccessibleFromMainBlock = true;
                foreach (LandBlock connectedBlock in connectedBlocks) {
                    connectedBlock.SetAccessibleFromMainBlock();
                }
            }
        }

        public static void ConnectBlocks(LandBlock blockA, LandBlock blockB)
        {
            if (blockA.isAccessibleFromMainBlock) {
                blockB.SetAccessibleFromMainBlock();
            }
            else if (blockB.isAccessibleFromMainBlock) {
                blockA.SetAccessibleFromMainBlock();
            }
            blockA.connectedBlocks.Add(blockB);
            blockB.connectedBlocks.Add(blockA);
        }

        public bool IsConnected(LandBlock otherBlock)
        {
            return connectedBlocks.Contains(otherBlock);
        }

        public int CompareTo(LandBlock otherBlock)
        {
            return otherBlock.blockSize.CompareTo(blockSize);
        }
    }
}

