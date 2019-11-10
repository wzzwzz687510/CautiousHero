namespace Wing.TileUtils
{
    public static class Patterns
    {
        public static readonly Location[] Cross = {
                                 new Location(+0, -1),
            new Location(-1, +0),                     new Location(+1, +0),
                                 new Location(+0, +1)
        };

        public static readonly Location[] X = {
            new Location(-1, -1),                     new Location(+1, -1),

            new Location(-1, +1),                     new Location(+1, +1)
        };

        public static readonly Location[] Round = {
            new Location(-1, -1),new Location(+0, -1),new Location(+1, -1),
            new Location(-1, +0),                     new Location(+1, +0),
            new Location(-1, +1),new Location(+0, +1),new Location(+1, +1)
        };
    }
}