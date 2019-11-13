using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wing.TileUtils;

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

    [Header("Component References")]
    public LayerMask tileLayer;
    public LayerMask defaultLayer;
    public PlayerController player;
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

        player.Sprite.color = new Color(1, 1, 1, 0f);
    }

    public void SetAstarNavigation(MapGenerator generator)
    {
        m_astar = new TileNavigation(generator.width, generator.height, generator.map);
        PreparePlacePlayer();
    }

    private void ChangeState(BattleState state)
    {
        State = state;
        GridManager.Instance.ResetTiles();
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
                    SelectVisual(tile,TileState.MoveZone);

                    if (Input.GetMouseButtonDown(0) && tileZone.Count != 0) {
                        tmpPlayerVisual.SetActive(false);
                        player.Sprite.color = Color.white;
                        player.MoveToTile(tile, m_astar.GetPath(player.Loc, tile.Loc), true);
                        CompleteMovement();
                    }
                    break;
                case BattleState.CastSkill:
                    SelectVisual(tile, TileState.CastZone);
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
            hit = Physics2D.Raycast(ray.origin, ray.direction, 20, defaultLayer);
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
        for (int x = -player.MovementPoint; x < player.MovementPoint + 1; x++) {
            for (int y = -player.MovementPoint; y < player.MovementPoint + 1; y++) {
                if (Mathf.Abs(x) + Mathf.Abs(y) <= player.MovementPoint) {
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

        foreach (var point in skills[selectedSkillID].castPoints) {
            var loc = player.Loc + point;
            GridManager.Instance.ChangeTileState(loc, TileState.CastZone);
            tileZone.Add(loc);
        }
    }

    private void SelectVisual(TileController tile, TileState stateZone)
    {
        if (currentSelected.Count != 0 && currentSelected[0].Equals(tile.Loc))
            return;

        if (tileZone.Contains(tile.Loc)) {
            if (currentSelected.Count != 0) {
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

            if (stateZone == TileState.MoveZone) {
                tmpPlayerVisual.transform.position = tile.Archor;
            }

            tile.ChangeTileState(stateZone + 1);
            currentSelected.Add(tile.Loc);
            switch (stateZone) {
                case TileState.CastZone:
                    var deltaLoc = tile.Loc - player.Loc;
                    int xDir = deltaLoc.x >= 0 ? 1 : -1;
                    int yDir = deltaLoc.y >= 0 ? 1 : -1;
                    bool exchange = deltaLoc.x != 0;

                    foreach (var point in skills[selectedSkillID].affectPoints) {
                        Location fixedPoint;
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
                default:
                    break;
            }

        }
    }

    public void Button_CastSkill(int id)
    {
        selectedSkillID = id;
        ChangeState(BattleState.CastSkill);
        PrepareSkill();
    }

    private void FixedUpdate()
    {
        
    }
}
