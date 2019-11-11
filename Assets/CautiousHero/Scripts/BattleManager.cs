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

    public LayerMask tileLayer;
    public PlayerController player;
    public BattleState State { get; private set; }

    private GameObject tmpPlayerVisual;
    private TileNavigation m_astar;
    private TileController currentSelected;
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
        Invoke("PreparePlacePlayer", 2);
    }

    private void CompletePlacement()
    {
        State = BattleState.Move;
        GridManager.Instance.ResetTiles();
        currentSelected = null;
        tileZone.Clear();

        if (!tmpPlayerVisual)
            tmpPlayerVisual = Instantiate(player.Sprite.gameObject, player.transform.position, Quaternion.identity);
        player.Sprite.color = new Color(1, 1, 1, 0.5f);
        for (int x = -player.MovementPoint; x < player.MovementPoint+1; x++) {
            for (int y = -player.MovementPoint; y < player.MovementPoint+1; y++) {
                if (Mathf.Abs(x) + Mathf.Abs(y) <= player.MovementPoint) {
                    Location loc = new Location(player.Loc.x + x, player.Loc.y + y);
                    GridManager.Instance.ChangeTileState(loc, TileState.MoveZone);
                    tileZone.Add(loc);
                }
            }
        }        
    }

    private void CompleteMovement()
    {
        State = BattleState.CastSkill;
        GridManager.Instance.ResetTiles();
        currentSelected = null;
        tileZone.Clear();
    }

    private void Update()
    {
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
                        player.gameObject.SetActive(true);
                        player.MoveToTile(tile, null);
                        Invoke("CompletePlacement", 0.5f);
                    }
                    break;
                case BattleState.Move:
                    SelectVisual(tile,TileState.MoveZone);

                    if (Input.GetMouseButtonDown(0)) {
                        player.Sprite.color = Color.white;
                        tmpPlayerVisual.SetActive(false);
                        player.MoveToTile(tile, m_astar.GetPath(player.Loc, tile.Loc), true);
                        CompleteMovement();
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
    }

    public void PreparePlacePlayer()
    {
        for (int x = 0; x < 2; x++) {
            for (int y = 0; y < GridManager.Instance.MapBoundingBox.y; y++) {
                Location loc = new Location(x, y);
                GridManager.Instance.ChangeTileState(loc, TileState.PlaceZone);
                tileZone.Add(loc);
            }
        }
    }

    private void SelectVisual(TileController tile,TileState stateZone)
    {
        if (tileZone.Contains(tile.Loc)) {
            if (currentSelected)
                currentSelected.ChangeTileState(stateZone);

            if (stateZone == TileState.MoveZone) {
                tmpPlayerVisual.transform.position = tile.Archor;
            }

            tile.ChangeTileState(stateZone + 1);
            currentSelected = tile;
        }
    }

    private void FixedUpdate()
    {
        
    }
}
