using QPath;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HexScript : IQPathTileScript
{
    public int C; //Column
    public int R; //Row
    public int S;

    bool isBlocked;

    static readonly float WIDTH_MULTIPLIER = Mathf.Sqrt(3) / 2;

    HashSet<UnitScript> units;
    List<AIUnitScript> aiUnits;

    public int movementCost;

    //TODO:Need some kind of property to track hex time(oil, grass, Slowdown, etc)

    public readonly MapGeneratorScript HexMap;

    private HexScript[,] hexes;

    public HexScript(MapGeneratorScript hexMap, int c, int r)
    {
        this.HexMap = hexMap;

        this.C = c;
        this.R = r;
        this.S = -(c + r);
    }

    public Vector3 Position()
    {
        float radius = 1f;
        float height = radius * 2;
        float width = WIDTH_MULTIPLIER * height;

        float verticalSpacing = height * 0.75f;
        float horizontalSpacing = width;

        return new Vector3(horizontalSpacing * (this.C + this.R / 2f), 0, verticalSpacing * this.R);
    }

    public Vector3 PositionFromCamera()
    {
        return HexMap.GetHexPosition(this);
    }

    public Vector3 PositionFromCamera(Vector3 cameraPosition, float numRows, float numColumns)
    {
        Vector3 position = Position();

        return position;
    }

    public static float CostEstimate(IQPathTileScript aa, IQPathTileScript bb)
    {
        return Distance((HexScript)aa, (HexScript)bb);
    }

    public static float Distance(HexScript a, HexScript b)
    {
        int dC = Mathf.Abs(a.C - b.C);

        if (dC > a.HexMap.numColumns / 2)
            dC = a.HexMap.numColumns - dC;

        int dR = Mathf.Abs(a.R - b.R);

        if (dR > a.HexMap.numRows / 2)
            dR = a.HexMap.numRows - dR;

        return Mathf.Max(dC, dR, Mathf.Abs(a.S - b.S)
            );
    }

    public HexScript[] GetNeighbours(GameObject hex_go, int radius, int column, int row)
    {
        List<HexScript> results = new List<HexScript>();
        HexScript hex = hex_go.GetComponent<HexComponentScript>().hex;
        hexes = /*MapGeneratorScript.instance.hexes*/ HexMap.hexes;

        for (int dx = -radius; dx < radius + 1; dx++)
        {
            for (int dy = Mathf.Max(-radius, -dx - radius); dy < Mathf.Min(radius + 1, -dx + radius + 1); dy++)
            {
                if (hex.R + dy >= 0 && hex.C + dx >= 0 && hex.R + dy <= /*MapGeneratorScript.instance.numRows*/ 
                    HexMap.numRows - 1 && hex.C + dx <= /*MapGeneratorScript.instance.numColumns*/ HexMap.numColumns - 1)

                    results.Add(hexes[hex.C + dx, hex.R + dy]);
            }

        }

        return results.ToArray();

    }

    public void AddUnit(UnitScript unit)
    {
        if (units == null)
        {
            units = new HashSet<UnitScript>();
        }

        units.Add(unit);
    }

    public void RemoveUnit(UnitScript unit)
    {
        if (units != null)
        {
            units.Remove(unit);
        }
    }

    public void AddUnit(AIUnitScript aiUnit)
    {
        if (aiUnits == null)
        {
            aiUnits = new List<AIUnitScript>();
        }

        aiUnits.Add(aiUnit);
    }

    public void RemoveUnit(AIUnitScript aiUnit)
    {
        if (aiUnits != null)
        {
            aiUnits.Remove(aiUnit);
        }
    }

    public UnitScript[] Units()
    {
        return units.ToArray();
    }

    public int BaseMovementCost()
    {
        //TODO: Factor in terrain type & Features
        return movementCost;
    }

    HexScript[] neighbours;

    #region IQPathTile implementation
    public IQPathTileScript[] GetNeighbours()
    {
        if (this.neighbours != null)
            return this.neighbours;

        List<HexScript> neighbours = new List<HexScript>();

        neighbours.Add(HexMap.GetHexAt(C + 1, R + 0));
        neighbours.Add(HexMap.GetHexAt(C + -1, R + 0));
        neighbours.Add(HexMap.GetHexAt(C + 0, R + +1));
        neighbours.Add(HexMap.GetHexAt(C + 0, R + -1));
        neighbours.Add(HexMap.GetHexAt(C + +1, R + -1));
        neighbours.Add(HexMap.GetHexAt(C + -1, R + +1));

        List<HexScript> neighbours2 = new List<HexScript>();

        // This is an alternative to the code below
        // this.neighbours = neighbours.Where(hex => hex != null).ToArray();

        foreach (HexScript h in neighbours)
        {
            if (h != null)
            {
                neighbours2.Add(h);
            }
        }

        this.neighbours = neighbours2.ToArray();
      
        return this.neighbours;
    }

    public float AggregateCostToEnter(float costSoFar, IQPathTileScript sourceTile, IQPathUnitScript theUnit)
    {
        // TODO: We are ignoring source tile right now, this will have to change when
        // we have rivers.
        if (StateManagerScript.instance.aiTurn == true)
        {
            return ((AIUnitScript)theUnit).AggregateTurnsToEnterHex(this, costSoFar);
        }
        else
        {
            return ((UnitScript)theUnit).AggregateTurnsToEnterHex(this, costSoFar);
        }
    }
    #endregion

    #region my attempt
    //    //Coordinated within the map array
    //    public int x;
    //    public int y;
    //    public int tilesWidthMax;
    //    public int tilesHeightMax;

    //    public HexScript[] GetNeighbours(GameObject hex, int radius)
    //    {

    //        List<HexScript> results = new List<HexScript>();

    //        for (int dx = 0; dx < radius - 1; dx++)
    //        {
    //            for (int dy = Mathf.Max(-radius + 1, -dx - radius); dy < Mathf.Min(radius, -dx + radius + 1); dy++)
    //            {
    //                results.Add()
    //            }
    //        }


    //#region My Stupid Attempt
    //        //This allows me to get the immediate neighbout to the right if im not at the edge
    //        if (x > 0)
    //        {
    //            GameObject leftNeighbour = GameObject.Find("Hex_" + (x - 1) + "_" + y);
    //            //Debug.Log("Tile Hit: " + leftNeighbour.name);
    //            MeshRenderer lmr = leftNeighbour.GetComponentInChildren<MeshRenderer>();
    //            lmr.material.color = Color.yellow;
    //        }
    //        if (x < tilesWidthMax)
    //        {
    //            GameObject rightNeighbour = GameObject.Find("Hex_" + (x + 1) + "_" + y);
    //            //Debug.Log("Tile Hit: " + rightNeighbour.name);
    //            MeshRenderer rmr = rightNeighbour.GetComponentInChildren<MeshRenderer>();
    //            rmr.material.color = Color.yellow;
    //        }

    //        if (y % 2 == 1)
    //        {
    //            GameObject topLeftNeighbour = GameObject.Find("Hex_" + (x) + "_" + (y + 1));
    //            //Debug.Log("Tile Hit: " + topLeftNeighbour.name);
    //            MeshRenderer tlmr = topLeftNeighbour.GetComponentInChildren<MeshRenderer>();
    //            tlmr.material.color = Color.yellow;

    //            GameObject topRightNeighbour = GameObject.Find("Hex_" + (x + 1) + "_" + (y + 1));
    //            //Debug.Log("Tile Hit: " + topRightNeighbour.name);
    //            MeshRenderer rlmr = topRightNeighbour.GetComponentInChildren<MeshRenderer>();
    //            rlmr.material.color = Color.yellow;

    //            GameObject bottomLeftNeighbour = GameObject.Find("Hex_" + (x) + "_" + (y - 1));
    //            //Debug.Log("Tile Hit: " + topLeftNeighbour.name);
    //            MeshRenderer blmr = bottomLeftNeighbour.GetComponentInChildren<MeshRenderer>();
    //            blmr.material.color = Color.yellow;

    //            GameObject bottomRightNeighbour = GameObject.Find("Hex_" + (x + 1) + "_" + (y - 1));
    //            //Debug.Log("Tile Hit: " + topRightNeighbour.name);
    //            MeshRenderer brmr = bottomRightNeighbour.GetComponentInChildren<MeshRenderer>();
    //            brmr.material.color = Color.yellow;
    //        }
    //        else
    //        {
    //            GameObject topLeftNeighbour = GameObject.Find("Hex_" + (x - 1) + "_" + (y + 1));
    //            //Debug.Log("Tile Hit: " + topLeftNeighbour.name);
    //            MeshRenderer tlmr = topLeftNeighbour.GetComponentInChildren<MeshRenderer>();
    //            tlmr.material.color = Color.yellow;

    //            GameObject topRightNeighbour = GameObject.Find("Hex_" + (x) + "_" + (y + 1));
    //            //Debug.Log("Tile Hit: " + topRightNeighbour.name);
    //            MeshRenderer rlmr = topRightNeighbour.GetComponentInChildren<MeshRenderer>();
    //            rlmr.material.color = Color.yellow;

    //            GameObject bottomLeftNeighbour = GameObject.Find("Hex_" + (x - 1) + "_" + (y - 1));
    //            //Debug.Log("Tile Hit: " + topLeftNeighbour.name);
    //            MeshRenderer blmr = bottomLeftNeighbour.GetComponentInChildren<MeshRenderer>();
    //            blmr.material.color = Color.yellow;

    //            GameObject bottomRightNeighbour = GameObject.Find("Hex_" + (x) + "_" + (y - 1));
    //            //Debug.Log("Tile Hit: " + topRightNeighbour.name);
    //            MeshRenderer brmr = bottomRightNeighbour.GetComponentInChildren<MeshRenderer>();
    //            brmr.material.color = Color.yellow;
    //        }
    //#endregion
    //        return
    //            null;
    //    }

    //    public HexScript[] ResetColor(GameObject hex)
    //    {
    //        //This allows me to get the immediate neighbout to the right if im not at the edge
    //        if (x > 0)
    //        {
    //            GameObject leftNeighbour = GameObject.Find("Hex_" + (x - 1) + "_" + y);
    //            //Debug.Log("Tile Hit: " + leftNeighbour.name);
    //            MeshRenderer lmr = leftNeighbour.GetComponentInChildren<MeshRenderer>();
    //            lmr.material.color = Color.white;
    //        }
    //        if (x < tilesWidthMax)
    //        {
    //            GameObject rightNeighbour = GameObject.Find("Hex_" + (x + 1) + "_" + y);
    //            //Debug.Log("Tile Hit: " + rightNeighbour.name);
    //            MeshRenderer rmr = rightNeighbour.GetComponentInChildren<MeshRenderer>();
    //            rmr.material.color = Color.white;
    //        }

    //        if (y % 2 == 1)
    //        {
    //            GameObject topLeftNeighbour = GameObject.Find("Hex_" + (x) + "_" + (y + 1));
    //            //Debug.Log("Tile Hit: " + topLeftNeighbour.name);
    //            MeshRenderer tlmr = topLeftNeighbour.GetComponentInChildren<MeshRenderer>();
    //            tlmr.material.color = Color.white;

    //            GameObject topRightNeighbour = GameObject.Find("Hex_" + (x +1) + "_" + (y + 1));
    //            //Debug.Log("Tile Hit: " + topRightNeighbour.name);
    //            MeshRenderer rlmr = topRightNeighbour.GetComponentInChildren<MeshRenderer>();
    //            rlmr.material.color = Color.white;

    //            GameObject bottomLeftNeighbour = GameObject.Find("Hex_" + (x) + "_" + (y - 1));
    //            //Debug.Log("Tile Hit: " + topLeftNeighbour.name);
    //            MeshRenderer blmr = bottomLeftNeighbour.GetComponentInChildren<MeshRenderer>();
    //            blmr.material.color = Color.white;

    //            GameObject bottomRightNeighbour = GameObject.Find("Hex_" + (x + 1) + "_" + (y - 1));
    //            //Debug.Log("Tile Hit: " + topRightNeighbour.name);
    //            MeshRenderer brmr = bottomRightNeighbour.GetComponentInChildren<MeshRenderer>();
    //            brmr.material.color = Color.white;
    //        }
    //        else
    //        {
    //            GameObject topLeftNeighbour = GameObject.Find("Hex_" + (x - 1) + "_" + (y + 1));
    //            //Debug.Log("Tile Hit: " + topLeftNeighbour.name);
    //            MeshRenderer tlmr = topLeftNeighbour.GetComponentInChildren<MeshRenderer>();
    //            tlmr.material.color = Color.white;

    //            GameObject topRightNeighbour = GameObject.Find("Hex_" + (x) + "_" + (y + 1));
    //            //Debug.Log("Tile Hit: " + topRightNeighbour.name);
    //            MeshRenderer rlmr = topRightNeighbour.GetComponentInChildren<MeshRenderer>();
    //            rlmr.material.color = Color.white;

    //            GameObject bottomLeftNeighbour = GameObject.Find("Hex_" + (x - 1) + "_" + (y - 1));
    //            //Debug.Log("Tile Hit: " + topLeftNeighbour.name);
    //            MeshRenderer blmr = bottomLeftNeighbour.GetComponentInChildren<MeshRenderer>();
    //            blmr.material.color = Color.white;

    //            GameObject bottomRightNeighbour = GameObject.Find("Hex_" + (x) + "_" + (y - 1));
    //            //Debug.Log("Tile Hit: " + topRightNeighbour.name);
    //            MeshRenderer brmr = bottomRightNeighbour.GetComponentInChildren<MeshRenderer>();
    //            brmr.material.color = Color.white;
    //        }
    //        return
    //            null;
    //    }
    #endregion
}
