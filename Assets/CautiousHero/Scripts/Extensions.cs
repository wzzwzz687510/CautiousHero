using System;

namespace Wing.RPGSystem
{
    public static class Extensions
    {
        // string.GetHashCode is not quaranteed to be the same on all machines, but
        // we need one that is the same on all machines. simple and stupid:
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

        public static bool IsValid(this Location location) => GridManager.Instance.tileDic.ContainsKey(location);

        public static bool IsEmpty(this Location location) => GridManager.Instance.IsEmptyLocation(location);

        public static bool TryGetTileController(this Location location, out TileController tc) => GridManager.Instance.tileDic.TryGetValue(location, out tc);

        public static TileController GetTileController(this Location location) => GridManager.Instance.tileDic[location];

        public static bool TryGetStayEntity(this Location location, out Entity entity) {
            if(location.TryGetTileController(out TileController tc)) {
                entity = tc.StayEntity;
                return true;
            }
            entity = null;
            return false;
        }

        public static int Distance(this Location location, Location loc) =>Math.Abs(location.x - loc.x) + Math.Abs(location.y - loc.y);

        public static bool HasPath(this Location from, Location to) => GridManager.Instance.Astar.HasPath(from, to);

        public static BaseSkill GetBaseSkill(this int hash) => BaseSkill.Dict[hash];

        public static BaseBuff GetBaseBuff(this int hash) => BaseBuff.Dict[hash];

        public static Entity GetEntity(this int hash) {
            EntityManager.Instance.entityDic.TryGetValue(hash, out Entity entity);
            return entity;
        }

    }
}