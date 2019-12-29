using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Wing.RPGSystem
{
    public enum PatternType
    {
        Fill,
        Cross,
        X
    }

    //[CreateAssetMenu(fileName = "Pattern", menuName = "Wing/Scriptable Patterns/Base Pattern", order = 1)]
    [System.Serializable]
    public class ScriptablePattern 
    {
        public PatternType type;
        public int size;
        public int emptySize;

        public Location[] CalculateLocation()
        {
            List<Location> locs = new List<Location>();
            switch (type) {
                case PatternType.Fill:
                    for (int x = -size + 1; x < size; x++) {
                        for (int y = -size + 1; y < size; y++) {
                            int distance = Mathf.Abs(x) + Mathf.Abs(y);
                            if (distance < size && distance > emptySize)
                                locs.Add(new Location(x, y));
                        }
                    }
                    break;
                case PatternType.Cross:
                    for (int x = -size + 1; x < size; x++) {
                        for (int y = -size + 1; y < size; y++) {
                            if ((x == 0 || y == 0) && (Mathf.Abs(x) + Mathf.Abs(y)) > emptySize)
                                locs.Add(new Location(x, y));
                        }
                    }
                    break;
                case PatternType.X:
                    for (int x = -size + 1; x < size; x++) {
                        for (int y = -size + 1; y < size; y++) {
                            int distance = Mathf.Abs(x) + Mathf.Abs(y);
                            if (x == y && distance < size && distance > emptySize)
                                locs.Add(new Location(x, y));
                        }
                    }
                    break;
                default:
                    break;
            }

            return locs.ToArray();
        }

        public static readonly Location[] Cross = {
                                 new Location(+0, -1),
            new Location(-1, +0),                     new Location(+1, +0),
                                 new Location(+0, +1)
        };

        static Dictionary<int, Location[]> cache;
        public static Dictionary<int, Location[]> Dict {
            get {
                // load if not loaded yet
                return cache ?? (cache = BaseSkill.Dict.ToDictionary(
                    item => item.Value.Hash, item => item.Value.tCastPatterns.CalculateLocation())
                );
            }
        }
    }
}

