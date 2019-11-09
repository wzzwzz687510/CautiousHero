using System;
using System.Collections.Generic;
using UnityEngine;

namespace Wing.TileUtils
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

        [Space]

        public string seed;
        public bool useRandomSeed = true;
        [Range(40, 50)]
        public int randomFillPercent = 45;

        public int[,] map { get; private set; }
        private System.Random pseudoRandom;
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
                for (int i = 0; i < 1; i++) {
                    SmoothMap(true);
                }
            }

        }

        private void RandomTerrainMap()
        {
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    if (map[x, y] == 1) {
                        map[x, y] = pseudoRandom.Next(1, terrainTypeCount + 1);
                    }
                }
            }
        }

        private void ProcessMap()
        {
            List<List<Coord>> airRegions = GetRegions(0);

            foreach (List<Coord> landRegion in airRegions) {
                if (landRegion.Count < AirThresholdSize) {
                    foreach (Coord tile in landRegion) {
                        map[tile.tileX, tile.tileY] = 0;
                    }
                }
            }

            List<List<Coord>> landRegions = GetRegions(1);
            List<LandBlock> survivingLandBlock = new List<LandBlock>();

            foreach (List<Coord> landRegion in landRegions) {
                if (landRegion.Count < blockThresholdSize) {
                    foreach (Coord tile in landRegion) {
                        map[tile.tileX, tile.tileY] = 0;
                    }
                }
                else {
                    survivingLandBlock.Add(new LandBlock(landRegion, map));
                }
            }
            survivingLandBlock.Sort();
            survivingLandBlock[0].isMainBlock = true;
            survivingLandBlock[0].isAccessibleFromMainBlock = true;

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
            Coord bestTileA = new Coord();
            Coord bestTileB = new Coord();
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
                            Coord tileA = blockA.edgeTiles[tileIndexA];
                            Coord tileB = blockB.edgeTiles[tileIndexB];
                            int distanceBetweenBlocks = (int)(Mathf.Pow(tileA.tileX - tileB.tileX, 2) + Mathf.Pow(tileA.tileY - tileB.tileY, 2));

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

        private void CreatePassage(LandBlock blockA, LandBlock blockB, Coord tileA, Coord tileB)
        {
            LandBlock.ConnectBlocks(blockA, blockB);

            Debug.DrawLine (CoordToWorldPoint (tileA), CoordToWorldPoint (tileB), Color.green, 3);

            List<Coord> line = GetLine(tileA, tileB);
            foreach (Coord c in line) {
                DrawCircle(c, passageWidth);
            }
        }

        private Vector3 CoordToWorldPoint(Coord tile)
        {
            return new Vector3((tile.tileX - tile.tileY) * 0.524f, (tile.tileX + tile.tileY) * -0.262f, 0);
        }

        private void DrawCircle(Coord c, int r)
        {
            for (int x = -r; x <= r; x++) {
                for (int y = -r; y <= r; y++) {
                    if (x * x + y * y <= r * r) {
                        int drawX = c.tileX + x;
                        int drawY = c.tileY + y;
                        if (IsInMapRange(drawX, drawY)) {
                            map[drawX, drawY] = 1;
                        }
                    }
                }
            }
        }

        private List<Coord> GetLine(Coord from, Coord to)
        {
            List<Coord> line = new List<Coord>();

            int x = from.tileX;
            int y = from.tileY;

            int dx = to.tileX - from.tileX;
            int dy = to.tileY - from.tileY;

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
                line.Add(new Coord(x, y));

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

        private List<List<Coord>> GetRegions(int tileType)
        {
            List<List<Coord>> regions = new List<List<Coord>>();
            int[,] mapFlags = new int[width, height];

            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    if (mapFlags[x, y] == 0 && map[x, y] == tileType) {
                        List<Coord> newRegion = GetRegionTiles(x, y);
                        regions.Add(newRegion);

                        foreach (Coord tile in newRegion) {
                            mapFlags[tile.tileX, tile.tileY] = 1;
                        }
                    }
                }
            }

            return regions;
        }

        List<Coord> GetRegionTiles(int startX, int startY)
        {
            List<Coord> tiles = new List<Coord>();
            int[,] mapFlags = new int[width, height];
            int tileType = map[startX, startY];

            Queue<Coord> queue = new Queue<Coord>();
            queue.Enqueue(new Coord(startX, startY));
            mapFlags[startX, startY] = 1;

            while (queue.Count > 0) {
                Coord tile = queue.Dequeue();
                tiles.Add(tile);

                for (int x = tile.tileX - 1; x <= tile.tileX + 1; x++) {
                    for (int y = tile.tileY - 1; y <= tile.tileY + 1; y++) {
                        if (IsInMapRange(x, y) && (y == tile.tileY || x == tile.tileX)) {
                            if (mapFlags[x, y] == 0 && map[x, y] == tileType) {
                                mapFlags[x, y] = 1;
                                queue.Enqueue(new Coord(x, y));
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

            for (int i = 0; i < terrainTypeCount + 1; i++) {
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
            if (useRandomSeed) {
                seed = System.DateTime.Now.Millisecond.ToString();
            }

            pseudoRandom = new System.Random(seed.GetHashCode());

            if (hasEmptyTile) {
                for (int x = 0; x < width; x++) {
                    for (int y = 0; y < height; y++) {
                        if (x == 0 || x == width - 1 || y == 0 || y == height - 1) {
                            map[x, y] = 0;
                        }
                        else {
                            map[x, y] = pseudoRandom.Next(0, 100) < randomFillPercent ? 0 : 1;
                        }
                    }
                }
            }
            else {
                for (int x = 0; x < width; x++) {
                    for (int y = 0; y < height; y++) {
                        map[x, y] = pseudoRandom.Next(1, terrainTypeCount + 1);
                    }
                }
            }
        }

        struct Coord
        {
            public int tileX;
            public int tileY;

            public Coord(int x, int y)
            {
                tileX = x;
                tileY = y;
            }
        }

        class LandBlock : IComparable<LandBlock>
        {
            public List<Coord> tiles;
            public List<Coord> edgeTiles;
            public List<LandBlock> connectedBlocks;
            public int blockSize;
            public bool isAccessibleFromMainBlock;
            public bool isMainBlock;

            public LandBlock()
            {
            }

            public LandBlock(List<Coord> blockTiles, int[,] map)
            {
                tiles = blockTiles;
                blockSize = tiles.Count;
                connectedBlocks = new List<LandBlock>();

                edgeTiles = new List<Coord>();
                foreach (Coord tile in tiles) {
                    for (int x = tile.tileX - 1; x <= tile.tileX + 1; x++) {
                        for (int y = tile.tileY - 1; y <= tile.tileY + 1; y++) {
                            if (x == tile.tileX || y == tile.tileY) {
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
}

