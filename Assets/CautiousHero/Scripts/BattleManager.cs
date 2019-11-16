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
    
    private List<Location> currentSelected = new List<Location>();
    private int selectedSkillID = 0;

    private HashSet<Location> tileZone = new HashSet<Location>();

    private void Awake()
    {
        if (!Instance)
            Instance = this;
        State = BattleState.PlacePlayer;
        GridManager.Instance.onCompleteMapRenderEvent += PrepareBattleStart;

        // For test
        player.InitPlayer(skills);
        GetComponent<BattleUIController>().UpdateUI();
    }

    public void PrepareBattleStart()
    {       
        PreparePlacePlayer();

        // For test
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
        /***************************************************************************
         * if not cooldown, return 
         ***************************************************************************/
        //HashSet<Location> repeatCheck = new HashSet<Location>();
        //for (int i = 0; i < currentSelected.Count; i++) {
        //    TileController tc = GridManager.Instance.GetTileController(currentSelected[i]);
        //    if (!tc.isEmpty&& !repeatCheck.Contains(tc.Loc)) {
        //        //skill.ApplyEffect(player, tc.stayEntity, skill.effectPatterns[i].coefficient);
                
        //        repeatCheck.Add(tc.Loc);
        //    }               
        //}
        player.CastSkill(selectedSkillID, currentSelected[0]);
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
                        player.MoveToTile(tile,false);
                        player.DropAnimation();
                        CompletePlacement();
                    }
                    break;
                case BattleState.Move:
                    SelectVisual(tile, TileState.MoveZone);

                    if (Input.GetMouseButtonDown(0) && tileZone.Contains(currentSelected[0])) {
                        tmpPlayerVisual.SetActive(false);
                        player.Sprite.color = Color.white;
                        player.MoveToTile(tile, true);
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
        if (!tmpPlayerVisual) {
            tmpPlayerVisual = Instantiate(player.Sprite.gameObject, player.transform.position + new Vector3(0, 0.2f, 0), Quaternion.identity);
            tmpPlayerVisual.GetComponent<SpriteRenderer>().sortingOrder = player.Loc.x + player.Loc.y * 8 + 1;
        }

        player.Sprite.color = new Color(1, 1, 1, 0.5f);
        foreach (var loc in GridManager.Instance.Astar.GetGivenDistancePoints(player.Loc, player.ActionPoints / player.MoveCost)) {
            GridManager.Instance.ChangeTileState(loc, TileState.MoveZone);
            tileZone.Add(loc);
        }
    }

    private void PrepareSkill()
    {
        player.SetActiveCollider(false);

        switch (skills[selectedSkillID].castType) {
            case CastType.Instant:
                foreach (var point in skills[selectedSkillID].castPatterns) {
                    var loc = player.Loc + point;
                    if (GridManager.Instance.ChangeTileState(loc, TileState.CastZone))
                        tileZone.Add(loc);
                }
                break;
            case CastType.Trajectory:
                foreach (var castPattern in skills[selectedSkillID].castPatterns) {
                    var castLoc = player.Loc + castPattern;
                    if (!GridManager.Instance.ChangeTileState(castLoc, TileState.CastZone))
                        continue;
                    tileZone.Add(castLoc);

                    foreach (var pattern in skills[selectedSkillID].GetFixedEffectPatterns(castPattern)) {

                        var hitPath = GridManager.Instance.GetTrajectoryHitTile(castLoc, pattern, false);
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
                    HighlightAffectPoints(tile.Loc);
                    break;
                default:
                    break;
            }
        }
        else {
            if (tile.isBind && skills[selectedSkillID].castType == CastType.Trajectory) {
                if (currentSelected.Count == 0 || currentSelected[0] != tile.CastLoc.Loc)
                    HighlightAffectPoints(tile.CastLoc.Loc);
            }
            else
                currentSelected.Add(tile.Loc);
        }
    }

    public void HighlightAffectPoints(Location castLoc)
    {
        var skill = skills[selectedSkillID];
        switch (skill.castType) {
            case CastType.Instant:
                foreach (var effectLoc in skill.SubEffectZone(player.Loc, castLoc-player.Loc)) {
                    if (GridManager.Instance.ChangeTileState(effectLoc, TileState.CastSelected)) {
                        currentSelected.Add(effectLoc);
                    }
                }
                break;
            case CastType.Trajectory:
                foreach (var effectLoc in skill.SubEffectZone(player.Loc, castLoc - player.Loc, true)) {
                    currentSelected.Add(effectLoc);
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
