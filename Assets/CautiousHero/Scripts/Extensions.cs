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

        public static Entity StayEntity(this Location location) => GridManager.Instance.GetTileController(location).StayEntity;
    }
}