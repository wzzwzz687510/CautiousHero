using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wing.RPGSystem;
using UnityEngine.Events;
using System;
using UnityEngine.EventSystems;

public enum BattleState
{
    FreeMove,
    ExtraMove,
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
    //public BattleConfig config;
    public float clickCDDuration = 0.1f;

    [Header("Component References")]
    public LayerMask tileLayer;
    public LayerMask entityLayer;
    public PlayerController character;
    public AreaUIController m_battleUIController;
    public Camera battleCamera;

    public BattleState State { get; private set; }
    public bool IsInBattle => State != BattleState.FreeMove;
    public bool IsPlayerTurn => State == BattleState.PlayerMove 
        || State == BattleState.PlayerCast
        || State == BattleState.PlayerAnim;

    private SpriteRenderer tmpVisualPlayer;
    private Transform abtioticHolder;
    
    private List<Location> currentSelected = new List<Location>();
    private int selectedSkillID = 0;
    private int selectedCreatureID = 0;

    private HashSet<Location> tileZone = new HashSet<Location>();
    private bool endTurn;
    private bool clickCD;

    public delegate void TurnSwitchCallback(bool isPlayerTurn);
    public event TurnSwitchCallback OnTurnSwitched;

    public delegate void CreatureBoard(int hash, bool isExit);
    public CreatureBoard CreatureBoardEvent;

    public delegate void MovePreview(int steps);
    public MovePreview MovePreviewEvent;
    public delegate void CastPreview(int skillID);
    public CastPreview CastPreviewEvent;

    [HideInInspector] public UnityEvent BattleEndEvent;

    private void Awake()
    {
        if (!Instance)
            Instance = this;
        State = BattleState.FreeMove;
    }

    private void Start()
    {
        // Agility decides player first or creature first
        //State = BattleState.PlayerMove;
        //GridManager.Instance.OnCompleteMapRenderEvent += PrepareBattleStart;
        //AnimationManager.Instance.OnAnimCompleted.AddListener(OnAnimCompleted);

        //player.InitPlayer(Database.Instance.ActiveData.attribute);
        //GetComponent<BattleUIController>()?.Init();
    }

    public void Init()
    {
        ChangeState(BattleState.FreeMove);
        m_battleUIController.EnterAreaAnim();
        GridManager.Instance.LoadMap();
    }

    public void NewBattle(List<int> battleSet)
    {
        AnimationManager.Instance.OnAnimCompleted.RemoveListener(OnAnimCompleted);
        AnimationManager.Instance.OnAnimCompleted.AddListener(OnAnimCompleted);
        AIManager.Instance.Init(battleSet);
        AudioManager.Instance.PlayMeetClip();
        AudioManager.Instance.PlayBattleClip();
        character.StartNewBattle();
        m_battleUIController.BattleStartAnim();
        StartNewTurn(true);
    }

    private void OnAnimCompleted()
    {
        if (!IsInBattle || GameConditionCheck())
            return;
        if (State == BattleState.PlayerAnim) {
            State = BattleState.PlayerMove;
            if (endTurn) {
                endTurn = false;
                CompleteCharacterTurn();
                return;
            }
        }            
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
        character.SetActiveCollider(true);
        character.EntitySprite.color = Color.white;
        if (tmpVisualPlayer)
            tmpVisualPlayer.gameObject.SetActive(false);
    }

    private void CompletePlacement()
    {
        ChangeState(BattleState.PlayerMove);
        character.OnTurnStarted();
    }

    private void CompleteMovement()
    {
        ChangeState(BattleState.PlayerMove);
    }

    IEnumerator StartClickCD()
    {
        clickCD = true;
        yield return new WaitForSeconds(clickCDDuration);
        clickCD = false;
    }

    private void ApplyCast()
    {
        Location loc = currentSelected[0];
        bool effectiveCast = false;
        BaseSkill skill = character.SkillHashes[selectedSkillID].GetBaseSkill();        
        foreach (var ep in skill.GetFixedEffectPatterns(loc-character.Loc)) {
            if ((loc + ep).TryGetStayEntity(out Entity entity) && entity != null) {
                effectiveCast = true;
                break;
            }
        }
        if (!effectiveCast) return;

        ChangeState(BattleState.PlayerAnim);
        character.CastSkill(selectedSkillID, loc);
        StartCoroutine(StartClickCD());
    }

    private void CompleteCharacterTurn()
    {
        character.OnTurnEnded();
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
            character.OnTurnStarted();
        }
        else {
            AIManager.Instance.OnBotTurnStart();
        }
    }

    private void Update()
    {
        if (!IsInBattle) return;

        if (Input.GetKeyDown(KeyCode.Alpha1))
            CastSkill(1);
        if (Input.GetKeyDown(KeyCode.Alpha2))
            CastSkill(2);
        if (Input.GetKeyDown(KeyCode.Alpha3))
            CastSkill(3);
        if (Input.GetKeyDown(KeyCode.Alpha4))
            CastSkill(4);

        if (Input.GetMouseButtonDown(1) && (State == BattleState.PlayerMove || State == BattleState.PlayerCast)) {
            ChangeState(BattleState.PlayerMove);
            MovePreviewEvent?.Invoke(0);
        }

        if (EventSystem.current.IsPointerOverGameObject()) return;
        var ray = battleCamera.ViewportPointToRay(new Vector3(Input.mousePosition.x / Screen.width,
            Input.mousePosition.y / Screen.height, Input.mousePosition.z));
        Debug.DrawRay(ray.origin,10* ray.direction,Color.red,10);
        var hit = Physics2D.Raycast(ray.origin, ray.direction, 20, tileLayer);
        if (hit) {
            var tile = hit.transform.parent.GetComponent<TileController>();
            switch (State) {
                case BattleState.ExtraMove:
                    SelectVisual(tile.Loc, TileState.MoveZone);
                    if (Input.GetMouseButtonDown(0) && tile.Loc != character.Loc && tileZone.Contains(currentSelected[0])) {
                        character.MoveToTile(tile.Loc, 0);
                        ChangeState(BattleState.PlayerAnim);
                        AnimationManager.Instance.PlayOnce();
                    }
                    break;
                case BattleState.PlayerMove:
                    SelectVisual(tile.Loc, TileState.MoveZone);

                    if (Input.GetMouseButtonDown(0) && tile.Loc != character.Loc && tileZone.Contains(currentSelected[0])) {
                        character.MoveToTile(tile.Loc,character.MoveCost);
                        ChangeState(BattleState.PlayerAnim);
                        AnimationManager.Instance.PlayOnce();
                    }
                    break;
                case BattleState.PlayerCast:
                    SelectVisual(tile.Loc, TileState.CastZone);

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
                    case BattleState.PlayerMove:
                        character.ChangeOutlineColor(Color.red);

                        if (!clickCD && Input.GetMouseButtonDown(0)) {
                            PrepareMovement();
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
            else if (State != BattleState.PlayerCast) {
                character.ChangeOutlineColor(Color.black);
            }
        }


        hit = Physics2D.Raycast(ray.origin, ray.direction, 20, entityLayer);
        if (hit && hit.transform.CompareTag("Creature")) {
            var cc = hit.transform.parent.GetComponent<CreatureController>();
            if (selectedCreatureID != cc.Hash) {
                selectedCreatureID = cc.Hash;
                CreatureBoardEvent?.Invoke(cc.Hash, false);
            }
            switch (State) {
                case BattleState.PlayerMove:
                    break;
                case BattleState.PlayerCast:
                    SelectVisual(cc.Loc, TileState.CastZone);

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

    private void PrepareMovement()
    {
        if (character.ActionPoints == 0)
            return;
        character.SetActiveCollider(false);
        SetVisualPlayer(character.transform.position, character.Loc.GetTileController().SortOrder + 64);
        character.EntitySprite.color = new Color(1, 1, 1, 0.5f);
        foreach (var loc in GridManager.Instance.Nav.GetGivenDistancePoints(character.Loc, character.ActionPoints / character.MoveCost)) {
            GridManager.Instance.ChangeTileState(loc, TileState.MoveZone);
            tileZone.Add(loc);
        }
    }

    private void PrepareExtraMovement(int steps)
    {
        character.SetActiveCollider(false);
        SetVisualPlayer(character.transform.position, character.Loc.GetTileController().SortOrder + 64);
        character.EntitySprite.color = new Color(1, 1, 1, 0.5f);
        foreach (var loc in GridManager.Instance.Nav.GetGivenDistancePoints(character.Loc, steps)) {
            GridManager.Instance.ChangeTileState(loc, TileState.MoveZone);
            tileZone.Add(loc);
        }
    }

    private void SetVisualPlayer(Vector3 des,int sortingOrder)
    {
        if (!tmpVisualPlayer) {
            tmpVisualPlayer = Instantiate(character.EntitySprite.gameObject, character.transform.position,
                Quaternion.identity).GetComponent<SpriteRenderer>();

            tmpVisualPlayer.GetComponent<SpriteRenderer>().sortingOrder = character.Loc.x + character.Loc.y * 8 + 1;
        }
        else {
            tmpVisualPlayer.transform.position = des;
            tmpVisualPlayer.sortingOrder = sortingOrder;
            tmpVisualPlayer.gameObject.SetActive(true);
        }
        
    }

    private void PrepareSkill()
    {
        character.SetActiveCollider(false);

        var skill = character.SkillHashes[selectedSkillID].GetBaseSkill();
        switch (skill.castType) {
            case CastType.Instant:
                foreach (var point in skill.CastPattern) {
                    var loc = character.Loc + point;
                    if (GridManager.Instance.ChangeTileState(loc, TileState.CastZone))
                        tileZone.Add(loc);
                }
                break;
            case CastType.Trajectory:
                foreach (var castPattern in skill.CastPattern) {
                    var castLoc = character.Loc + castPattern;
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

    private void SelectVisual(Location loc, TileState stateZone)
    {
        TileController tile = loc.GetTileController();
        if (currentSelected.Count != 0) {
            if (currentSelected[0] == loc || (tile.IsBind && currentSelected[0] == tile.CastLoc))
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
            MovePreviewEvent?.Invoke(0);
        }

        if (tileZone.Contains(loc)) {
            if (stateZone == TileState.MoveZone) {
                               
            }

            currentSelected.Add(loc);
            switch (stateZone) {
                case TileState.MoveZone:
                    tile.ChangeTileState(stateZone + 1);
                    PreviewMovement(tile);
                    break;
                case TileState.CastZone:
                    HighlightAffectPoints(loc);
                    PreviewCast();
                    break;
                default:
                    break;
            }
        }
        else {
            if (tile.IsBind && character.SkillHashes[selectedSkillID].GetBaseSkill().castType == CastType.Trajectory) {
                if (currentSelected.Count == 0 || currentSelected[0] != tile.CastLoc)
                    HighlightAffectPoints(tile.CastLoc);
            }
            else
                currentSelected.Add(loc);
        }
    }

    private void PreviewMovement(TileController tile)
    {
        SetVisualPlayer(tile.Archor, tile.SortOrder + 64);
        MovePreviewEvent?.Invoke(character.Loc.Distance(tile.Loc));
    }

    private void PreviewCast()
    {
        CastPreviewEvent?.Invoke(selectedSkillID);
    }

    private void Gameover()
    {
        ChangeState(BattleState.End);
        // To do
        AnimationManager.Instance.Clear();
        AnimationManager.Instance.AddAnimClip(new BaseAnimClip(AnimType.Gameover, 0.5f));
        AnimationManager.Instance.PlayOnce();
        AudioManager.Instance.Gameover();
        Debug.Log("You lost");
    }

    private void BattleVictory()
    {
        ChangeState(BattleState.FreeMove);
        BattleEndEvent?.Invoke();
        AnimationManager.Instance.Clear();
        m_battleUIController.DisplayInfo("Victory");
        AudioManager.Instance.PlayVictoryClip();     
                
        AreaManager.Instance.CompleteBattle();
        Debug.Log("You win");

        selectedCreatureID = 0;
        CreatureBoardEvent?.Invoke(-1, true);
    }

    private void InformLackAP()
    {
        AudioManager.Instance.PlayErrorClip();
    }

    public void HighlightAffectPoints(Location castLoc)
    {
        var skill = character.SkillHashes[selectedSkillID].GetBaseSkill();
        switch (skill.castType) {
            case CastType.Instant:
                foreach (var effectLoc in skill.GetSubEffectZone(character.Loc, castLoc-character.Loc)) {
                    if (GridManager.Instance.ChangeTileState(effectLoc, TileState.CastSelected)) {
                        currentSelected.Add(effectLoc);
                    }
                }
                break;
            case CastType.Trajectory:
                foreach (var effectLoc in skill.GetSubEffectZone(character.Loc, castLoc - character.Loc, true)) {
                    currentSelected.Add(effectLoc);
                }
                break;
            default:
                break;
        }
    }

    public void CastSkill(int id)
    {
        if (!IsPlayerTurn)
            return;

        m_battleUIController.UpdateSkillSprites();
        if(character.ActionPoints < character.SkillHashes[id].GetBaseSkill().actionPointsCost) {
            InformLackAP();
            return;
        }
        selectedSkillID = id;
        ChangeState(BattleState.PlayerCast);
        PrepareSkill();
    }

    public void EndTurn()
    {
        if (State == BattleState.PlayerAnim) {
            endTurn = true;
            return;
        }
        CompleteCharacterTurn();
    }

    /// <summary>
    /// Check whether battle to be end
    /// </summary>
    /// <returns>True for battle end</returns>
    public bool GameConditionCheck()
    {
        if (State == BattleState.End)
            return false;

        bool res = true;
        if (character.IsDeath) {
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
