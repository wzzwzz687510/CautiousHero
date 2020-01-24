using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wing.RPGSystem;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [Header("Tile Elements")]
    public TileController tilePrefab;
    public Transform tileHolder;
    
    public TileNavigation Nav { get; private set; }
    public Dictionary<Location, TileController> TileDic { get; private set; }

    private List<Location> changedTies;
    private List<Location> exploredTiles;

    private void Awake()
    {
        if (!Instance)
            Instance = this;

        TileDic = new Dictionary<Location, TileController>();
        changedTies = new List<Location>();
        exploredTiles = new List<Location>();
        Nav = new TileNavigation(32, 32, 1);

        for (int x = 0; x < 32; x++) {
            for (int y = 0; y < 32; y++) {
                Location loc = new Location(x, y);
                //Instantiate(tilePrefab, new Vector3(x+100, y+100, 0), Quaternion.identity, tileHolder);
                TileDic.Add(loc, tileHolder.GetChild(x * 32 + y).GetComponent<TileController>());
                TileDic[loc].Init(loc);
            }
        }
    }

    private IEnumerator AddAbiotics()
    {
        //var config = Database.Instance.config;
        //var sets = config.abioticSets;
        //int[] possiblities = new int[sets.Length];
        //possiblities[0] = sets[0].power;
        //for (int i = 1; i < config.abioticSets.Length; i++) {
        //    possiblities[i] = possiblities[i - 1] + sets[i].power;
        //}
        //HashSet<Location> entityLocs = new HashSet<Location>();
        //foreach (var set in Database.Instance.config.creatureSets) {
        //    entityLocs.Add(set.location);
        //}
        //int totalPower = possiblities[sets.Length - 1];
        //float randomFill, randomAbiotic;
        //foreach (var loc in ValidLocations()) {
        //    if (entityLocs.Contains(loc)) continue;
        //    randomFill = 1000.Random() / 1000.0f;
        //    if (randomFill <= config.coverage) {
        //        randomAbiotic = totalPower.Random();
        //        for (int i = 0; i < sets.Length; i++) {
        //            if (randomAbiotic < possiblities[i]) {
        //                Instantiate(abioticPrefab, loc.GetTileController().m_spriteRenderer.transform).GetComponent<AbioticController>().
        //                    InitAbioticEntity(sets[i].tAbiotic, loc);
        //                yield return null;
        //                break;
        //            }
        //        }
        //    }
        //}
        yield return null;
    }

    public void LoadMap()
    {
        exploredTiles.Clear();
        for (int x = 0; x < 32; x++) {
            for (int y = 0; y < 32; y++) {
                Location loc = new Location(x, y);
                TileDic[loc].UpdateSprite();
                if (TileDic[loc].Info.isExplored) exploredTiles.Add(loc);
                Nav.SetTileWeight(loc, TileDic[loc].Info.IsBlocked ? 0 : 1);
            }
        }
    }

    public bool CheckEntrance(Location loc)
    {
        return exploredTiles.Contains(loc);
    }


    public void AddExploredTile(Location loc)
    {
        if (!exploredTiles.Contains(loc)) exploredTiles.Add(loc);
    }

    public void DiscoverTiles(Location loc)
    {
        int heuristic = 0;
        int viewDistance = AreaManager.Instance.playerViewDistance;
        for (int x = -viewDistance; x < viewDistance + 1; x++) {
            for (int y = -viewDistance; y < viewDistance + 1; y++) {
                heuristic = Mathf.Abs(x) + Mathf.Abs(y);
                if (heuristic <= viewDistance) {
                    Location tmp = new Location(loc.x + x, loc.y + y);
                    if(TileDic.ContainsKey(tmp)) AddExploredTile(tmp);                
                }
            }
        }
    }

    public void SaveExplorationState()
    {
        foreach (var tile in exploredTiles) {
            AreaManager.Instance.SetExploration(tile);
        }
    }

    public void ResetAllTiles()
    {
        foreach (var loc in changedTies) {
            TileDic[loc].ChangeTileState(TileState.Normal);
            TileDic[loc].DebindCastLocation();
        }
        changedTies.Clear();
    }

    public bool IsUnblockedLocation(Location id)
    {
        return TileDic.ContainsKey(id) && !TileDic[id].IsBlocked;
    }

    public bool ChangeTileState(Location id, TileState state)
    {
        if(!TileDic.ContainsKey(id))
            return false;

        if (state != TileState.Normal) {
            if (!changedTies.Contains(id))
                changedTies.Add(id);
        }
        else changedTies.Remove(id);

        TileDic[id].ChangeTileState(state);
        return true;
    }

    public Location GetRandomLoc(bool isValid = true)
    {
        int cnt = 0;
        if (isValid) {
            IEnumerator<Location> locEnumerator = ValidLocations().GetEnumerator();
            int random = Random.Range(0, TileDic.Count);
            for (int i = 0; i < random; i++) {
                if (--random < 0)
                    return locEnumerator.Current;
                if (!locEnumerator.MoveNext()) {
                    random = Random.Range(0, cnt);
                    for (int j = 0; j < random; j++) {
                        if (--random < 0)
                            return locEnumerator.Current;
                    }
                }
                cnt++;
            }

        }

        cnt = Random.Range(0, TileDic.Count);
        foreach (var tile in TileDic.Values) {
            if (--cnt < 0)
                return tile.Loc;
        }

        return new Location();
    }

    public IEnumerable<TileController> GetTrajectoryHitTile(Location id, Location dir, bool highlight = false)
    {
        TileController tc;
        if (!TileDic.TryGetValue(id, out tc))
            yield break;
        Location tmp = id;

        // Safe count for exit from while.
        int cnt = 0;
        while (tc.IsEmpty) {
            if (highlight)
                tc.ChangeTileState(TileState.CastZone);
            yield return tc;
            tmp += dir;
            if (!TileDic.TryGetValue(tmp, out tc) || cnt++ > 8)
                yield break;
        }

        if (highlight)
            tc.ChangeTileState(TileState.CastSelected);
        yield return tc;
    }

    public void CalculateEntityEffectZone(Entity entity,HashSet< Label> labels,bool isPrediction, out HashSet<Location> EffectZone)
    {
        EffectZone = new HashSet<Location>();

        for (int i = 0; i < entity.SkillHashes.Count; i++) {
            foreach (var label in labels) {
                if (entity.SkillHashes[i].GetBaseSkill().labels.Contains(label)){
                    foreach (var effectLoc in CalculateEntityGivenSkillEffectZone(entity, isPrediction, i)) {
                        EffectZone.Add(effectLoc);
                    }
                    break;
                }
            }
        }
    }

    public void CalculateEntityEffectZone(Entity entity, HashSet<Label> labels, bool isPrediction, HashSet<Location> avoidZone, out HashSet<Location> EffectZone)
    {
        EffectZone = new HashSet<Location>();

        for (int i = 0; i < entity.SkillHashes.Count; i++) {
            foreach (var label in labels) {
                if (entity.SkillHashes[i].GetBaseSkill().labels.Contains(label)) {
                    foreach (var effectLoc in CalculateEntityGivenSkillEffectZone(entity, isPrediction, i, avoidZone)) {
                        EffectZone.Add(effectLoc);
                    }
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Has Duplication checked
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="isPrediction"></param>
    /// <param name="skill"></param>
    /// <returns></returns>
    public IEnumerable<Location> CalculateEntityGivenSkillEffectZone(Entity entity, bool isPrediction, int skillID)
    {
        var skill = entity.SkillHashes[skillID].GetBaseSkill();
        var zone = new HashSet<Location>();
        int entityAP = isPrediction ? Mathf.Clamp(entity.ActionPoints + entity.ActionPointsPerTurn, 0, 8) : entity.ActionPoints;
        int entityMoveStep = entityAP / entity.MoveCost;
        for (int i = 0; i < entityMoveStep + 1; i++) {
            if (skill.actionPointsCost <= entityAP - i * entity.MoveCost) {
                foreach (var loc in Nav.GetGivenDistancePoints(entity.Loc, i, false)) {
                    foreach (var effectLoc in skill.GetEffectZone(loc)) {
                        if (!zone.Contains(effectLoc)) {
                            zone.Add(effectLoc);
                            yield return effectLoc;
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Has Duplication checked
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="isPrediction"></param>
    /// <param name="skill"></param>
    /// <returns></returns>
    public IEnumerable<Location> CalculateEntityGivenSkillEffectZone(Entity entity, bool isPrediction, int skillID, HashSet<Location> avoidZone)
    {
        var skill = entity.SkillHashes[skillID].GetBaseSkill();

        var zone = new HashSet<Location>();
        int entityAP = isPrediction ? Mathf.Clamp(entity.ActionPoints + entity.ActionPointsPerTurn, 0, 8) : entity.ActionPoints;
        int entityMoveStep = entityAP / entity.MoveCost;
        for (int i = 0; i < entityMoveStep + 1; i++) {
            if (skill.actionPointsCost <= entityAP - i * entity.MoveCost) {
                foreach (var loc in Nav.GetGivenDistancePoints(entity.Loc, i, false)) {
                    if (avoidZone.Contains(loc))
                        continue;
                    foreach (var effectLoc in skill.GetEffectZone(loc)) {
                        if (!zone.Contains(effectLoc)) {
                            zone.Add(effectLoc);
                            yield return effectLoc;
                        }
                    }
                }
            }
        }
    }

    public IEnumerable<CastSkillAction> CalculateCastSkillTile(Entity entity, int skillID, Location target, bool isPrediction = false)
    {
        var skill = entity.SkillHashes[skillID].GetBaseSkill();
        int availableAP = isPrediction ? Mathf.Clamp(entity.ActionPoints + entity.ActionPointsPerTurn, 0, 8) : entity.ActionPoints - skill.actionPointsCost;
        if (availableAP < 0) yield break;
        for (int i = 0; i < availableAP / entity.MoveCost + 1; i++) {
            foreach (var loc in Nav.GetGivenDistancePoints(entity.Loc, i)) {
                foreach (var cp in skill.CastPattern) {
                    foreach (var el in skill.GetSubEffectZone(loc, cp)) {
                        if (el.Equals(target)) {
                            yield return new CastSkillAction(loc, loc + cp);
                        }
                    }
                }
            }
        }
    }

    public bool TryGetTileOutsideZone(Entity self,HashSet<Location> zone,out TileController safeTile)
    {
        foreach (var reachableLoc in Nav.GetGivenDistancePoints(self.Loc, self.ActionPoints/self.MoveCost)) {
            if (!zone.Contains(reachableLoc) && IsUnblockedLocation(reachableLoc)) {
                safeTile = TileDic[reachableLoc];
                return true;
            }
        }

        safeTile = null;
        return false;
    }

    public IEnumerable<Location> TryGetLocationOutsideZone(Entity self, Entity caster, HashSet<Location> zone)
    {
        foreach (var reachableLoc in Nav.GetGivenDistancePoints(self.Loc, self.ActionPoints / self.MoveCost)) {
            if (!zone.Contains(reachableLoc) && IsUnblockedLocation(reachableLoc)) {
                yield return reachableLoc;
            }
        }
    }

    public IEnumerable<Location> ValidLocations()
    {
        foreach (var tile in TileDic.Values) {
            if (tile.IsEmpty)
                yield return tile.Loc;
        }
    }

}
