using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum TurnsState
{
    PlayerTurn,
    EnemyTurn_1,
    EnemyTurn_2,
    EnemyTurn_3,
}

public enum States
{
    WaitingForTurn,
    Stalling,
    MoveableRangeDiscovery,
    DecidingMostSuitableMovement,
    InitiatingPotentialMovement,
}

public class StateManagerScript : MonoBehaviour
{
    public static StateManagerScript instance;
     
    public TurnsState currentTurnState = TurnsState.PlayerTurn;
    public States currentState = States.Stalling;

    private AIUnitScript selectedAIUnit;
    private UnitScript selectedPlayerUnit;
    private HexScript[] hexes;
    public List<GameObject> selectedHexes = new List<GameObject>();

    public int selectedHexColumnPositioningDifference;
    public int selectedHexRowPositioningDifference;
    private float movementDelay;
    private List<int> rows;
    private List<int> columns;

    AIUnitScript[] PlayerAIs;

    public int numberOfPlayers = 2;
    public int currentPlayerID = 0;
    private bool movementActivated;
    private bool pathBegun;
    public bool aiTurn;
    

    // Use this for initialization
    void Start()
    {
        #region attemptAtStateMachine
        //PlayerAIs = new AIUnitScript[numberOfPlayers];
        //PlayerAIs[0] = null;
        //for (int i = 1; i < numberOfPlayers; i++)
        //{
        //    PlayerAIs[1] = new AIUnitScript_UtilityAI();
        //}
#endregion
        currentTurnState = TurnsState.PlayerTurn;
        currentState = States.WaitingForTurn;

        rows = new List<int>();
        columns = new List<int>();

        movementActivated = false;
        pathBegun = true;
        aiTurn = false;

        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        if (pathBegun)
                    movementDelay += Time.deltaTime;

                if (movementDelay > 1f)
                    pathBegun = false;

        if (currentState == States.Stalling)
        {
            DoMoveableRangeDiscovery();
        }

        if (currentState == States.DecidingMostSuitableMovement)
        {
            DoDecidingMostSuitableMovement();
        }

        if (currentState == States.InitiatingPotentialMovement)
        {
            DoInitiatingPotentialMovement();
        }
    }

    private void DoMoveableRangeDiscovery()
    {
        foreach (var aiUnit in MapGeneratorScript.instance.aiUnits)
        {
            selectedAIUnit = aiUnit;
        }

        GameObject tempHex_go = GameObject.Find("Hex_" + selectedAIUnit.Hex.C + "_" + selectedAIUnit.Hex.R);

        hexes = selectedAIUnit.Hex.GetNeighbours(tempHex_go, 5, MapGeneratorScript.instance.numColumns, MapGeneratorScript.instance.numRows);

        foreach (HexScript hex in hexes)
        {
            GameObject tempHexNeighbour_go = GameObject.Find("Hex_" + hex.C + "_" + hex.R);
            if (hex.movementCost == 1)
            {
                tempHexNeighbour_go.transform.gameObject.GetComponentInChildren<MeshRenderer>().material.color = Color.yellow;
                if (!selectedHexes.Contains(tempHexNeighbour_go) || selectedHexes == null)
                {
                    selectedHexes.Add(tempHexNeighbour_go);
                }
            }
        }
        tempHex_go.GetComponentInChildren<MeshRenderer>().material.color = Color.red;

        currentState = States.DecidingMostSuitableMovement;
    }

    private void DoDecidingMostSuitableMovement()
    {
        foreach (var unit in MapGeneratorScript.instance.units)
        {
            selectedPlayerUnit = unit;
        }

        GameObject tempHex_go = GameObject.Find("Hex_" + selectedPlayerUnit.Hex.C + "_" + selectedPlayerUnit.Hex.R);

        foreach (HexScript hex in hexes)
        {
            if (!columns.Contains(hex.C))
            {
                columns.Add(hex.C);
            }

            if (!rows.Contains(hex.R))
            {
                rows.Add(hex.R);
            }
        }

        int closestRow = rows.OrderBy(item => Math.Abs(selectedPlayerUnit.Hex.R - item)).First();
        int closestColumn = columns.OrderBy(item => Math.Abs(selectedPlayerUnit.Hex.C - item)).First();

        selectedHexColumnPositioningDifference = closestColumn - selectedAIUnit.Hex.C;
        selectedHexRowPositioningDifference = closestRow - selectedAIUnit.Hex.R;

        selectedAIUnit.DUMMY_PATHING_FUNCTION();

        movementActivated = true;
        pathBegun = false;

        currentState = States.InitiatingPotentialMovement;
    }

    private void DoInitiatingPotentialMovement()
    {
        if (movementActivated)
        {
            if (!pathBegun)
            {
                pathBegun = true;
                movementDelay = 0;
                selectedAIUnit.DoTurn();
            }
        }
    }

    public void NewTurn()
    {
        //Debug.Log("NewTurn");
        // This is the start of a player's turn.
        // We don't have a roll for them yet.
        //IsDoneRolling = false;
        //IsDoneClicking = false;
        //IsDoneAnimating = false;

        currentPlayerID = (currentPlayerID + 1) % numberOfPlayers;
    }

    public void ChangingTurnState()
    {
        currentTurnState = TurnsState.EnemyTurn_1;
        currentState = States.Stalling;
        aiTurn = true;
    }
}
