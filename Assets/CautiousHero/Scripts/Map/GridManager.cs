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
    public Sprite[] darkArea;

    public bool isRendered { get; private set; }
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

    private void Start()
    {
        m_mg.GenerateMap(tileSprites.Length);
        Astar = new TileNavigation(m_mg.width, m_mg.height, m_mg.map);
        StartCoroutine(RenderMap());       
    }

    private void Update()
    {
        if (!isRendered && Input.GetKeyDown(KeyCode.S)) {
            StartCoroutine(RenderMap());
            isRendered = true;
        }
    }

    private IEnumerator AddAbiotics()
    {
        var config = Database.Instance.config;
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
                        yield return null;
                        break;
                    }
                }
            }
        }
    }

    private IEnumerator RenderMap()
    {
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
        yield return AddAbiotics();
        yield return new WaitForSeconds(2);
        OnCompleteMapRenderEvent?.Invoke();
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

    public void CalculateEntityEffectZone(Entity entity,HashSet< Label> labels,bool isPrediction, out HashSet<Location> EffectZone)
    {
        EffectZone = new HashSet<Location>();

        for (int i = 0; i < entity.Skills.Length; i++) {
            foreach (var label in labels) {
                if (entity.Skills[i].labels.Contains(label)){
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

        for (int i = 0; i < entity.Skills.Length; i++) {
            foreach (var label in labels) {
                if (entity.Skills[i].labels.Contains(label)) {
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
        if (!entity.ActiveSkills[skillID].Castable) yield break;
        var skill = entity.Skills[skillID];
        var zone = new HashSet<Location>();
        int entityAP = isPrediction ? Mathf.Clamp(entity.ActionPoints + entity.ActionPointsPerTurn, 0, 8) : entity.ActionPoints;
        int entityMoveStep = entityAP / entity.MoveCost;
        for (int i = 0; i < entityMoveStep + 1; i++) {
            if (skill.actionPointsCost <= entityAP - i * entity.MoveCost) {
                foreach (var loc in Astar.GetGivenDistancePoints(entity.Loc, i, false)) {
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
        if (!entity.ActiveSkills[skillID].Castable) yield break;
        var skill = entity.Skills[skillID];

        var zone = new HashSet<Location>();
        int entityAP = isPrediction ? Mathf.Clamp(entity.ActionPoints + entity.ActionPointsPerTurn, 0, 8) : entity.ActionPoints;
        int entityMoveStep = entityAP / entity.MoveCost;
        for (int i = 0; i < entityMoveStep + 1; i++) {
            if (skill.actionPointsCost <= entityAP - i * entity.MoveCost) {
                foreach (var loc in Astar.GetGivenDistancePoints(entity.Loc, i, false)) {
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
        var skill = entity.Skills[skillID];
        int availableAP = isPrediction ? Mathf.Clamp(entity.ActionPoints + entity.ActionPointsPerTurn, 0, 8) : entity.ActionPoints - skill.actionPointsCost;
        if (availableAP < 0) yield break;
        for (int i = 0; i < availableAP / entity.MoveCost + 1; i++) {
            foreach (var loc in Astar.GetGivenDistancePoints(entity.Loc, i)) {
                foreach (var cp in skill.CastPatterns) {
                    foreach (var el in skill.GetSubEffectZone(loc, cp)) {
                        if (el.Equals(target)) {
                            yield return new CastSkillAction(GetTileController(loc), loc + cp);
                        }
                    }
                }
            }
        }
    }

    public bool TryGetTileOutsideZone(Entity self, Entity caster,HashSet<Location> zone,out TileController safeTile)
    {
        foreach (var reachableLoc in Astar.GetGivenDistancePoints(self.Loc, self.ActionPoints/self.MoveCost)) {
            if (!zone.Contains(reachableLoc) && IsEmptyLocation(reachableLoc)) {
                safeTile = GetTileController(reachableLoc);
                return true;
            }
        }

        safeTile = null;
        return false;
    }

    public IEnumerable<Location> TryGetLocationOutsideZone(Entity self, Entity caster, HashSet<Location> zone)
    {
        foreach (var reachableLoc in Astar.GetGivenDistancePoints(self.Loc, self.ActionPoints / self.MoveCost)) {
            if (!zone.Contains(reachableLoc) && IsEmptyLocation(reachableLoc)) {
                yield return reachableLoc;
            }
        }
    }

    public IEnumerable<TileController> ValidTiles()
    {
        foreach (var tile in tileDic.Values) {
            if (tile.IsEmpty)
                yield return tile;
        }
    }

}
