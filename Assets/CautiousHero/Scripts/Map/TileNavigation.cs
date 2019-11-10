using System;
using Priority_Queue;
using System.Collections.Generic;

namespace Wing.TileUtils
{
    public class TileNavigation
    {
        SquareGrid grid;

        public TileNavigation() { }

        public TileNavigation(int width,int height,int[,] map)
        {
            grid = new SquareGrid(width, height);
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    grid.SetWeight(new Location(x, y), map[x, y]);
                }
            }
        }

        public Location[] GetPath(Location from, Location to)
        {
            var astar = new AStarSearch(grid, from, to);
            var path = new Location[astar.cameFrom.Count];

            Location tmp = to;
            for (int i = 0; i < astar.cameFrom.Count; i++) {               
                tmp = astar.cameFrom[tmp];
                path[astar.cameFrom.Count - i - 1] = tmp;
            }

            return path;
        }
    }

    public interface WeightedGraph<L>
    {
        int Cost(Location a, Location b);
        IEnumerable<Location> Neighbors(Location id);
    }

    public class SquareGrid : WeightedGraph<Location>
    {
        public int Width  { get; }
        public int Height { get; }

        private Dictionary<Location, int> weights 
            = new Dictionary<Location, int>();
        private int defaultWeight;

        public SquareGrid(int width, int height, int defaultWeight = 1)
        {
            this.Width = width;
            this.Height = height;

            this.defaultWeight = defaultWeight;
            for (int x = 0; x < Width; x++) {
                for (int y = 0; y < Height; y++) {
                    weights.Add(new Location(x, y), defaultWeight);
                }
            }
        }

        public void SetWeight(Location id, int weight)
        {
            weights[id] = weight;
        }

        public bool InBounds(Location id)
        {
            return 0 <= id.x && id.x < Width
                && 0 <= id.y && id.y < Height;
        }

        // Negative number or zero seen as block
        public bool Passable(Location id)
        {
            return weights[id] <= 0;
        }

        public int Cost(Location from, Location to)
        {
            return weights[to];
        }

        public IEnumerable<Location> Neighbors(Location id)
        {
            foreach (var dir in Patterns.Cross) {
                Location next = new Location(id.x + dir.x, id.y + dir.y);
                if (InBounds(next) && Passable(next)) {
                    yield return next;
                }
            }
        }
    }

    public class AStarSearch
    {
        public Dictionary<Location, Location> cameFrom
            = new Dictionary<Location, Location>();
        public Dictionary<Location, int> costSoFar
            = new Dictionary<Location, int>();

        private Location start, goal;

        // Note: a generic version of A* would abstract over Location and
        // also Heuristic
        static public int Heuristic(Location a, Location b)
        {
            return Math.Abs(a.x - b.x) + Math.Abs(a.y - b.y);
        }

        public AStarSearch(WeightedGraph<Location> graph, Location start, Location goal)
        {
            this.start = start;
            this.goal = goal;
            var frontier = new SimplePriorityQueue<Location>();
            frontier.Enqueue(start, 0);

            cameFrom[start] = start;
            costSoFar[start] = 0;

            while (frontier.Count > 0) {
                var current = frontier.Dequeue();

                if (current.Equals(goal)) {
                    break;
                }

                foreach (var next in graph.Neighbors(current)) {
                    int newCost = costSoFar[current] + graph.Cost(current, next);
                    if (!costSoFar.ContainsKey(next)
                        || newCost < costSoFar[next]) {
                        costSoFar[next] = newCost;
                        float priority = newCost + Heuristic(next, goal);
                        frontier.Enqueue(next, priority);
                        cameFrom[next] = current;
                    }
                }
            }
        }
    }
}

