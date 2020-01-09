using System;
using Priority_Queue;
using System.Collections.Generic;

namespace Wing.RPGSystem
{
    public class TileNavigation
    {
        SquareGrid grid;

        public TileNavigation() { }

        public TileNavigation(int width,int height, int defaultWeight)
        {
            grid = new SquareGrid(width, height, defaultWeight);
        }

        public Stack<Location> GetPath(Location from, Location to)
        {            
            Stack<Location> path = new Stack<Location>();


            var astar = new AStarSearch(grid, from, to);

            Location tmp = to;
            while (!tmp.Equals(from)) {
                path.Push(tmp);
                tmp = astar.cameFrom[tmp];                
            }

            return path;
        }

        public bool HasPath(Location from, Location to)
        {
            return new AStarSearch(grid, from, to).cameFrom.ContainsKey(to);
        }

        public Location GetDirectionPattern(Location from, Location to)
        {
            return new AStarSearch(grid, from, to).cameFrom[to] - to;
        }

        public Location GetLocationWithGivenStep(Location from, Location to, int step)
        {
            Location destination = to;
            var astar = new AStarSearch(grid, from, to);
            if (!astar.cameFrom.ContainsKey(to)) return destination;
            int cnt = 0;
            while (!destination.Equals(from)) {
                cnt++;
                destination = astar.cameFrom[destination];
            }
            destination = to;
            for (int i = 0; i < cnt - step; i++) {
                destination = astar.cameFrom[destination];
            }
            return destination;
        }

        public IEnumerable<Location> GetGivenDistancePoints(Location origin, int steps, bool includeInside = true)
        {
            Location destination;
            int heuristic = 0;
            for (int x = -steps; x < steps + 1; x++) {
                for (int y = -steps; y < steps + 1; y++) {
                    heuristic = Math.Abs(x) + Math.Abs(y);
                    if (heuristic <= steps) {
                        destination = new Location(origin.x + x, origin.y + y);
                        if (HasPath(origin, destination)) {
                            int pathCnt = GetPath(origin, destination).Count;
                            if (includeInside ? pathCnt <= steps : pathCnt == steps)
                                yield return destination;
                        }
                    }
                }
            }
        }

        public void SetTileWeight(Location id, int cost)
        {
            grid.SetWeight(id, cost);
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

        public SquareGrid(int width, int height, int defaultWeight)
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
            return weights[id] > 0;
        }

        public int Cost(Location from, Location to)
        {
            return weights[to];
        }

        public IEnumerable<Location> Neighbors(Location id)
        {
            foreach (var dir in ScriptablePattern.Cross) {
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

