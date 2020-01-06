using System;
using System.Collections.Generic;

namespace Wing.RPGSystem
{
    public static class Extensions
    {

        public static int GetStableHashCode(this string text)
        {
            unchecked {
                int hash = 23;
                foreach (char c in text)
                    hash = hash * 31 + c;
                return hash;
            }
        }

        public static int Random(this int max) => Database.Instance.Random(0, max);

        public static Location Set(this Location location, int x,int y)
        {
            location.x = x; location.y = y;
            return location;
        }
        public static Location Set(this Location location,Location loc)
        {
            location.x = loc.x; location.y = loc.y;
            return location;
        }

        public static bool IsValid(this Location location) => GridManager.Instance.TileDic.ContainsKey(location);

        public static bool IsEmpty(this Location location) => GridManager.Instance.IsEmptyLocation(location);

        public static bool TryGetTileController(this Location location, out TileController tc) => GridManager.Instance.TileDic.TryGetValue(location, out tc);

        public static TileController GetTileController(this Location location) => GridManager.Instance.TileDic[location];

        public static bool TryGetStayEntity(this Location location, out Entity entity) {
            if(location.TryGetTileController(out TileController tc)) {
                entity = tc.StayEntity;
                return true;
            }
            entity = null;
            return false;
        }

        public static int Distance(this Location location, Location loc) => Math.Abs(location.x - loc.x) + Math.Abs(location.y - loc.y);

        public static bool HasPath(this Location from, Location to) => GridManager.Instance.Astar.HasPath(from, to);

        public static IEnumerable<Location> GetGivenDistancePoints(this Location target, int step, bool includeInside = true)
            => GridManager.Instance.Astar.GetGivenDistancePoints(target, step, includeInside);

        public static Location GetLocationWithGivenStep(this Location from, Location to, int step)
            => GridManager.Instance.Astar.GetLocationWithGivenStep(from, to, step);

        public static AreaConfig GetAreaConfig(this int hash) => AreaConfig.Dict[hash];

        public static BaseSkill GetBaseSkill(this int hash) => BaseSkill.Dict[hash];

        public static BaseBuff GetBaseBuff(this int hash) => BaseBuff.Dict[hash];

        public static TRace GetTRace(this int hash) => TRace.Dict[hash];

        public static TClass GetTClass(this int hash) => TClass.Dict[hash];

        public static TTile GetTTile(this int hash) => TTile.Dict[hash];

        public static Entity GetEntity(this int hash) {
            EntityManager.Instance.entityDic.TryGetValue(hash, out Entity entity);
            return entity;
        }

        public static UnityEngine.Color SetAlpha(this UnityEngine.Color c, float a)
        {
            return new UnityEngine.Color(c.r, c.g, c.b, a);
        }
    }
}