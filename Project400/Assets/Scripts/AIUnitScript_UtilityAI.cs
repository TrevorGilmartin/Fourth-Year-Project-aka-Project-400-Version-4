using QPath;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIUnitScript_UtilityAI : AIUnitScript
{
    private AIUnitScript selectedUnit = null;

    public void GetRangeOfMovement()
    {
        Debug.Log("AIUnitScript_UtilityAI");

        foreach (var aiUnit in MapGeneratorScript.instance.aiUnits)
        {
            //if (aiUnit.enemyNumber == 0)
                selectedUnit = aiUnit;
        }
    }
}

