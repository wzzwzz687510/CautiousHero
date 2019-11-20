using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wing.RPGSystem;

public struct CastSkillAction
{
    public TileController destination;
    public Location castLocation;

    public CastSkillAction(TileController moveto, Location castLoc)
    {
        destination = moveto;
        castLocation = castLoc;
    }
}

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [Range(0,0.1f)]   public float animationDuration = 0.01f;

    public TileController tcPrefab;
    public AbioticController abioticPrefab;
    public Sprite[] tileSprites;
    
    public TileNavigation Astar { get; private set; }
    public Vector2 MapBoundingBox { get { return new Vector2(m_mg.width, m_mg.height); } }

    private MapGenerator m_mg;    
    private GameObject tileHolder;
    private Dictionary<Location, TileController> tileDic 
        = new Dictionary<Location, TileController>();
    private HashSet<Location> playerEffectZone = new HashSet<Location>();

    public delegate void OnCompleteMapRendering();
    public event OnCompleteMapRendering OnCompleteMapRenderEvent;

    private void Awake()
    {
        if (!Instance)
            Instance = this;
        m_mg = GetComponent<MapGenerator>();
    }

    private IEnumerator Start()
    {
        RenderMap();       
        Astar = new TileNavigation(m_mg.width, m_mg.height, m_mg.map);
        AddAbiotics();
        yield return new WaitForSeconds(2);
        OnCompleteMapRenderEvent();      
    }

    private void AddAbiotics()
    {
        var config = BattleManager.Instance.config;
        var sets = config.abioticSets;
        int[] possiblities = new int[sets.Length];
        possiblities[0] = sets[0].power;
        for (int i = 1; i < config.abioticSets.Length; i++) {
            possiblities[i] = possiblities[i - 1] + sets[i].power;
        }
        int totalPower = possiblities[sets.Length - 1];
        float randomFill, randomAbiotic;
        foreach (var tile in ValidTiles()) {
            randomFill = 1000.Random() / 1000.0f;
            if (randomFill <= config.coverage) {
                randomAbiotic = totalPower.Random();
                for (int i = 0; i < sets.Length; i++) {
                    if (randomAbiotic < possiblities[i]) {
                        Instantiate(abioticPrefab, tile.m_spriteRenderer.transform).GetComponent<AbioticController>().
                            InitAbioticEntity(sets[i].tAbiotic, tile);
                        break;
                    }
                }
            }
        }
    }

    private void RenderMap()
    {
        m_mg.GenerateMap(tileSprites.Length);

        if (tileHolder)
            Destroy(tileHolder);

        tileHolder = new GameObject("Tile Holder");
        tileDic = new Dictionary<Location, TileController>();

        for (int x = 0; x < m_mg.width; x++) {
            for (int y = 0; y < m_mg.height; y++) {
                if (m_mg.map[x, y] != 0) {
                    var pos = new Location(x, y);
                    TileController tc = Instantiate(tcPrefab, new Vector3(0.524f * (x - y), -0.262f * (x + y), 0), 
                        Quaternion.identity, tileHolder.transform).GetComponent<TileController>();
                    tc.Init_SpriteRenderer(pos, y * m_mg.width + x - m_mg.width * m_mg.height, 
                        m_mg.map[x, y] - 1, UnityEngine.Random.Range(0.01f, 1f));
                    tileDic.Add(pos, tc);
                }
            }
        }
        //Debug.Log("tile cnt:" + tileHolder.transform.childCount);
    }

    public void ResetAllTiles()
    {
        foreach (var tile in tileDic.Values) {
            tile.ChangeTileState(TileState.Normal);
            tile.DebindCastLocation();
        } 
    }

    public bool IsValidLocation(Location id)
    {
        return tileDic.ContainsKey(id);
    }

    /// <summary>
    /// Check whether Location is valid before call this function
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public TileController GetTileController(Location id)
    {
        return tileDic[id];
    }

    public bool IsEmptyLocation(Location id)
    {
        return tileDic[id].IsEmpty;
    }

    public bool ChangeTileState(Location id, TileState state)
    {
        if(!IsValidLocation(id))
            return false;

        GetTileController(id).ChangeTileState(state);
        return true;
    }

    public TileController GetRandomTile(bool isValid = true)
    {
        int cnt = 0;
        if (isValid) {
            var tilesEnumerator = ValidTiles().GetEnumerator();
            int random = Random.Range(0, tileDic.Count);
            for (int i = 0; i < random; i++) {
                if (--random < 0)
                    return tilesEnumerator.Current;
                if (!tilesEnumerator.MoveNext()) {
                    random = Random.Range(0, cnt);
                    for (int j = 0; j < random; j++) {
                        if (--random < 0)
                            return tilesEnumerator.Current;
                    }
                }
                cnt++;
            }

        }

        cnt = Random.Range(0, tileDic.Count);
        foreach (var tile in tileDic.Values) {
            if (--cnt < 0)
                return tile;
        }

        return null;
    }

    public IEnumerable<TileController> GetTrajectoryHitTile(Location id, Location dir, bool highlight = false)
    {
        TileController tc;
        if (!tileDic.TryGetValue(id, out tc))
            yield break;
        Location tmp = id;

        // Safe count for exit from while.
        int cnt = 0;
        while (tc.IsEmpty) {
            if (highlight)
                tc.ChangeTileState(TileState.CastZone);
            yield return tc;
            tmp += dir;
            if (!tileDic.TryGetValue(tmp, out tc) || cnt++ > 8)
                yield break;
        }

        if (highlight)
            tc.ChangeTileState(TileState.CastSelected);
        yield return tc;
    }

    public HashSet<Location> CalculateEntityEffectZone(Entity entity, bool isPrediction)
    {
        HashSet<Location> EffectZone = new HashSet<Location>();
        int entityAP = isPrediction ? entity.ActionPoints + entity.ActionPointsPerTurn : entity.ActionPoints;
        int entityMoveStep = entityAP / entity.MoveCost;
        for (int i = 0; i < entityMoveStep + 1; i++) {
            foreach (var skill in entity.Skills) {
                if (skill.actionPointsCost <= entityAP - i * entity.MoveCost) {
                    foreach (var loc in Astar.GetGivenDistancePoints(entity.Loc, i, false)) {
                        foreach (var effecLoc in skill.GetEffectZone(loc)) {
                            if (!EffectZone.Contains(effecLoc))
                                EffectZone.Add(effecLoc);
                        }
                    }
                }
            }
        }
        return EffectZone;
    }

    public bool CalculateCastSkillTile(Entity entity,int skillID,Location target,out CastSkillAction action)
    {
        action = new CastSkillAction();
        var skill = entity.Skills[skillID];
        int availableAP = entity.ActionPoints - skill.actionPointsCost;
        if (availableAP < 0) return false;
        for (int i = 0; i < availableAP/entity.MoveCost + 1; i++) {
            foreach (var loc in Astar.GetGivenDistancePoints(entity.Loc, i)) {
                foreach (var cp in skill.CastPatterns) {
                    foreach (var el in skill.GetSubEffectZone(loc,cp)) {
                        if (el.Equals(target)) {
                            action.destination = GetTileController(loc);
                            action.castLocation = loc + cp;
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }

    public bool TryGetSafeTile(Entity self, Entity caster, out TileController safeTile)
    {
        var zone = CalculateEntityEffectZone(caster, true);
        foreach (var reachableLoc in Astar.GetGivenDistancePoints(self.Loc, self.ActionPoints/self.MoveCost)) {
            if (!zone.Contains(reachableLoc) && IsEmptyLocation(reachableLoc)) {
                safeTile = GetTileController(reachableLoc);
            }
        }

        safeTile = null;
        return false;
    }

    public IEnumerable<TileController> ValidTiles()
    {
        foreach (var tile in tileDic.Values) {
            if (tile.IsEmpty)
                yield return tile;
        }
    }

}
