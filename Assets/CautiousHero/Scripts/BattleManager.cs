using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public PlayerController player;
    public BattleState State { get; private set; }

    private void Awake()
    {
        if (!Instance)
            Instance = this;
        State = BattleState.PlacePlayer;
    }

    public void CompletePlacePlayer()
    {
        State = BattleState.Move;
    }
}
