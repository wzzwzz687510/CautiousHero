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

        public static bool IsValid(this Location location) => GridManager.Instance.IsValidLocation(location);

        public static bool IsEmpty(this Location location) => GridManager.Instance.IsEmptyLocation(location);

        public static TileController GetTileController(this Location location) => GridManager.Instance.GetTileController(location);

        public static Entity GetStayEntity(this Location location) => location.GetTileController().StayEntity;

        public static int GetDistance(this Location location, Location loc) =>Math.Abs(location.x - loc.x) + Math.Abs(location.y - loc.y);

        public static BaseSkill GetBaseSkill(this int hash) => BaseSkill.Dict[hash];

        public static BaseBuff GetBaseBuff(this int hash) => BaseBuff.Dict[hash];

        public static Entity GetEntity(this int hash) => EntityManager.Instance.entityDic[hash];

    }
}