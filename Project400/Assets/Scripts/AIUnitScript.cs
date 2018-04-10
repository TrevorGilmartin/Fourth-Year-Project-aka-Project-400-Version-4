using QPath;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIUnitScript : IQPathUnitScript
{
    public string name = "AIPlumber";
    public int enemyNumber = 0;
    public int HitPoints = 100;
    public int Strenght = 8;
    public int Movement = 2;
    public int MovementRemaining;
    public string unitType = "AIUnitScript";
    private StateManagerScript stateManager;

    public HexScript Hex { get; protected set; }

    public delegate void UnitMovedDelegate(HexScript oldHex, HexScript newHex);
    public event UnitMovedDelegate OnUnitMoved;

    public delegate void receivedNameDelegate(string Name);
    public event receivedNameDelegate ReceivedName;

    public delegate void receivedNumberDelegate(int Number);
    public event receivedNumberDelegate ReceivedNumber;

    Queue<HexScript> hexPath;

    //TODO: This should be moved to a central option config file
    const bool MOVEMENT_RULES_LIKE_CIV6 = false;

    public void SetName()
    {
        if (ReceivedName != null)
        {
            ReceivedName(name);
        }
    }

    public void SetNumber()
    {
        if (ReceivedNumber != null)
        {
            ReceivedNumber(enemyNumber);
        }
    }

    //virtual public void DoAI()
    //{

    //    // Do the thing for the current stage we're in

    //    if (stateManager.currentState == States.Stalling)
    //    {
    //        // We need to roll the dice!
    //        DoMoveableRangeDiscovery();
    //        Debug.Log(name + "" + enemyNumber);
    //    }

    //    //if (stateManager.IsDoneClicking == false)
    //    //{
    //    //    // We have a die roll, but we need to pick a stone to move
    //    //    DoClick();
    //    //    return;
    //    //}

    //}

    //virtual protected void DoMoveableRangeDiscovery()
    //{
        
    //}


    public void SetHex(HexScript newHex)
    {
        HexScript oldHex = Hex;

        if (newHex != null)
        {
            newHex.RemoveUnit(this);
        }

        Hex = newHex;

        newHex.AddUnit(this);

        if (OnUnitMoved != null)
        {
            OnUnitMoved(oldHex, newHex);
        }
    }


    public void DUMMY_PATHING_FUNCTION()
    {
        /*QPath.CostEstimateDelegate ced = (IQPathTileScript a, IQPathTileScript b) => (
            return Hex.Distance(a, b);
        );*/

        HexScript[] pathHexes = QPathScript.FindPath(
            Hex.HexMap,
            this,
            Hex,
            //Hex.HexMap.GetHexAt(Hex.C - 1, Hex.R - 3),
            Hex.HexMap.GetHexAt(Hex.C + StateManagerScript.instance.selectedHexColumnPositioningDifference, Hex.R + StateManagerScript.instance.selectedHexRowPositioningDifference),
            HexScript.CostEstimate
        );

        //Debug.Log("Got pathfinding path of length: " + pathHexes.Length);

        MouseManagerScript.instance.PathCount(pathHexes.Length);

        SetHexPath(pathHexes);
    }

    public void SetHexPath(HexScript[] hexPath)
    {
        this.hexPath = new Queue<HexScript>(hexPath);
        this.hexPath.Dequeue();
    }

    public void DoTurn()
    {
        Debug.Log("DoTurn");

        if (hexPath == null || hexPath.Count == 0)
        {
            return;
        }
        //Grab the first hex in our queue
        HexScript newHex = hexPath.Dequeue();


        //Move to the new Hex  
        SetHex(newHex);
        
        #region Debuging
        //HexScript oldHex = Hex;
        //newHex = oldHex.HexMap.GetHexAt(oldHex.C + 1, oldHex.R);
        #endregion

        SetHex(newHex);
    }

    #region Helpers
    public int MovementCostToEnterHex(HexScript hex)
    {
        //TODO: Override base movement cost based on our movement plus tile type
        return hex.BaseMovementCost();
    }

    public float AggregateTurnsToEnterHex(HexScript hex, float turnsToDate)
    {
        {
            // The issue at hand is that if you are trying to enter a tile
            // with a movement cost greater than your current remaining movement
            // points, this will either result in a cheaper-than expected
            // turn cost (Civ5) or a more-expensive-than expected turn cost (Civ6)

            float baseTurnsToEnterHex = MovementCostToEnterHex(hex) / Movement; // Example: Entering a forest is "1" turn

            if (baseTurnsToEnterHex < 0)
            {
                // Impassible terrain
                Debug.Log("Impassible terrain at:" + hex.ToString());
                return -99999;
            }

            if (baseTurnsToEnterHex > 1)
            {
                // Even if something costs 3 to enter and we have a max move of 2, 
                // you can always enter it using a full turn of movement.
                return baseTurnsToEnterHex;
            }


            float turnsRemaining = MovementRemaining / Movement;    // Example, if we are at 1/2 move, then we have .5 turns left

            float turnsToDateWhole = Mathf.Floor(turnsToDate); // Example: 4.33 becomes 4
            float turnsToDateFraction = turnsToDate - turnsToDateWhole; // Example: 4.33 becomes 0.33

            if ((turnsToDateFraction > 0 && turnsToDateFraction < 0.01f) || turnsToDateFraction > 0.99f)
            {
                Debug.LogError("Looks like we've got floating-point drift: " + turnsToDate);

                if (turnsToDateFraction < 0.01f)
                    turnsToDateFraction = 0;

                if (turnsToDateFraction > 0.99f)
                {
                    turnsToDateWhole += 1;
                    turnsToDateFraction = 0;
                }
            }

            float turnsUsedAfterThismove = turnsToDateFraction + baseTurnsToEnterHex; // Example 0.33 + 1

            if (turnsUsedAfterThismove > 1)
            {
                // We have hit the situation where we don't actually have enough movement to complete this move.
                // What do we do?

                if (MOVEMENT_RULES_LIKE_CIV6)
                {
                    // We aren't ALLOWED to enter the tile this move. That means, we have to...

                    if (turnsToDateFraction == 0)
                    {
                        // We have full movement, but this isn't enough to enter the tile
                        // EXAMPLE: We have a max move of 2 but the tile costs 3 to enter.
                        // We are good to go.
                    }
                    else
                    {
                        // We are NOT on a fresh turn -- therefore we need to 
                        // sit idle for the remainder of this turn.
                        turnsToDateWhole += 1;
                        turnsToDateFraction = 0;
                    }

                    // So now we know for a fact that we are starting the move into difficult terrain
                    // on a fresh turn.
                    turnsUsedAfterThismove = baseTurnsToEnterHex;
                }
                else
                {
                    // Civ5-style movement state that we can always enter a tile, even if we don't
                    // have enough movement left.
                    turnsUsedAfterThismove = 1;
                }
            }

            // turnsUsedAfterThismove is now some value from 0..1. (this includes
            // the factional part of moves from previous turns).


            // Do we return the number of turns THIS move is going to take?
            // I say no, this an an "aggregate" function, so return the total
            // turn cost of turnsToDate + turns for this move.

            return turnsToDateWhole + turnsUsedAfterThismove;

        }
        #endregion
    }
    public float CostToEnterHex(IQPathTileScript sourceTile, IQPathTileScript destinationTile)
    {
        return 1;
    }
}


