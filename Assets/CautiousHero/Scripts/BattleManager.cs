using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wing.TileUtils;
using Wing.RPGSystem;

public enum BattleState
{
    PlacePlayer,
    Move,
    CastSkill,
    Animate,
    NonInteractable  
}

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    [Header("Test")]
    public BaseSkill[] skills;
    public BaseCreature[] creatures;

    [Header("Component References")]
    public LayerMask tileLayer;
    public LayerMask entityLayer;
    public PlayerController player;
    public CreatureController enemy;

    public BattleState State { get; private set; }

    private GameObject tmpPlayerVisual;
    private TileNavigation m_astar;
    private List<Location> currentSelected = new List<Location>();
    private int selectedSkillID = 0;

    private HashSet<Location> tileZone = new HashSet<Location>();

    private void Awake()
    {
        if (!Instance)
            Instance = this;
        State = BattleState.PlacePlayer;
        GridManager.Instance.onCompleteMapRenderEvent += SetAstarNavigation;
    }

    public void SetAstarNavigation(MapGenerator generator)
    {
        m_astar = new TileNavigation(generator.width, generator.height, generator.map);
        PreparePlacePlayer();
        enemy.InitCreature(creatures[0], GridManager.Instance.GetRandomTile());
        enemy.Sprite.color = Color.white;
    }

    private void ChangeState(BattleState state)
    {
        State = state;
        GridManager.Instance.ResetAllTiles();
        currentSelected.Clear();
        tileZone.Clear();
        player.SetActiveCollider(true);
    }

    private void CompletePlacement()
    {
        ChangeState(BattleState.Move);       
    }

    private void CompleteMovement()
    {
        ChangeState(BattleState.CastSkill);
    }

    private void CompleteCast()
    {
        var skill = skills[selectedSkillID];

        /***************************************************************************
         * if not cooldown, return 
         ***************************************************************************/

        for (int i = 0; i < currentSelected.Count; i++) {
            TileController tc = GridManager.Instance.GetTileController(currentSelected[i]);
            if (!tc.isEmpty)
                skill.ApplyEffect(player, tc.stayEntity, i);
        }
        ChangeState(BattleState.Animate);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
            Button_CastSkill(0);
        if (Input.GetKeyDown(KeyCode.W))
            Button_CastSkill(1);
        if (Input.GetKeyDown(KeyCode.E))
            Button_CastSkill(2);
        if (Input.GetKeyDown(KeyCode.R))
            Button_CastSkill(3);

        var ray = Camera.main.ViewportPointToRay(new Vector3(Input.mousePosition.x / Screen.width,
            Input.mousePosition.y / Screen.height, Input.mousePosition.z));
        //Debug.DrawRay(ray.origin,10* ray.direction,Color.red,10);
        var hit = Physics2D.Raycast(ray.origin, ray.direction, 20, tileLayer);
        if (hit) {
            var tile = hit.transform.parent.GetComponent<TileController>();
            switch (State) {
                case BattleState.PlacePlayer:
                    SelectVisual(tile, TileState.PlaceZone);

                    if (Input.GetMouseButtonDown(0)) {
                        player.Sprite.color = Color.white;
                        player.MoveToTile(tile, null);
                        CompletePlacement();
                    }
                    break;
                case BattleState.Move:
                    SelectVisual(tile, TileState.MoveZone);

                    if (Input.GetMouseButtonDown(0) && tileZone.Count != 0) {
                        tmpPlayerVisual.SetActive(false);
                        player.Sprite.color = Color.white;
                        player.MoveToTile(tile, m_astar.GetPath(player.Loc, tile.Loc), true);
                        CompleteMovement();
                    }
                    break;
                case BattleState.CastSkill:
                    SelectVisual(tile, TileState.CastZone);

                    if (Input.GetMouseButtonDown(0) && currentSelected.Count != 0) {
                        if (tileZone.Contains(tile.Loc) || (tile.isBind && currentSelected[0] == tile.CastLoc.Loc))
                            CompleteCast();
                    }
                    break;
                case BattleState.Animate:
                    break;
                case BattleState.NonInteractable:
                    break;
                default:
                    break;
            }         
        }

        if (tileZone.Count == 0) {
            hit = Physics2D.Raycast(ray.origin, ray.direction, 20, entityLayer);
            if (hit && hit.transform.CompareTag("Player")) {
                player.ChangeOutlineColor(Color.red);
                switch (State) {
                    case BattleState.PlacePlayer:
                        break;
                    case BattleState.Move:
                        if (Input.GetMouseButtonDown(0)) {
                            PrepareMove();
                        }
                        break;
                    case BattleState.CastSkill:
                        break;
                    case BattleState.Animate:
                        break;
                    case BattleState.NonInteractable:
                        break;
                    default:
                        break;
                }
            }
            else if(State!= BattleState.CastSkill){
                player.ChangeOutlineColor(Color.black);
            }
        }
        else {
            hit = Physics2D.Raycast(ray.origin, ray.direction, 20, entityLayer);
            if(hit && hit.transform.CompareTag("Creature")) {
                switch (State) {
                    case BattleState.PlacePlayer:
                        break;
                    case BattleState.Move:
                        break;
                    case BattleState.CastSkill:
                        SelectVisual(hit.transform.parent.GetComponent<CreatureController>().LocateTile, TileState.CastZone);

                        if (Input.GetMouseButtonDown(0) && currentSelected.Count != 0) {
                            CompleteCast();
                        }
                        break;
                    case BattleState.Animate:
                        break;
                    case BattleState.NonInteractable:
                        break;
                    default:
                        break;
                }
            }
        }

    }

    private void PreparePlacePlayer()
    {
        for (int x = 0; x < 2; x++) {
            for (int y = 0; y < GridManager.Instance.MapBoundingBox.y; y++) {
                Location loc = new Location(x, y);
                GridManager.Instance.ChangeTileState(loc, TileState.PlaceZone);
                tileZone.Add(loc);
            }
        }
    }

    private void PrepareMove()
    {
        player.SetActiveCollider(false);
        if (!tmpPlayerVisual)
            tmpPlayerVisual = Instantiate(player.Sprite.gameObject, player.transform.position + new Vector3(0, 0.2f, 0), Quaternion.identity);   

        player.Sprite.color = new Color(1, 1, 1, 0.5f);
        for (int x = -player.ActionPoints; x < player.ActionPoints + 1; x++) {
            for (int y = -player.ActionPoints; y < player.ActionPoints + 1; y++) {
                if (Mathf.Abs(x) + Mathf.Abs(y) <= player.ActionPoints) {
                    Location loc = new Location(player.Loc.x + x, player.Loc.y + y);
                    GridManager.Instance.ChangeTileState(loc, TileState.MoveZone);
                    tileZone.Add(loc);
                }
            }
        }
    }

    private void PrepareSkill()
    {
        player.SetActiveCollider(false);

        switch (skills[selectedSkillID].skillType) {
            case SkillType.Instant:
                foreach (var point in skills[selectedSkillID].castPoints) {
                    var loc = player.Loc + point;
                    GridManager.Instance.ChangeTileState(loc, TileState.CastZone);
                    tileZone.Add(loc);
                }
                break;
            case SkillType.Trajectory:
                foreach (var castPoint in skills[selectedSkillID].castPoints) {
                    var castLoc = player.Loc + castPoint;
                    if (!GridManager.Instance.IsLocationValid(castLoc))
                        continue;

                    int xDir = castPoint.x >= 0 ? 1 : -1;
                    int yDir = castPoint.y >= 0 ? 1 : -1;
                    bool exchange = castPoint.x != 0;
                    Location fixedPoint;
                    foreach (var dir in skills[selectedSkillID].AffectPoints()) {
                        if (exchange) {
                            fixedPoint = new Location(xDir * dir.y, dir.x);
                        }
                        else {
                            fixedPoint = new Location(dir.x, yDir * dir.y);
                        }

                        var hitPath = GridManager.Instance.GetTrajectoryHitTile(castLoc, fixedPoint, false);
                        foreach (var passTile in hitPath) {
                            passTile.BindCastLocation(GridManager.Instance.GetTileController(castLoc));
                        }
                    }
                }
                break;
            default:
                break;
        }

    }

    private void SelectVisual(TileController tile, TileState stateZone)
    {
        if (currentSelected.Count != 0) {
            if (currentSelected[0] == tile.Loc || (tile.isBind && currentSelected[0] == tile.CastLoc.Loc))
                return;
                
            foreach (var selected in currentSelected) {
                if (tileZone.Contains(selected)) {
                    GridManager.Instance.ChangeTileState(selected, stateZone);
                }
                else {
                    GridManager.Instance.ChangeTileState(selected, TileState.Normal);
                }
            }
            currentSelected.Clear();
        }

        if (tileZone.Contains(tile.Loc)) {
            if (stateZone == TileState.MoveZone) {
                tmpPlayerVisual.transform.position = tile.Archor;
            }

            currentSelected.Add(tile.Loc);
            switch (stateZone) {
                case TileState.PlaceZone:
                case TileState.MoveZone:
                    tile.ChangeTileState(stateZone + 1);
                    break;
                case TileState.CastZone:
                    HighlightAffectPoints(tile);
                    break;
                default:
                    break;
            }
        }
        else {
            if (tile.isBind && skills[selectedSkillID].skillType == SkillType.Trajectory)
                HighlightAffectPoints(tile.CastLoc);
            else
                currentSelected.Add(tile.Loc);
        }
    }

    public void HighlightAffectPoints(TileController tile)
    {
        var deltaLoc = tile.Loc - player.Loc;
        int xDir = deltaLoc.x >= 0 ? 1 : -1;
        int yDir = deltaLoc.y >= 0 ? 1 : -1;
        bool exchange = deltaLoc.x != 0;
        Location fixedPoint;

        switch (skills[selectedSkillID].skillType) {
            case SkillType.Instant:
                foreach (var point in skills[selectedSkillID].AffectPoints()) {                    
                    if (exchange) {
                        fixedPoint = new Location(xDir * point.y, point.x);
                    }
                    else {
                        fixedPoint = new Location(point.x, yDir * point.y);
                    }

                    var tileLoc = tile.Loc + fixedPoint;
                    if (GridManager.Instance.ChangeTileState(tileLoc, TileState.CastSelected)) {
                        currentSelected.Add(tileLoc);
                    }
                }
                break;
            case SkillType.Trajectory:
                foreach (var point in skills[selectedSkillID].AffectPoints()) {
                    if (exchange) {
                        fixedPoint = new Location(xDir * point.y, point.x);
                    }
                    else {
                        fixedPoint = new Location(point.x, yDir * point.y);
                    }

                    var hitPath = GridManager.Instance.GetTrajectoryHitTile(tile.Loc, fixedPoint, true);
                    foreach (var passTile in hitPath) {
                        currentSelected.Add(passTile.Loc);
                    }
                }
                break;
            default:
                break;
        }
    }

    public void Button_CastSkill(int id)
    {
        if (State != BattleState.Move && State != BattleState.CastSkill)
            return;
        selectedSkillID = id;
        ChangeState(BattleState.CastSkill);
        PrepareSkill();
    }

    private void FixedUpdate()
    {
        
    }
}
