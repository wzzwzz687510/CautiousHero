using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wing.RPGSystem;
using UnityEngine.Events;

public enum BattleState
{
    PlacePlayer,
    PlayerMove,
    PlayerCast,    
    PlayerAnim,
    BotTurn,
    NonInteractable,
    End
}

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    [Header("Test")]
    //public BaseSkill[] skills;
    public BattleConfig config;
    public GameObject win;

    [Header("Component References")]
    public LayerMask tileLayer;
    public LayerMask entityLayer;
    public PlayerController player;
    public AbioticController abioticPrefab;

    public BattleState State { get; private set; }
    public bool IsPlayerTurn { get { return State == BattleState.PlayerMove || State == BattleState.PlayerCast|| State == BattleState.PlayerAnim; } }
    

    private SpriteRenderer tmpVisualPlayer;
    private Transform abtioticHolder;
    
    private List<Location> currentSelected = new List<Location>();
    private int selectedSkillID = 0;
    private int selectedCreatureID = 0;

    private HashSet<Location> tileZone = new HashSet<Location>();

    public delegate void TurnSwitchCallback(bool isPlayerTurn);
    public event TurnSwitchCallback OnTurnSwitched;

    public delegate void CreatureBoard(int hash, bool isExit);
    public CreatureBoard CreatureBoardEvent;

    private void Awake()
    {
        if (!Instance)
            Instance = this;

    }

    private void Start()
    {
        State = BattleState.PlacePlayer;
        GridManager.Instance.onCompleteMapRenderEvent += PrepareBattleStart;
        AnimationManager.Instance.OnAnimCompleted.AddListener(OnAnimCompleted);

        player.InitPlayer(Database.Instance.ActiveData.attribute, Database.Instance.GetEquippedSkills());
        GetComponent<BattleUIController>().Init();
    }

    public void PrepareBattleStart()
    {
        AIManager.Instance.Init(config);
        AddAbiotics();
        PreparePlacePlayer();       
    }

    private void AddAbiotics()
    {
        if (abtioticHolder) {
            Destroy(abtioticHolder);
        }
        abtioticHolder = new GameObject("Abiotic Holder").transform;

        var sets = config.abioticSets;
        int[] possiblities = new int[sets.Length];
        possiblities[0] = sets[0].power;
        for (int i = 1; i < config.abioticSets.Length; i++) {
            possiblities[i] = possiblities[i - 1] + sets[i].power;        
        }
        int totalPower = possiblities[sets.Length - 1];
        float randomFill, randomAbiotic;
        foreach (var tile in GridManager.Instance.ValidTiles()) {
            randomFill = 1000.Random()/1000.0f;
            if (randomFill <= config.coverage) {
                randomAbiotic = totalPower.Random();
                for (int i = 0; i < sets.Length; i++) {
                    if (randomAbiotic < possiblities[i]) {
                        Instantiate(abioticPrefab, abtioticHolder).GetComponent<AbioticController>().
                            InitAbioticEntity(sets[i].tAbiotic, tile);
                        break;
                    }
                }
            }
        }        
    }

    private void OnAnimCompleted()
    {
        if (State != BattleState.PlacePlayer && GameConditionCheck())
            return;

        if (State == BattleState.PlayerAnim)
            State = BattleState.PlayerMove;
        else if (State == BattleState.BotTurn) {
            CompleteBotTurn();
        }

    }

    private void ChangeState(BattleState state)
    {
        //Debug.Log(state);
        State = state;
        GridManager.Instance.ResetAllTiles();
        currentSelected.Clear();
        tileZone.Clear();
        player.SetActiveCollider(true);
        player.Sprite.color = Color.white;
        if (tmpVisualPlayer)
            tmpVisualPlayer.gameObject.SetActive(false);
    }

    private void CompletePlacement()
    {
        ChangeState(BattleState.PlayerMove);
        player.OnEntityTurnStart();
    }

    private void CompleteMovement()
    {
        ChangeState(BattleState.PlayerMove);
    }

    private void ApplyCast()
    {
        player.CastSkill(selectedSkillID, currentSelected[0]);
        ChangeState(BattleState.PlayerAnim);
        //ChangeState(BattleState.Move);
    }

    private void CompletePlayerTurn()
    {
        ChangeState(BattleState.BotTurn);
        OnTurnSwitched?.Invoke(false);
    }

    public void CompleteBotTurn()
    {
        OnTurnSwitched?.Invoke(true);      
    }

    public void StartNewTurn(bool isPlayerTurn)
    {
        if (isPlayerTurn) {
            ChangeState(BattleState.PlayerMove);
            player.OnEntityTurnStart();
        }
        else {
            AIManager.Instance.OnBotTurnStart();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
            CastSkill(0);
        if (Input.GetKeyDown(KeyCode.W))
            CastSkill(1);
        if (Input.GetKeyDown(KeyCode.E))
            CastSkill(2);
        if (Input.GetKeyDown(KeyCode.R))
            CastSkill(3);

        if (Input.GetMouseButtonDown(1) && (State == BattleState.PlayerMove || State == BattleState.PlayerCast)) {
            ChangeState(BattleState.PlayerMove);
        }

        var ray = Camera.main.ViewportPointToRay(new Vector3(Input.mousePosition.x / Screen.width,
            Input.mousePosition.y / Screen.height, Input.mousePosition.z));
        //Debug.DrawRay(ray.origin,10* ray.direction,Color.red,10);
        var hit = Physics2D.Raycast(ray.origin, ray.direction, 20, tileLayer);
        if (hit) {
            var tile = hit.transform.parent.GetComponent<TileController>();
            switch (State) {
                case BattleState.PlacePlayer:
                    SelectVisual(tile, TileState.PlaceZone);

                    if (Input.GetMouseButtonDown(0) && tileZone.Contains(tile.Loc)) {
                        player.MoveToTile(tile, true);
                        player.DropAnimation();
                        CompletePlacement();
                    }
                    break;
                case BattleState.PlayerMove:
                    SelectVisual(tile, TileState.MoveZone);

                    if (Input.GetMouseButtonDown(0) && tileZone.Contains(currentSelected[0])) {
                        var tc = player.LocateTile;
                        player.MoveToTile(tile);
                        if (tile != tc) {
                            ChangeState(BattleState.PlayerAnim);
                            AnimationManager.Instance.PlayOnce();
                        }
                    }
                    break;
                case BattleState.PlayerCast:
                    SelectVisual(tile, TileState.CastZone);

                    if (Input.GetMouseButtonDown(0) && currentSelected.Count != 0) {
                        if (tileZone.Contains(tile.Loc) || (tile.IsBind && currentSelected[0] == tile.CastLoc))
                            ApplyCast();
                    }
                    break;
                case BattleState.BotTurn:
                    break;
                case BattleState.PlayerAnim:
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
                switch (State) {
                    case BattleState.PlacePlayer:
                        break;
                    case BattleState.PlayerMove:
                        player.ChangeOutlineColor(Color.red);

                        if (Input.GetMouseButtonDown(0)) {
                            PrepareMove();
                        }
                        break;
                    case BattleState.PlayerCast:
                        break;
                    case BattleState.BotTurn:
                        break;
                    case BattleState.PlayerAnim:
                        break;
                    case BattleState.NonInteractable:
                        break;
                    default:
                        break;
                }
            }
            else if(State!= BattleState.PlayerCast){
                player.ChangeOutlineColor(Color.black);
            }
        }
        else {
            hit = Physics2D.Raycast(ray.origin, ray.direction, 20, entityLayer);
            if (hit && hit.transform.CompareTag("Creature")) {
                var cc = hit.transform.parent.GetComponent<CreatureController>();
                if (selectedCreatureID != cc.EntityHash) {
                    selectedCreatureID = cc.EntityHash;
                    CreatureBoardEvent?.Invoke(cc.EntityHash, false);
                }
                switch (State) {
                    case BattleState.PlacePlayer:
                        break;
                    case BattleState.PlayerMove:
                        break;
                    case BattleState.PlayerCast:
                        SelectVisual(cc.LocateTile, TileState.CastZone);

                        if (Input.GetMouseButtonDown(0) && currentSelected.Count != 0 && tileZone.Contains(currentSelected[0])) {
                            ApplyCast();
                        }
                        break;
                    case BattleState.BotTurn:
                        break;
                    case BattleState.PlayerAnim:
                        break;
                    case BattleState.NonInteractable:
                        break;
                    default:
                        break;
                }
            }
            else if (selectedCreatureID != 0) {
                selectedCreatureID = 0;
                CreatureBoardEvent?.Invoke(-1, true);
            }
        }

    }

    private void PreparePlacePlayer()
    {
        for (int x = 0; x < GridManager.Instance.MapBoundingBox.x; x++) {
            for (int y = (int)GridManager.Instance.MapBoundingBox.y - 2; y < GridManager.Instance.MapBoundingBox.y; y++) {
                Location loc = new Location(x, y);
                if (!loc.IsEmpty()) continue;
                GridManager.Instance.ChangeTileState(loc, TileState.PlaceZone);
                tileZone.Add(loc);
            }
        }
    }

    private void PrepareMove()
    {
        if (player.ActionPoints == 0)
            return;
        player.SetActiveCollider(false);
        SetVisualPlayer(player.transform.position + new Vector3(0, 0.3f, 0), player.LocateTile.SortOrder + 64);

        player.Sprite.color = new Color(1, 1, 1, 0.5f);
        foreach (var loc in GridManager.Instance.Astar.GetGivenDistancePoints(player.Loc, player.ActionPoints / player.MoveCost)) {
            GridManager.Instance.ChangeTileState(loc, TileState.MoveZone);
            tileZone.Add(loc);
        }
    }

    private void SetVisualPlayer(Vector3 des,int sortingOrder)
    {
        if (!tmpVisualPlayer) {
            tmpVisualPlayer = Instantiate(player.Sprite.gameObject, player.transform.position + new Vector3(0, 0.3f, 0),
                Quaternion.identity).GetComponent<SpriteRenderer>();

            tmpVisualPlayer.GetComponent<SpriteRenderer>().sortingOrder = player.Loc.x + player.Loc.y * 8 + 1;
        }
        else {
            tmpVisualPlayer.transform.position = des;
            tmpVisualPlayer.sortingOrder = sortingOrder;
            tmpVisualPlayer.gameObject.SetActive(true);
        }
        
    }

    private void PrepareSkill()
    {
        player.SetActiveCollider(false);

        var skill = player.Skills[selectedSkillID];
        switch (skill.castType) {
            case CastType.Instant:
                foreach (var point in skill.CastPatterns) {
                    var loc = player.Loc + point;
                    if (GridManager.Instance.ChangeTileState(loc, TileState.CastZone))
                        tileZone.Add(loc);
                }
                break;
            case CastType.Trajectory:
                foreach (var castPattern in skill.CastPatterns) {
                    var castLoc = player.Loc + castPattern;
                    if (!GridManager.Instance.ChangeTileState(castLoc, TileState.CastZone))
                        continue;
                    tileZone.Add(castLoc);

                    foreach (var pattern in skill.GetFixedEffectPatterns(castPattern)) {

                        var hitPath = GridManager.Instance.GetTrajectoryHitTile(castLoc, pattern, false);
                        foreach (var passTile in hitPath) {
                            passTile.BindCastLocation(castLoc);
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
            if (currentSelected[0] == tile.Loc || (tile.IsBind && currentSelected[0] == tile.CastLoc))
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
                SetVisualPlayer(tile.Archor, tile.SortOrder + 64);
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
            if (tile.IsBind && player.Skills[selectedSkillID].castType == CastType.Trajectory) {
                if (currentSelected.Count == 0 || currentSelected[0] != tile.CastLoc)
                    HighlightAffectPoints(tile.CastLoc);
            }
            else
                currentSelected.Add(tile.Loc);
        }
    }

    private void Gameover()
    {
        ChangeState(BattleState.End);
        // To do
        AnimationManager.Instance.AddAnimClip(new BaseAnimClip(AnimType.Gameover, 0.5f));
        AnimationManager.Instance.PlayOnce();
        Debug.Log("You lost");
    }

    private void BattleVictory()
    {
        ChangeState(BattleState.End);
        // To do
        win.SetActive(true);
        Debug.Log("You win");
    }

    private void InformLackAP()
    {

    }

    public void HighlightAffectPoints(Location castLoc)
    {
        var skill = player.Skills[selectedSkillID];
        switch (skill.castType) {
            case CastType.Instant:
                foreach (var effectLoc in skill.GetSubEffectZone(player.Loc, castLoc-player.Loc)) {
                    if (GridManager.Instance.ChangeTileState(effectLoc, TileState.CastSelected)) {
                        currentSelected.Add(effectLoc);
                    }
                }
                break;
            case CastType.Trajectory:
                foreach (var effectLoc in skill.GetSubEffectZone(player.Loc, castLoc - player.Loc, true)) {
                    currentSelected.Add(effectLoc);
                }
                break;
            default:
                break;
        }
    }

    public void CastSkill(int id)
    {
        if (!player.ActiveSkills[id].Castable || !IsPlayerTurn)
            return;

        if(player.ActionPoints < player.Skills[id].actionPointsCost) {
            InformLackAP();
            return;
        }
        selectedSkillID = id;
        ChangeState(BattleState.PlayerCast);
        PrepareSkill();
    }

    public void CancelMove()
    {
        if (!IsPlayerTurn) return;
        player.CancelMove();
        ChangeState(BattleState.PlayerMove);
    }

    public void EndTurn()
    {
        if (!IsPlayerTurn) return;
        CompletePlayerTurn();
    }

    /// <summary>
    /// Check whether battle to be end
    /// </summary>
    /// <returns>True for battle end</returns>
    public bool GameConditionCheck()
    {
        bool res = true;
        if (player.IsDeath) {
            Gameover();
        }
        else 
        {
            if (AIManager.Instance.IsAllBotsDeath())
                BattleVictory();
            else
                res = false;
        }

        return res;
    }


}
