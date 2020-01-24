using System;
using System.Collections.Generic;
using UnityEngine;

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

        public delegate Vector3 ToPositionMethods(Location loc);
        public static ToPositionMethods[] LocationToPositionMethods = {
            PositionToWorldView,
            PositionToAreaView
        };
        public static Vector3 PositionToWorldView(Location loc) 
            => new Vector3((loc.x - loc.y) * -0.524f, (loc.x + loc.y) * 0.262f, 0);
        public static Vector3 PositionToAreaView(Location loc)
            => new Vector3(loc.x + 100, loc.y + 100, 0);
        public static Vector3 ToPosition(this Location location)
            => LocationToPositionMethods[WorldMapManager.Instance.IsWorldView ? 0 : 1](location);

        public delegate Location ToLocationMethods(Vector3 pos);
        public static ToLocationMethods[] ViewToLocationMethods = {
            WorldViewToPosition,
            AreaViewToPosition
        };
        public static Location WorldViewToPosition(Vector3 pos)
        {
            float a = pos.x / -0.524f;
            float b = pos.y / 0.262f;
            return new Location((int)(a + b) / 2, (int)(b - a) / 2);
        }
        public static Location AreaViewToPosition(Vector3 pos)
        => new Location((int)pos.x - 100, (int)pos.y - 100);
        public static Location WorldViewToLocation(this Vector3 position)
        => ViewToLocationMethods[WorldMapManager.Instance.IsWorldView ? 0 : 1](position);


        public static bool IsValid(this Location location) => GridManager.Instance.TileDic.ContainsKey(location);

        public static bool IsUnblocked(this Location location) => GridManager.Instance.IsUnblockedLocation(location);

        public static Location GetNearestUnblockedLocation(this Location to,Location from)
        {
            Location loc = to;
            if (from.x != to.x) {
                loc = to.x - from.x > 0 ? loc + Location.Left : loc + Location.Right;
            }
            if (from.y != to.y) {
                loc = to.y - from.y > 0 ? loc + Location.Down : loc + Location.Up;
            }

            return loc.IsUnblocked() ? loc : loc.GetNearestUnblockedLocation(from);
        }

        public static bool TryGetTileController(this Location location, out TileController tc) 
            => GridManager.Instance.TileDic.TryGetValue(location, out tc);

        public static TileController GetTileController(this Location location) => GridManager.Instance.TileDic[location];

        public static bool TryGetStayEntity(this Location location, out Entity entity) {
            if(location.TryGetTileController(out TileController tc)) {
                if (!tc.IsEmpty) {
                    entity = tc.StayEntity;
                    return true;
                }                            
            }
            entity = null;
            return false;
        }

        public static int Distance(this Location location, Location loc) 
            => Math.Abs(location.x - loc.x) + Math.Abs(location.y - loc.y);

        public static bool HasPath(this Location from, Location to) => GridManager.Instance.Nav.HasPath(from, to);

        public static Location[] GetPath(this Location from, Location to) => GridManager.Instance.Nav.GetPath(from, to).ToArray();

        public static IEnumerable<Location> GetGivenDistancePoints(this Location target, int step, bool includeInside = true)
            => GridManager.Instance.Nav.GetGivenDistancePoints(target, step, includeInside);

        public static Location GetLocationWithGivenStep(this Location from, Location to, int step)
            => GridManager.Instance.Nav.GetLocationWithGivenStep(from, to, step);

        public static AreaConfig GetAreaConfig(this int hash) => AreaConfig.Dict[hash];

        public static BaseSkill GetBaseSkill(this int hash) => BaseSkill.Dict[hash];

        public static BaseBuff GetBaseBuff(this int hash) => BaseBuff.Dict[hash];

        public static TRace GetTRace(this int hash) => TRace.Dict[hash];

        public static TRace GetTRaceFromID(this int selectID) => TRace.Dict[Database.Instance.ActivePlayerData.unlockedRaces[selectID]];

        public static TClass GetTClass(this int hash) => TClass.Dict[hash];

        public static TClass GetTClassFromID(this int selectID) => TClass.Dict[Database.Instance.ActivePlayerData.unlockedClasses[selectID]];

        public static TTile GetTTile(this int hash) => TTile.Dict[hash];

        public static CreatureSet GetCreatureSet(this int hash) => CreatureSet.Dict[hash];

        public static Entity GetEntity(this int hash) {
            if (!EntityManager.Instance.EntityDic.TryGetValue(hash, out Entity entity))
                Debug.LogError("Entity manager do not have given hash: " + hash + " entity.");
            return entity;
        }

        public static BaseRelic GetRelic(this int hash) => BaseRelic.Dict[hash];

        public static Color SetAlpha(this Color c, float a)
        {
            return new Color(c.r, c.g, c.b, a);
        }
    }
}