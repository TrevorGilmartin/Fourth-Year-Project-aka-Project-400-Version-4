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
    SelectedAIUnitCharacter,
    MoveableRangeDiscovery,
    DecidingMostSuitableMovement,
    InitiatingPotentialMovement,
    SelfDestructionMode,
    EndTurn,
}

public class StateManagerScript : MonoBehaviour
{
    public static StateManagerScript instance;

    private float turnNumber;
    public float enemyHealth;
    public float goodness;
    private bool overrideGoodness;
    private float defenceTurnCounter;

    public TurnsState currentTurnState = TurnsState.PlayerTurn;
    public States currentState = States.Stalling;

    private AIUnitScript selectedAIUnit;
    private UnitScript selectedPlayerUnit;
    private HexScript[] hexes;
    public List<GameObject> selectedHexes = new List<GameObject>();
    private Color selectedHexesOriginalColor;

    public int selectedHexColumnPositioningDifference;
    public int selectedHexRowPositioningDifference;
    private float movementDelay;
    private List<int> rows;
    private List<int> columns;

    private float coordinatesX1;
    private float coordinatesX2;
    private float coordinatesY1;
    private float coordinatesY2;

    private float xCoordinatesDifference;
    private float yCoordinatesDifference;

    private float totalResult;
    private float squaredResult;
    public HexScript selectedHex;

    public float shortestSquareResult;

    private bool endTurnTrue;
    public float endTurnDelay;

    AIUnitScript[] PlayerAIs;

    public int numberOfPlayers = 2;
    public int currentPlayerID = 0;
    private bool movementActivated;
    private bool pathBegun;
    public bool aiTurn;
    private IEnumerable<object> characterToDestroy;


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
        goodness = -Mathf.Infinity;
        enemyHealth = 150f;
        overrideGoodness = false;
        defenceTurnCounter = 0;

        shortestSquareResult = 0;

        currentTurnState = TurnsState.PlayerTurn;
        currentState = States.WaitingForTurn;

        rows = new List<int>();
        columns = new List<int>();

        movementActivated = false;
        pathBegun = true;
        aiTurn = false;

        endTurnTrue = false;
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        if (selectedAIUnit != null && !overrideGoodness)
        {
            goodness = selectedAIUnit.HitPoints / 2;
        }
        else
        {
            goodness = 50f;
        }

        if (defenceTurnCounter == 2)
        {
            overrideGoodness = true;
        }

        if (pathBegun)
            movementDelay += Time.deltaTime;

        if (movementDelay > 1f)
            pathBegun = false;

        if (endTurnTrue)
            endTurnDelay += Time.deltaTime;

        if (selectedAIUnit != null && selectedAIUnit.HitPoints <= 0)
        {
            currentState = States.SelfDestructionMode;
        }

        if (currentState == States.Stalling)
        {
            DoSelectedAIUnitCharacter();
        }

        if (currentState == States.SelectedAIUnitCharacter)
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

        if (currentState == States.EndTurn)
        {
            DoEndTurn();
        }

        if (currentState == States.SelfDestructionMode)
        {
            DoSelfDestructionMode();
        }
    }

    private void DoSelectedAIUnitCharacter()
    {
        if (currentTurnState == TurnsState.EnemyTurn_1)
        {
            foreach (var aiUnit in MapGeneratorScript.instance.aiUnits)
            {
                List<GameObject> potentialCharacters = MapGeneratorScript.instance.aiUnitToGameObjectMapList;
                foreach (GameObject character in potentialCharacters)
                {
                    if (character.GetComponent<UnitViewScript>().Number == aiUnit.enemyNumber && aiUnit.enemyNumber == 1)
                    {
                        selectedAIUnit = aiUnit;
                        currentState = States.SelectedAIUnitCharacter;
                        Debug.LogError(selectedAIUnit.enemyNumber);
                    }
                }
            }
        }
        if (currentTurnState == TurnsState.EnemyTurn_2)
        {
            foreach (var aiUnit in MapGeneratorScript.instance.aiUnits)
            {
                List<GameObject> potentialCharacters = MapGeneratorScript.instance.aiUnitToGameObjectMapList;
                foreach (GameObject character in potentialCharacters)
                {
                    if (character.GetComponent<UnitViewScript>().Number == aiUnit.enemyNumber && aiUnit.enemyNumber == 2)
                    {
                        selectedAIUnit = aiUnit;
                        currentState = States.SelectedAIUnitCharacter;
                        Debug.LogError(selectedAIUnit.enemyNumber);
                    }
                }
            }
        }
        if (currentTurnState == TurnsState.EnemyTurn_3)
        {
            foreach (var aiUnit in MapGeneratorScript.instance.aiUnits)
            {
                List<GameObject> potentialCharacters = MapGeneratorScript.instance.aiUnitToGameObjectMapList;
                foreach (GameObject character in potentialCharacters)
                {
                    if (character.GetComponent<UnitViewScript>().Number == aiUnit.enemyNumber && aiUnit.enemyNumber == 3)
                    {
                        selectedAIUnit = aiUnit;
                        currentState = States.SelectedAIUnitCharacter;
                        Debug.LogError(selectedAIUnit.enemyNumber);
                    }
                }
            }
        }
    }

    private void DoMoveableRangeDiscovery()
    {
        GameObject tempHex_go = GameObject.Find("Hex_" + selectedAIUnit.Hex.C + "_" + selectedAIUnit.Hex.R);

        hexes = selectedAIUnit.Hex.GetNeighbours(tempHex_go, 5, MapGeneratorScript.instance.numColumns, MapGeneratorScript.instance.numRows);

        foreach (HexScript hex in hexes)
        {
            GameObject tempHexNeighbour_go = GameObject.Find("Hex_" + hex.C + "_" + hex.R);
            if (hex.movementCost == 1)
            {
                selectedHexesOriginalColor = tempHexNeighbour_go.transform.gameObject.GetComponentInChildren<MeshRenderer>().material.color;
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


        #region Attempt1_Of_AI_Movement
        //foreach (HexScript hex in hexes)
        //{
        //    if (!columns.Contains(hex.C))
        //    {
        //        columns.Add(hex.C);
        //    }

        //    if (!rows.Contains(hex.R))
        //    {
        //        rows.Add(hex.R);
        //    }
        //}

        //int closestRow = rows.OrderBy(item => Math.Abs(selectedPlayerUnit.Hex.R - item)).First();
        //int closestColumn = columns.OrderBy(item => Math.Abs(selectedPlayerUnit.Hex.C - item)).First();

        //selectedHexColumnPositioningDifference = closestColumn - selectedAIUnit.Hex.C;
        //selectedHexRowPositioningDifference = closestRow - selectedAIUnit.Hex.R;
        #endregion

        if (goodness > 35)
        {
            foreach (HexScript hex in hexes)//Potential Selectable Hexes for the Current Enemy
            {
                HexScript Found = null;
                foreach (var unit in MapGeneratorScript.instance.aiUnits) //Collection of enemy players
                {
                    if (unit.Hex == hex)
                    {
                        Found = unit.Hex;
                        Debug.LogError("The Current: R=" + unit.Hex.R + " C=" + unit.Hex.C);
                    }
                }

                if (Found == null)
                {
                    coordinatesX1 = hex.C;
                    coordinatesX2 = selectedPlayerUnit.Hex.C;

                    coordinatesY1 = hex.R;
                    coordinatesY2 = selectedPlayerUnit.Hex.R;

                    xCoordinatesDifference = (coordinatesX2 - coordinatesX1);
                    yCoordinatesDifference = (coordinatesY2 - coordinatesY1);
                    xCoordinatesDifference *= xCoordinatesDifference;
                    yCoordinatesDifference *= yCoordinatesDifference;

                    totalResult = (xCoordinatesDifference + yCoordinatesDifference);

                    squaredResult = Mathf.Sqrt(totalResult);

                    if (shortestSquareResult == 0)
                    {
                        shortestSquareResult = squaredResult;
                        selectedHex = hex;
                    }

                    if (shortestSquareResult > squaredResult)
                    {
                        shortestSquareResult = squaredResult;
                        selectedHex = hex;
                    }
                }

            }
        }

        selectedHexColumnPositioningDifference = selectedHex.C - selectedAIUnit.Hex.C;
        selectedHexRowPositioningDifference = selectedHex.R - selectedAIUnit.Hex.R;

        selectedAIUnit.DUMMY_PATHING_FUNCTION();

        movementActivated = true;
        pathBegun = false;

        currentState = States.InitiatingPotentialMovement;


        if (goodness < 35)
        {
            defenceTurnCounter++;

            foreach (HexScript hex in hexes)
            {
                coordinatesX1 = hex.C;
                coordinatesX2 = selectedPlayerUnit.Hex.C;

                coordinatesY1 = hex.R;
                coordinatesY2 = selectedPlayerUnit.Hex.R;

                xCoordinatesDifference = (coordinatesX2 - coordinatesX1);
                yCoordinatesDifference = (coordinatesY2 - coordinatesY1);
                xCoordinatesDifference *= xCoordinatesDifference;
                yCoordinatesDifference *= yCoordinatesDifference;

                totalResult = (xCoordinatesDifference + yCoordinatesDifference);

                squaredResult = Mathf.Sqrt(totalResult);

                if (shortestSquareResult == 0)
                {
                    shortestSquareResult = squaredResult;
                    selectedHex = hex;
                }

                if (shortestSquareResult < squaredResult)
                {
                    shortestSquareResult = squaredResult;
                    selectedHex = hex;
                }
            }

            selectedHexColumnPositioningDifference = selectedHex.C - selectedAIUnit.Hex.C;
            selectedHexRowPositioningDifference = selectedHex.R - selectedAIUnit.Hex.R;

            selectedAIUnit.DUMMY_PATHING_FUNCTION();

            movementActivated = true;
            pathBegun = false;

            currentState = States.InitiatingPotentialMovement;
        }
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

        if (selectedAIUnit.Hex == selectedHex)
        {
            endTurnTrue = true;
        }

        if (endTurnDelay > 1.2f)
        {
            currentState = States.EndTurn;
        }
    }

    private void DoEndTurn()
    {
        foreach (HexScript hex in hexes)
        {
            GameObject tempHexNeighbour_go = GameObject.Find("Hex_" + hex.C + "_" + hex.R);
            if (hex.movementCost == 1)
            {
                tempHexNeighbour_go.transform.gameObject.GetComponentInChildren<MeshRenderer>().material.color = selectedHexesOriginalColor;
            }
        }
        shortestSquareResult = 0;

        endTurnTrue = false;
        endTurnDelay = 0;
        selectedHex = null;

        rows = new List<int>();
        columns = new List<int>();

        movementActivated = false;
        pathBegun = true;
        aiTurn = false;

        turnNumber++;

        currentState = States.WaitingForTurn;

        if (currentTurnState == TurnsState.EnemyTurn_1 && MapGeneratorScript.instance.aiUnits.Count > 1)
        {
            currentTurnState = TurnsState.EnemyTurn_2;
            currentState = States.Stalling;
            aiTurn = true;
            endTurnTrue = false;
        }
        else if (currentTurnState == TurnsState.EnemyTurn_2 && MapGeneratorScript.instance.aiUnits.Count > 2)
        {
            currentTurnState = TurnsState.EnemyTurn_3;
            currentState = States.Stalling;
            aiTurn = true;
            endTurnTrue = false;
        }
        else
        {
            currentTurnState = TurnsState.PlayerTurn;
        }


    }

    private void DoSelfDestructionMode()
    {
        List<GameObject> charactersToDestroy = MapGeneratorScript.instance.aiUnitToGameObjectMapList;
        foreach (GameObject character in characterToDestroy)
        {
            foreach (var aiUnit in MapGeneratorScript.instance.aiUnits)
            {
                if (aiUnit.enemyNumber == character.GetComponent<UnitViewScript>().Number && aiUnit.HitPoints <= 0)
                {
                    Destroy(character);
                }
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
        //if (selectedAIUnit != null)
        //{
        //    if (turnNumber > 0 && selectedAIUnit.HitPoints > 10)
        //    {
        //        selectedAIUnit.HitPoints -= 33;
        //        enemyHealth = selectedAIUnit.HitPoints;
        //    }
        //    else
        //    {
        //        enemyHealth = selectedAIUnit.HitPoints;
        //    }
        //}

        currentTurnState = TurnsState.EnemyTurn_1;
        currentState = States.Stalling;
        aiTurn = true;
        endTurnTrue = false;
    }
}
