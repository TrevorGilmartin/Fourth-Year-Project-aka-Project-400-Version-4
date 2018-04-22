using QPath;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AIEnemyCreation
{
    Stalling,
    AIEnemyTheFirst,
    AIEnemyTheSecond,
    AIEnemyTheThird,
    AIEnemyTheFourth,
}

public class MapGeneratorScript : MonoBehaviour, IQPathWorldScript
{
    public static MapGeneratorScript instance;

    public GameObject hexPrefab;

    //Size of the map in terms of number of hex tiles
    //This is NOT represtative of the amount of
    //world space that we're going to take up
    //i.e. our tiles might be more or less than 1 unity world unit
    public int numColumns = 20;
    public int numRows = 20;

    //public float xOffset = 0.882f;
    //public float zOffest = .764f;

    [Range(0, 1)]
    public float outlinePercent;
    [Range(-90, 90)]
    public float tileRotation;
    [Range(-90, 90)]
    public float pivotRotation = 42f;

    public HexScript[,] hexes;
    private Dictionary<HexScript, GameObject> hexToGameObjectMap;

    public HashSet<UnitScript> units;
    private Dictionary<UnitScript, GameObject> unitToGameObjectMap;

    //public HashSet<AIUnitScript> aiUnits;
    public List<AIUnitScript> aiUnits;
    public HashSet<GameObject> aiUnitGameObjects;
    public Dictionary<AIUnitScript, GameObject> aiUnitToGameObjectMap;
    public List<GameObject> aiUnitToGameObjectMapList;

    private AIEnemyCreation currentAICreation;

    public GameObject unitPlumberPrefab;

    private int enemyCount;

    public HexScript GetHexAt(int x, int y)
    {
        if (hexes == null)
        {
            Debug.LogError("Hexes array not yet instantiated!");
            return null;
        }

        try
        {
            return hexes[x, y];
        }
        catch
        {
            Debug.LogError("GetHexAt: " + x + "," + y);
            return null;
        }
    }

    public Vector3 GetHexPosition(int c, int r)
    {
        HexScript h = GetHexAt(c, r);

        return GetHexPosition(h);
    }

    public Vector3 GetHexPosition(HexScript Hex)
    {
        return Hex.PositionFromCamera(Camera.main.transform.position, numRows, numColumns);
    }
    // Use this for initialization
    void Start()
    {
        if (aiUnits == null)
        {
            aiUnits = new List<AIUnitScript>();

            aiUnitToGameObjectMap = new Dictionary<AIUnitScript, GameObject>();
        }

        currentAICreation = AIEnemyCreation.AIEnemyTheFirst;
        enemyCount = 0;
        instance = this;
        GenerateMap();
        UnitScript unit = new UnitScript();
        SpawnUnitAt(unit, unitPlumberPrefab, 10, 10);
        AIUnitScript aiUnit = new AIUnitScript();
        AIUnitScript aiUnit2 = new AIUnitScript();
        AIUnitScript aiUnit3 = new AIUnitScript();
        SpawnAIUnitAt(aiUnit, unitPlumberPrefab, 17, 17);
        currentAICreation = AIEnemyCreation.AIEnemyTheSecond;
        SpawnAIUnitAt(aiUnit2, unitPlumberPrefab, 2, 17);
        currentAICreation = AIEnemyCreation.AIEnemyTheThird;
        SpawnAIUnitAt(aiUnit3, unitPlumberPrefab, 17, 2);

        foreach (var item in aiUnits)
        {
            Debug.LogError(item);
        }

        //Debug.LogError(-Mathf.Infinity);
    }

    public void GenerateMap()
    {
        string holderName = "Generator Map";

        if (transform.Find(holderName))
        {
            DestroyImmediate(transform.Find(holderName).gameObject);
        }

        Transform mapHolder = new GameObject(holderName).transform;
        mapHolder.parent = transform;

        //tileRotations = Random.Range(-90, 90);

        hexes = new HexScript[numColumns, numRows];
        hexToGameObjectMap = new Dictionary<HexScript, GameObject>();

        for (int column = 0; column < numColumns; column++)
        {
            for (int row = 0; row < numRows; row++)
            {
                HexScript h = new HexScript(this, column, row);
                hexes[column, row] = h;

                h.movementCost = 1;

                GameObject hex_go = Instantiate(hexPrefab, h.Position(), Quaternion.identity, this.transform);

                if (h == GetHexAt(3, 4) || h == GetHexAt(4, 3) || h == GetHexAt(5, 2) || h == GetHexAt(15, 16) || h == GetHexAt(14, 17) || h == GetHexAt(13, 18) || h == GetHexAt(12, 19))
                {
                    h.movementCost = -99;
                    hex_go.GetComponentInChildren<MeshRenderer>().material.color = Color.black;
                }
                if (h == GetHexAt(9, 10))
                {
                    h.movementCost = 2;
                    hex_go.GetComponentInChildren<MeshRenderer>().material.color = Color.black;
                }
                if (h == GetHexAt(6, 9) || h == GetHexAt(7, 9) || h == GetHexAt(8, 9) || h == GetHexAt(9, 9) || h == GetHexAt(10, 9))
                {
                    h.movementCost = 3;
                    hex_go.GetComponentInChildren<MeshRenderer>().material.color = Color.black;
                }
                else if (h == GetHexAt(2, 5))
                {
                    h.movementCost = 10;
                    hex_go.GetComponentInChildren<MeshRenderer>().material.color = Color.green;
                }

                hexToGameObjectMap.Add(h, hex_go);

                hex_go.GetComponent<HexComponentScript>().hex = new HexScript(this, column, row);
                hex_go.GetComponent<HexComponentScript>().map = this;

                //hex_go.GetComponentInChildren<TextMesh>().text = string.Format("{0},{1}\n\t{2}", column, row, h.movementCost);

                hex_go.transform.localScale = Vector3.one * (1 - outlinePercent);

                //Rotation of the tiles around the Y axis of their creation             
                var rotationVector = hex_go.transform.rotation.eulerAngles;
                rotationVector.y = tileRotation;
                hex_go.transform.localRotation = Quaternion.Euler(rotationVector);

                //Gives the game object a suitable name for debugging
                hex_go.name = "Hex_" + column + "_" + row;

                //Used to create a cleaner hierachy, thus parenting this hex.
                hex_go.transform.SetParent(mapHolder);

                hex_go.isStatic = true;
                #region My Attempt
                //float xPos = column * xOffset;

                ////Are we on a odd row
                //if (row % 2 == 1)
                //{
                //    xPos += xOffset / 2f;
                //}

                //Vector3 tileposition = new Vector3(xPos, 0, row * zOffest);
                //GameObject hex_go = (GameObject)Instantiate(hexPrefab, tileposition, Quaternion.identity);

                //hex_go.transform.localScale = Vector3.one * (1 - outlinePercent);

                ////Rotation of the tiles around the Y axis of their creation             
                //var rotationVector = hex_go.transform.rotation.eulerAngles;
                //rotationVector.y = tileRotation;
                //hex_go.transform.localRotation = Quaternion.Euler(rotationVector);

                ////Gives the game object a suitable name for debugging
                //hex_go.name = "Hex_" + column + "_" + row;

                ////Give the Hex awareness of it's place in the array
                //hex_go.GetComponent<HexScript>().x = column;
                //hex_go.GetComponent<HexScript>().y = row;
                //hex_go.GetComponent<HexScript>().tilesWidthMax = width;
                //hex_go.GetComponent<HexScript>().tilesHeightMax = height;

                ////Used to create a cleaner hierachy, thus parenting this hex.
                //hex_go.transform.SetParent(mapHolder);

                //hex_go.isStatic = true;
                #endregion
            }
        }

        GameObject mapGenerator = GameObject.Find(holderName);
        var pivotRotationVector = mapGenerator.transform.rotation.eulerAngles;
        pivotRotationVector.y = pivotRotation;
        mapGenerator.transform.localRotation = Quaternion.Euler(pivotRotationVector);
    }

    public void UpdateHexTexture()
    {

    }

    public void SpawnUnitAt(UnitScript unit, GameObject prefab, int q, int r)
    {
        if (units == null)
        {
            units = new HashSet<UnitScript>();
            unitToGameObjectMap = new Dictionary<UnitScript, GameObject>();
        }

        GameObject myHex_go = hexToGameObjectMap[GetHexAt(q, r)];
        unit.SetHex(GetHexAt(q, r));
        GameObject unitPlumber_go = (GameObject)Instantiate(prefab, new Vector3(myHex_go.transform.position.x, 1, myHex_go.transform.position.z), Quaternion.identity, myHex_go.transform);
        unit.OnUnitMoved += unitPlumber_go.GetComponent<UnitViewScript>().OnUnitMoved;
        unit.ReceivedName += unitPlumber_go.GetComponent<UnitViewScript>().ReceiveName;
        unit.name = "PlayerPlumber";
        unit.SetName();

        units.Add(unit);
        unitToGameObjectMap.Add(unit, unitPlumber_go);
    }

    public void SpawnAIUnitAt(AIUnitScript aiUnit, GameObject prefab, int q, int r)
    {
        #region Attempt at Spawning procedures
        if (currentAICreation == AIEnemyCreation.AIEnemyTheFirst)
        {
            AIUnitScript aiUnitTheFirst = aiUnit;
            GameObject myHex_go = hexToGameObjectMap[GetHexAt(q, r)];
            aiUnitTheFirst.SetHex(GetHexAt(q, r));
            GameObject unitPlumberTheFirst_go = (GameObject)Instantiate(prefab, new Vector3(myHex_go.transform.position.x, 1, myHex_go.transform.position.z), Quaternion.identity, myHex_go.transform);
            aiUnitTheFirst.OnUnitMoved += unitPlumberTheFirst_go.GetComponent<UnitViewScript>().OnUnitMoved;
            aiUnitTheFirst.ReceivedName += unitPlumberTheFirst_go.GetComponent<UnitViewScript>().ReceiveName;
            aiUnitTheFirst.ReceivedNumber += unitPlumberTheFirst_go.GetComponent<UnitViewScript>().ReceiveNumber;
            aiUnitTheFirst.name = "AIAggressivePlumber";
            aiUnitTheFirst.SetName();
            aiUnitTheFirst.enemyNumber = 1;
            aiUnitTheFirst.SetNumber();

            aiUnits.Insert(enemyCount, aiUnitTheFirst);
            //aiUnitGameObjects.Add(unitPlumber_go);

            aiUnitToGameObjectMapList.Add(unitPlumberTheFirst_go);
            //aiUnitToGameObjectMap.Add(aiUnitTheFirst, unitPlumberTheFirst_go);

            enemyCount++;
            currentAICreation = AIEnemyCreation.Stalling;
        }

        if (currentAICreation == AIEnemyCreation.AIEnemyTheSecond)
        {
            List<AIUnitScript> test2 = aiUnits;
            AIUnitScript aiUnitTheSecond = aiUnit;
            GameObject myHex_go = hexToGameObjectMap[GetHexAt(q, r)];
            aiUnitTheSecond.SetHex(GetHexAt(q, r));
            GameObject unitPlumberTheSecond_go = (GameObject)Instantiate(prefab, new Vector3(myHex_go.transform.position.x, 1, myHex_go.transform.position.z), Quaternion.identity, myHex_go.transform);
            aiUnitTheSecond.OnUnitMoved += unitPlumberTheSecond_go.GetComponent<UnitViewScript>().OnUnitMoved;
            aiUnitTheSecond.ReceivedName += unitPlumberTheSecond_go.GetComponent<UnitViewScript>().ReceiveName;
            aiUnitTheSecond.ReceivedNumber += unitPlumberTheSecond_go.GetComponent<UnitViewScript>().ReceiveNumber;
            aiUnitTheSecond.name = "AIDocilePlumber";
            aiUnitTheSecond.SetName();
            List<AIUnitScript> test = aiUnits;
            aiUnitTheSecond.enemyNumber = 2;
            aiUnitTheSecond.SetNumber();


            aiUnits.Insert(enemyCount, aiUnitTheSecond);
            //aiUnitGameObjects.Add(unitPlumber_go);

            aiUnitToGameObjectMapList.Add(unitPlumberTheSecond_go);
            //aiUnitToGameObjectMap.Add(aiUnitTheSecond, unitPlumberTheSecond_go);

            enemyCount++;
            currentAICreation = AIEnemyCreation.Stalling;
        }

        if (currentAICreation == AIEnemyCreation.AIEnemyTheThird)
        {
            if (currentAICreation == AIEnemyCreation.AIEnemyTheThird)
            {
                AIUnitScript aiUnitTheThird = aiUnit;
                GameObject myHex_go = hexToGameObjectMap[GetHexAt(q, r)];
                aiUnitTheThird.SetHex(GetHexAt(q, r));
                GameObject unitPlumberTheFirst_go = (GameObject)Instantiate(prefab, new Vector3(myHex_go.transform.position.x, 1, myHex_go.transform.position.z), Quaternion.identity, myHex_go.transform);
                aiUnitTheThird.OnUnitMoved += unitPlumberTheFirst_go.GetComponent<UnitViewScript>().OnUnitMoved;
                aiUnitTheThird.ReceivedName += unitPlumberTheFirst_go.GetComponent<UnitViewScript>().ReceiveName;
                aiUnitTheThird.ReceivedNumber += unitPlumberTheFirst_go.GetComponent<UnitViewScript>().ReceiveNumber;
                aiUnitTheThird.name = "AIDesperatePlumber";
                aiUnitTheThird.SetName();
                aiUnitTheThird.enemyNumber = 3;
                aiUnitTheThird.SetNumber();

                aiUnits.Insert(enemyCount, aiUnitTheThird);
                //aiUnitGameObjects.Add(unitPlumber_go);

                aiUnitToGameObjectMapList.Add(unitPlumberTheFirst_go);
                //aiUnitToGameObjectMap.Add(aiUnitTheFirst, unitPlumberTheFirst_go);

                enemyCount++;
                currentAICreation = AIEnemyCreation.Stalling;
            }
        }

        if (currentAICreation == AIEnemyCreation.AIEnemyTheFourth)
        {
            GameObject myHex_go = hexToGameObjectMap[GetHexAt(q, r)];
            aiUnit.SetHex(GetHexAt(q, r));
            GameObject unitPlumberTheFourth_go = (GameObject)Instantiate(prefab, new Vector3(myHex_go.transform.position.x, 1, myHex_go.transform.position.z), Quaternion.identity, myHex_go.transform);
            aiUnit.OnUnitMoved += unitPlumberTheFourth_go.GetComponent<UnitViewScript>().OnUnitMoved;
            aiUnit.ReceivedName += unitPlumberTheFourth_go.GetComponent<UnitViewScript>().ReceiveName;
            aiUnit.ReceivedNumber += unitPlumberTheFourth_go.GetComponent<UnitViewScript>().ReceiveNumber;
            aiUnit.name = "AIDesperatePlumber";
            aiUnit.SetName();
            aiUnit.enemyNumber = 4;
            aiUnit.SetNumber();

            aiUnits.Add(aiUnit);
            //aiUnitGameObjects.Add(unitPlumber_go);

            AIUnitScript aiUnitTheFourth = aiUnit;
            aiUnitToGameObjectMapList.Add(unitPlumberTheFourth_go);
            //aiUnitToGameObjectMap.Add(aiUnitTheFourth, unitPlumberTheFourth_go);

            currentAICreation = AIEnemyCreation.Stalling;
        }
        #endregion
        #region attempt 2
        //if (aiUnits == null)
        //{
        //    aiUnits = new HashSet<AIUnitScript>();

        //    aiUnitToGameObjectMap = new Dictionary<AIUnitScript, GameObject>();
        //}

        //GameObject myHex_go = hexToGameObjectMap[GetHexAt(q, r)];
        //aiUnit.SetHex(GetHexAt(q, r));
        //GameObject unitPlumber_go = (GameObject)Instantiate(prefab, new Vector3(myHex_go.transform.position.x, 1, myHex_go.transform.position.z), Quaternion.identity, myHex_go.transform);
        //aiUnit.OnUnitMoved += unitPlumber_go.GetComponent<UnitViewScript>().OnUnitMoved;
        //aiUnit.ReceivedName += unitPlumber_go.GetComponent<UnitViewScript>().ReceiveName;
        //aiUnit.ReceivedNumber += unitPlumber_go.GetComponent<UnitViewScript>().ReceiveNumber;
        //aiUnit.name = "AIDesperatePlumber";
        //aiUnit.SetName();
        //aiUnit.enemyNumber = enemyCount;
        //aiUnit.SetNumber();

        //aiUnits.Add(aiUnit);
        //aiUnitToGameObjectMapList.Add(unitPlumber_go);
        #endregion
    }


    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (units != null)
            {
                foreach (UnitScript u in units)
                {
                    u.DoTurn();
                    Debug.Log(u.Hex);
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            if (units != null)
            {
                foreach (UnitScript u in units)
                {
                    u.DUMMY_PATHING_FUNCTION();
                }
            }
        }
    }
}
