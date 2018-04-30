using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MouseManagerScript : MonoBehaviour
{
    public static MouseManagerScript instance;

    //Generic Bookkeeping Variables
    Vector3 LastMousePosition; // From Input.mousePosition

    //Camera Bragging Bookkeeping variables
    private HexScript[] hexes;
    private HexScript[] neighbouringHexes;
    //bool isDraggingCamera = false;
    Vector3 lastMouseGroundPlanePosition;
    int mouseDragThreshold = 1;

    //Unit Movement
    private UnitScript selectedUnit = null;

    delegate void UpdateFunc();
    UpdateFunc Update_CurrentFunc;
    private bool btnMoveActivation = false;
    private bool canvasActivated = false;

    //private bool potentialSelectedHexSwitch = false;
    private Color potentialSelectedHexOriginalColour;
    private GameObject potentialSelectedHexCashe_go = null;
    private int potentalSelectedHexColumnPosition;
    private int potentalSelectedHexRowPosition;

    public bool potentialHexSelected = false;
    public int selectedHexColumnPositioningDifference;
    public int selectedHexRowPositioningDifference;

    private bool movementActivated = false;
    private bool pathBegun = false;
    public float movementDelay;
    private int pathCount;

    private List<GameObject> selectedHexes = new List<GameObject>();
    private List<GameObject> selfSelectedHexes = new List<GameObject>();
    private List<HexScript> hexPath = new List<HexScript>();

    public bool selfHexSelectionInitiated;
    private bool firstTime = true;
    private bool firstTimeSettingHexPath = true;

    //Attack Action The First Variables
    private HexScript[] attackingHexes;
    private bool attackTheFirstInitiated = false;
    private List<GameObject> selectedAttackingHexes = new List<GameObject>();
    private bool attackable = false;

    // Use this for initialization
    void Start()
    {
        instance = this;
        Update_CurrentFunc = Update_DetectModeStart;
    }

    private void Update()
    {
        Update_CurrentFunc();
        Update_PlayerControlActivation();
        Update_CameraZoomScrolling();

        if (movementActivated && selfHexSelectionInitiated && firstTimeSettingHexPath)
        {
            foreach (var item in selfSelectedHexes)
            {
                hexPath.Add(item.GetComponent<HexComponentScript>().hex);
            }
            selectedUnit.SetSelfSelectedHexPath(hexPath.ToArray());

            firstTimeSettingHexPath = false;
        }

        if (StateManagerScript.instance.currentTurnState == TurnsState.PlayerTurn)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CancelUpdateFunc();
            }

            if (!selfHexSelectionInitiated)
            {
                Update_HexSelection();
                Update_UnitMovement();
                Update_PlayerAttackActionTheFirst();
                LastMousePosition = Input.mousePosition;

                if (pathBegun && selectedUnit.MovementRemaining >= 0)
                    movementDelay += Time.deltaTime;

                if (movementDelay > 1f)
                    pathBegun = false;
            }
            else
            {
                Update_SelfHexSelection();
                Update_UnitMovement();
                Update_PlayerAttackActionTheFirst();
                LastMousePosition = Input.mousePosition;

                if (pathBegun)
                    movementDelay += Time.deltaTime;

                if (movementDelay > 1f)
                    pathBegun = false;
            }

            #region Main Camera Test
            //Get the first in the scene that has the "Tag" Main Camera
            //This only works in orthographic, only give us the
            //world position on the same plane as the camera's
            //near clipping play. (i.e It's not helpful for our application.
            //Vector3 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            //Debug.Log("World Point: "+ worldPoint);
            #endregion
        }
        else
        {

        }

    }

    void CancelUpdateFunc()
    {
        Update_CurrentFunc = Update_DetectModeStart;

        //Also do cleanup of any UI stuff associated with modes
    }

    void Update_DetectModeStart()
    {
        if (Input.GetMouseButtonDown(1))
        {
            //Left Mouse button just went down
            //This doesn't do anything by itself
            Debug.Log("Mouse Down");
        }
        else if (Input.GetMouseButtonUp(1))
        {
            //TODO: Are we clicking on a hex with a unit
            // If so, select it
        }
        else if (Input.GetMouseButton(1) && Vector3.Distance(Input.mousePosition, LastMousePosition) > mouseDragThreshold)
        {
            //Left button is being held down and the move moved
            //Thus this is a camera drag
            Update_CurrentFunc = Update_CameraDrag;
            lastMouseGroundPlanePosition = MouseToGroundPlane(Input.mousePosition);
            Update_CurrentFunc();
        }
        else if (selectedUnit != null && Input.GetMouseButton(0))
        {
            Update_PlayerControlActivation();
        }

    }

    void Update_CameraZoomScrolling()
    {
        if (Input.GetMouseButtonUp(2))
        {
            CancelUpdateFunc();
            return;
        }
        float scrollingAmount = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scrollingAmount) > 0.01f)
        {
            float minHeight = 5;
            float maxHeight = 20;
            Vector3 direction = MouseToGroundPlane(Input.mousePosition) - Camera.main.transform.position;
            Vector3 position = Camera.main.transform.position;

            if (scrollingAmount > 0 || position.y < 19.9)
            {
                Camera.main.transform.Translate(direction * scrollingAmount, Space.World);
            }

            position = Camera.main.transform.position;

            if (position.y < minHeight)
            {
                position.y = minHeight;
            }
            if (position.y > maxHeight)
            {
                position.y = maxHeight;
            }
            Camera.main.transform.position = position;

            float lowZoom = minHeight + 3;
            float highZoom = maxHeight - 10;

            Camera.main.transform.rotation = Quaternion.Euler(
                    Mathf.Lerp(10, 90, position.y / (maxHeight / 1.5f)),
                    Camera.main.transform.rotation.eulerAngles.y,
                    Camera.main.transform.rotation.eulerAngles.z
                    );
            #region Angle Zoom Attempt 1
            //if (position.y < lowZoom)
            //{
            //    Camera.main.transform.rotation = Quaternion.Euler(
            //        Mathf.Lerp(10, 60, (position.y - minHeight) / (lowZoom - minHeight)),
            //        Camera.main.transform.rotation.eulerAngles.y,
            //        Camera.main.transform.rotation.eulerAngles.z
            //        );
            //}
            //else if (position.y > highZoom)
            //{
            //    Camera.main.transform.rotation = Quaternion.Euler(
            //        Mathf.Lerp(60, 90, ((position.y - highZoom) / (maxHeight - highZoom))),
            //        Camera.main.transform.rotation.eulerAngles.y,
            //        Camera.main.transform.rotation.eulerAngles.z
            //        );
            //}
            //else
            //{
            //    Camera.main.transform.rotation = Quaternion.Euler(
            //        60,
            //        Camera.main.transform.rotation.eulerAngles.y,
            //        Camera.main.transform.rotation.eulerAngles.z
            //        );
            //}
            #endregion
        }
    }

    // Update is called once per frame
    void Update_CameraDrag()
    {
        if (Input.GetMouseButtonUp(1))
        {
            Debug.Log("Cancelling camera drag.");
            CancelUpdateFunc();
            return;
        }

        // Right now, all we need are camera controls

        Vector3 hitPos = MouseToGroundPlane(Input.mousePosition);

        Vector3 diff = lastMouseGroundPlanePosition - hitPos;
        Camera.main.transform.Translate(diff, Space.World);

        lastMouseGroundPlanePosition = hitPos = MouseToGroundPlane(Input.mousePosition);
    }

    void Update_PlayerControlActivation()/*Formally Update_NeighbouringSystem()*/
    {
        if (StateManagerScript.instance.currentTurnState == TurnsState.PlayerTurn)
        {
            #region Neighbouring System
            RaycastHit hitInfo;

            //Sends a ray from the centre of the camera to the point sent into the function and continues 
            //in that direction
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            //Checks to see if the ray has hit an object
            //Fills out the hitInfo class as to tell you what collider you hit
            if (Physics.Raycast(ray, out hitInfo) && !EventSystem.current.IsPointerOverGameObject()/*Is used to check to see if mouse is over any UI*/)
            {
                //This is in reference to the Hextile which is inside the GameObject HexTile_0_0
                GameObject currentHitModelObject = hitInfo.collider.transform.gameObject;
                //This is in reference to the Hextiles parent which is HexTile_0_0
                GameObject currentHitParentObject = hitInfo.collider.transform.parent.gameObject;

                //hitInfo.collider.transform.parent.name is done so it gets the name of the object
                //my parnet of my model which was an empty gameobject created so at all times it would be centred
                //Debug.Log("Raycast hit: " + hitInfo.collider.transform.parent.name);
                //Debug.Log("Raycast hit: " + currentHitModelObject.name);

                #region Notes/Thoughts
                //We know what we're mousing over.
                //What if we wanted to show a tooltip or have an
                //action occur

                //WE could could check to see if we're clicking
                #endregion
                //MeshRenderer mr = currentHitModelObject.GetComponentInChildren<MeshRenderer>();
                //Color originalColor = mr.material.color;

                #region attemptToGetCharacter
                GameObject tempHex_GoCharacter = GameObject.Find("group2");


                if (hitInfo.collider.transform.parent.name == tempHex_GoCharacter.name.ToString())
                {
                    Color originalColor = tempHex_GoCharacter.GetComponentInChildren<MeshRenderer>().material.color;
                    Debug.Log(true);
                }

                #endregion

                if (Input.GetMouseButtonDown(0) && hitInfo.collider.transform.parent.name == tempHex_GoCharacter.name.ToString() && currentHitModelObject.GetComponentInParent<UnitViewScript>().Name == "PlayerPlumber")
                {
                    CanvasManagerScript._instance.Activation();
                    canvasActivated = true;

                    if (!movementActivated)
                    {
                        foreach (var unit in MapGeneratorScript.instance.units)
                        {
                            selectedUnit = unit;
                        }

                        GameObject tempHex_go = GameObject.Find("Hex_" + selectedUnit.Hex.C + "_" + selectedUnit.Hex.R);

                        //hexes = tempHex_GoCharacter.GetComponentInParent<UnitScript>().Hex.GetNeighbours(currentHitParentObject, 2, MapGeneratorScript.instance.numColumns, MapGeneratorScript.instance.numRows);
                        //hexes = currentHitParentObject.GetComponent<HexComponentScript>().hex.GetNeighbours(currentHitParentObject, 2, MapGeneratorScript.instance.numColumns, MapGeneratorScript.instance.numRows);
                        hexes = selectedUnit.Hex.GetNeighbours(tempHex_go, 5, MapGeneratorScript.instance.numColumns, MapGeneratorScript.instance.numRows);
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
                    }
                }

                else if (/*Input.GetMouseButtonUp(0) && hitInfo.collider.transform.parent.name == tempHex_GoCharacter.name.ToString() || */
                    Input.GetKeyDown(KeyCode.Escape))
                {
                    CanvasManagerScript._instance.Deactivation();
                    canvasActivated = false;
                    movementActivated = false;

                    #region original code for unselection
                    //foreach (var unit in MapGeneratorScript.instance.units)
                    //{
                    //    selectedUnit = unit;
                    //}

                    //GameObject tempHex_go = GameObject.Find("Hex_" + selectedUnit.Hex.C + "_" + selectedUnit.Hex.R);

                    ////hexes = tempHex_GoCharacter.GetComponentInParent<UnitScript>().Hex.GetNeighbours(currentHitParentObject, 2, MapGeneratorScript.instance.numColumns, MapGeneratorScript.instance.numRows);
                    ////hexes = currentHitParentObject.GetComponent<HexComponentScript>().hex.GetNeighbours(currentHitParentObject, 2, MapGeneratorScript.instance.numColumns, MapGeneratorScript.instance.numRows);
                    //hexes = selectedUnit.Hex.GetNeighbours(tempHex_go, 5, MapGeneratorScript.instance.numColumns, MapGeneratorScript.instance.numRows);
                    //foreach (HexScript hex in hexes)
                    //{
                    //    GameObject tempHexNeighbour_go = GameObject.Find("Hex_" + hex.C + "_" + hex.R);
                    //    if (hex.movementCost == 1)
                    //        tempHexNeighbour_go.transform.gameObject.GetComponentInChildren<MeshRenderer>().material.color = Color.white;
                    //}
                    //tempHex_go.GetComponentInChildren<MeshRenderer>().material.color = Color.white;
                    #endregion
                    foreach (var hex in selectedHexes)
                    {
                        hex.transform.gameObject.GetComponentInChildren<MeshRenderer>().material.color = Color.white;
                    }
                }
                else
                {

                }
            }
            #endregion
        }
    }

    void Update_HexSelection()
    {
        if (StateManagerScript.instance.currentTurnState == TurnsState.PlayerTurn)
        {
            if (canvasActivated)
            {
                #region hitDetection
                RaycastHit hitInfo;

                //Sends a ray from the centre of the camera to the point sent into the function and continues 
                //in that direction
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                //Checks to see if the ray has hit an object
                //Fills out the hitInfo class as to tell you what collider you hit
                if (Physics.Raycast(ray, out hitInfo) && !EventSystem.current.IsPointerOverGameObject()/*Is used to check to see if mouse is over any UI*/)
                {
                    //This is in reference to the Hextile which is inside the GameObject HexTile_0_0
                    GameObject currentHitModelObject = hitInfo.collider.transform.gameObject;
                    //This is in reference to the Hextiles parent which is HexTile_0_0
                    GameObject currentHitParentObject = hitInfo.collider.transform.parent.gameObject;

                    //hitInfo.collider.transform.parent.name is done so it gets the name of the object
                    //my parnet of my model which was an empty gameobject created so at all times it would be centred
                    //Debug.Log("Raycast hit: " + hitInfo.collider.transform.parent.name);
                    //Debug.Log("Raycast hit: " + currentHitModelObject.name);

                    #region Notes/Thoughts
                    //We know what we're mousing over.
                    //What if we wanted to show a tooltip or have an
                    //action occur

                    //WE could could check to see if we're clicking
                    #endregion
                    //MeshRenderer mr = currentHitModelObject.GetComponentInChildren<MeshRenderer>();
                    //Color originalColor = mr.material.color;
                    #endregion

                    #region attemptToGetCharacter
                    GameObject tempHex_GoCharacter = GameObject.Find("group2");
                    Color originalColor = tempHex_GoCharacter.GetComponentInChildren<MeshRenderer>().material.color;

                    if (hitInfo.collider.transform.parent.name == tempHex_GoCharacter.name.ToString())
                        Debug.Log(true);

                    #endregion

                    foreach (var h in hexes)
                    {
                        if (Input.GetMouseButtonDown(0) && hitInfo.collider.transform.parent.name == "Hex_" + h.C + "_" + h.R)
                        {
                            GameObject potentialHexSelection_go = GameObject.Find("Hex_" + h.C + "_" + h.R);
                            potentalSelectedHexColumnPosition = h.C;
                            potentalSelectedHexRowPosition = h.R;

                            if (h.movementCost == 1)
                            {
                                if (potentialSelectedHexCashe_go != null)
                                {
                                    potentialSelectedHexCashe_go.transform.gameObject.GetComponentInChildren<MeshRenderer>().material.color = potentialSelectedHexOriginalColour;
                                    potentialSelectedHexOriginalColour = potentialHexSelection_go.transform.gameObject.GetComponentInChildren<MeshRenderer>().material.color;
                                    potentialHexSelection_go.transform.gameObject.GetComponentInChildren<MeshRenderer>().material.color = Color.blue;
                                    potentialSelectedHexCashe_go = potentialHexSelection_go;
                                    PotentialEnergyDifference();
                                }
                                else
                                {
                                    potentialSelectedHexOriginalColour = potentialHexSelection_go.transform.gameObject.GetComponentInChildren<MeshRenderer>().material.color;
                                    potentialHexSelection_go.transform.gameObject.GetComponentInChildren<MeshRenderer>().material.color = Color.blue;
                                    potentialSelectedHexCashe_go = potentialHexSelection_go;
                                    potentialHexSelected = true;
                                    PotentialEnergyDifference();
                                }
                            }
                            Debug.Log("It Works");
                        }
                        else if (Input.GetKeyDown(KeyCode.Escape))
                        {
                            potentialSelectedHexCashe_go.transform.gameObject.GetComponentInChildren<MeshRenderer>().material.color = potentialSelectedHexOriginalColour;
                        }
                    }
                }
            }
        }
    }

    public void PotentialEnergyDifference()
    {
        selectedHexColumnPositioningDifference = potentalSelectedHexColumnPosition - selectedUnit.Hex.C;
        selectedHexRowPositioningDifference = potentalSelectedHexRowPosition - selectedUnit.Hex.R;

        selectedUnit.PotentialEnergyUsage();
    }

    void Update_SelfHexSelection()
    {
        if (StateManagerScript.instance.currentTurnState == TurnsState.PlayerTurn)
        {
            if (canvasActivated)
            {

                #region hitDetection
                RaycastHit hitInfo;

                //Sends a ray from the centre of the camera to the point sent into the function and continues 
                //in that direction
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                //Checks to see if the ray has hit an object
                //Fills out the hitInfo class as to tell you what collider you hit
                if (Physics.Raycast(ray, out hitInfo) && !EventSystem.current.IsPointerOverGameObject()/*Is used to check to see if mouse is over any UI*/)
                {
                    //This is in reference to the Hextile which is inside the GameObject HexTile_0_0
                    GameObject currentHitModelObject = hitInfo.collider.transform.gameObject;
                    //This is in reference to the Hextiles parent which is HexTile_0_0
                    GameObject currentHitParentObject = hitInfo.collider.transform.parent.gameObject;

                    //hitInfo.collider.transform.parent.name is done so it gets the name of the object
                    //my parnet of my model which was an empty gameobject created so at all times it would be centred
                    //Debug.Log("Raycast hit: " + hitInfo.collider.transform.parent.name);
                    //Debug.Log("Raycast hit: " + currentHitModelObject.name);

                    #region Notes/Thoughts
                    //We know what we're mousing over.
                    //What if we wanted to show a tooltip or have an
                    //action occur

                    //WE could could check to see if we're clicking
                    #endregion
                    //MeshRenderer mr = currentHitModelObject.GetComponentInChildren<MeshRenderer>();
                    //Color originalColor = mr.material.color;
                    #endregion

                    #region attemptToGetCharacter
                    GameObject tempHex_GoCharacter = GameObject.Find("group2");
                    Color originalColor = tempHex_GoCharacter.GetComponentInChildren<MeshRenderer>().material.color;

                    if (hitInfo.collider.transform.parent.name == tempHex_GoCharacter.name.ToString() && currentHitModelObject.GetComponentInParent<UnitViewScript>().Name == "PlayerPlumber")
                        Debug.Log(true);

                    if (firstTime)
                    {
                        firstTime = false;
                        GameObject tempHex_go = GameObject.Find("Hex_" + selectedUnit.Hex.C + "_" + selectedUnit.Hex.R);
                        selfSelectedHexes.Add(tempHex_go);
                    }

                    neighbouringHexes = selfSelectedHexes[selfSelectedHexes.Count - 1].GetComponent<HexComponentScript>().hex.GetNeighbours(
                        selfSelectedHexes[selfSelectedHexes.Count - 1], 1, MapGeneratorScript.instance.numColumns, MapGeneratorScript.instance.numRows);

                    foreach (HexScript hex in neighbouringHexes)
                    {
                        GameObject tempHexNeighbour_go = GameObject.Find("Hex_" + hex.C + "_" + hex.R);
                        if (tempHexNeighbour_go.transform.gameObject.GetComponentInChildren<MeshRenderer>().material.color != Color.blue
                            && selectedHexes.Contains(tempHexNeighbour_go))
                        {
                            tempHexNeighbour_go.transform.gameObject.GetComponentInChildren<MeshRenderer>().material.color = Color.green;
                        }
                    }

                    #endregion

                    foreach (var h in neighbouringHexes)
                    {
                        if (Input.GetMouseButtonDown(0) && hitInfo.collider.transform.parent.name == "Hex_" + h.C + "_" + h.R)
                        {
                            GameObject potentialHexSelection_go = GameObject.Find("Hex_" + h.C + "_" + h.R);
                            potentalSelectedHexColumnPosition = h.C;
                            potentalSelectedHexRowPosition = h.R;


                            if (potentialSelectedHexCashe_go != null && selectedHexes.Contains(potentialHexSelection_go))
                            {
                                //potentialSelectedHexCashe_go.transform.gameObject.GetComponentInChildren<MeshRenderer>().material.color = potentialSelectedHexOriginalColour;
                                potentialSelectedHexOriginalColour = potentialHexSelection_go.transform.gameObject.GetComponentInChildren<MeshRenderer>().material.color;
                                potentialHexSelection_go.transform.gameObject.GetComponentInChildren<MeshRenderer>().material.color = Color.blue;
                                potentialSelectedHexCashe_go = potentialHexSelection_go;
                                selfSelectedHexes.Add(potentialHexSelection_go);
                            }
                            else if (selectedHexes.Contains(potentialHexSelection_go))
                            {
                                //potentialSelectedHexOriginalColour = potentialHexSelection_go.transform.gameObject.GetComponentInChildren<MeshRenderer>().material.color;
                                potentialHexSelection_go.transform.gameObject.GetComponentInChildren<MeshRenderer>().material.color = Color.blue;
                                potentialSelectedHexCashe_go = potentialHexSelection_go;
                                selfSelectedHexes.Add(potentialHexSelection_go);
                                potentialHexSelected = true;
                            }
                        }
                        else if (Input.GetKeyDown(KeyCode.Escape))
                        {
                            potentialSelectedHexCashe_go.transform.gameObject.GetComponentInChildren<MeshRenderer>().material.color = potentialSelectedHexOriginalColour;
                        }
                    }
                }

            }
        }
    }

    public void Update_UnitMovementPath()
    {
        if (StateManagerScript.instance.currentTurnState == TurnsState.PlayerTurn && selectedUnit.MovementRemaining >= 0)
        {
            attackTheFirstInitiated = false;
            attackable = false;

            if (!selfHexSelectionInitiated)
            {
                if (potentialHexSelected)
                {
                    selectedHexColumnPositioningDifference = potentalSelectedHexColumnPosition - selectedUnit.Hex.C;
                    selectedHexRowPositioningDifference = potentalSelectedHexRowPosition - selectedUnit.Hex.R;

                    selectedUnit.DUMMY_PATHING_FUNCTION();
                    movementActivated = true;
                    pathBegun = false;
                }
            }
            else
            {
                movementActivated = true;
                pathBegun = false;
            }
        }
    }

    public void Update_UnitMovement()
    {
        if (StateManagerScript.instance.currentTurnState == TurnsState.PlayerTurn && selectedUnit.MovementRemaining >= 0)
        {
            if (!selfHexSelectionInitiated)
            {
                if (movementActivated)
                {
                    if (!pathBegun)
                    {
                        pathBegun = true;
                        movementDelay = 0;
                        selectedUnit.DoTurn();
                    }
                }
            }
            else
            {
                if (movementActivated)
                {
                    if (!pathBegun)
                    {
                        pathBegun = true;
                        movementDelay = 0;
                        selectedUnit.SelfSelectedDoTurn(hexPath.ToArray());
                    }
                }
            }
        }
    }

    public void Update_PlayerAttackActionTheFirst()
    {
        if (!attackTheFirstInitiated)
        {
            if (attackingHexes != null)
            {
                foreach (var hex in selectedAttackingHexes)
                {
                    hex.transform.gameObject.GetComponentInChildren<MeshRenderer>().material.color = Color.yellow;
                }
            }
        }

        if (canvasActivated && attackTheFirstInitiated)
        {
            #region Setting the parameters aka range in which on can attack 
            GameObject tempHex_go = GameObject.Find("Hex_" + selectedUnit.Hex.C + "_" + selectedUnit.Hex.R);

            attackingHexes = selectedUnit.Hex.GetNeighbours(tempHex_go, 1, MapGeneratorScript.instance.numColumns, MapGeneratorScript.instance.numRows);

            foreach (HexScript hex in attackingHexes)
            {
                GameObject tempHexNeighbour_go = GameObject.Find("Hex_" + hex.C + "_" + hex.R);
                if (hex.movementCost == 1)
                {
                    tempHexNeighbour_go.transform.gameObject.GetComponentInChildren<MeshRenderer>().material.color = Color.red;
                    if (!selectedAttackingHexes.Contains(tempHexNeighbour_go) || selectedAttackingHexes == null)
                    {
                        selectedAttackingHexes.Add(tempHexNeighbour_go);
                    }
                }
            }
            tempHex_go.GetComponentInChildren<MeshRenderer>().material.color = Color.yellow;
            #endregion

            #region Is Something Attackable
            foreach (var aiUnit in MapGeneratorScript.instance.aiUnits)
            {
                foreach (var hex in attackingHexes)
                {
                    if (aiUnit.Hex == hex)
                    {
                        GameObject tempHexPotenitalAttackie_go = GameObject.Find("Hex_" + hex.C + "_" + hex.R);
                        tempHexPotenitalAttackie_go.transform.gameObject.GetComponentInChildren<MeshRenderer>().material.color = Color.cyan;
                        attackable = true;
                    }
                }
            }
            #endregion
            RaycastHit hitInfo;

            //Sends a ray from the centre of the camera to the point sent into the function and continues 
            //in that direction
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            //Checks to see if the ray has hit an object
            //Fills out the hitInfo class as to tell you what collider you hit
            if (Physics.Raycast(ray, out hitInfo))
            {
                //This is in reference to the Hextile which is inside the GameObject HexTile_0_0
                GameObject currentHitModelObject = hitInfo.collider.transform.gameObject;
                //This is in reference to the Hextiles parent which is HexTile_0_0
                GameObject currentHitParentObject = hitInfo.collider.transform.parent.gameObject;

                //hitInfo.collider.transform.parent.name is done so it gets the name of the object
                //my parnet of my model which was an empty gameobject created so at all times it would be centred
                //Debug.Log("Raycast hit: " + hitInfo.collider.transform.parent.name);
                //Debug.Log("Raycast hit: " + currentHitModelObject.name);

                #region Notes/Thoughts
                //We know what we're mousing over.
                //What if we wanted to show a tooltip or have an
                //action occur

                //WE could could check to see if we're clicking
                #endregion
                //MeshRenderer mr = currentHitModelObject.GetComponentInChildren<MeshRenderer>();
                //Color originalColor = mr.material.color;

                #region attemptToGetCharacter
                GameObject tempHex_GoCharacter = GameObject.Find("group2");
                #endregion
                Debug.LogError(currentHitModelObject.GetComponentInParent<UnitViewScript>().Name);
                if (Input.GetMouseButtonDown(0) && hitInfo.collider.transform.parent.name == tempHex_GoCharacter.name.ToString())
                {
                    foreach (var aiUnit in MapGeneratorScript.instance.aiUnits)
                    {
                        foreach (var hex in attackingHexes)
                        {
                            if (aiUnit.Hex == hex)
                            {
                                aiUnit.HitPoints -= 1000;
                            }

                            if (aiUnit.HitPoints <= 0)
                            {
                                StateManagerScript.instance.DoSelfDestructionMode(aiUnit);
                            }
                        }
                    }
                }
            }
        }
    }

    #region Helpers
    Vector3 MouseToGroundPlane(Vector3 mousePos)
    {
        Ray mouseRay = Camera.main.ScreenPointToRay(mousePos);
        // What is the point at which the mouse ray intersects Y=0
        if (mouseRay.direction.y >= 0)
        {
            //Debug.LogError("Why is mouse pointing up?");
            return Vector3.zero;
        }
        float rayLength = (mouseRay.origin.y / mouseRay.direction.y);
        return mouseRay.origin - (mouseRay.direction * rayLength);
    }

    public void ActivateBtnMove()
    {
        btnMoveActivation = true;
    }

    public void DeactivateBtnMove()
    {
        btnMoveActivation = false;
    }

    public void SwitchActivationBtnMove()
    {
        btnMoveActivation = !btnMoveActivation;
    }

    public void PathCount(int count)
    {
        pathCount = count;
    }

    public void ActivateAttackActionTheFirst()
    {
        attackTheFirstInitiated = true;
    }

    public void EndTurn()
    {
        CanvasManagerScript._instance.Deactivation();
        canvasActivated = false;
        movementActivated = false;

        selectedUnit.MovementRemaining = selectedUnit.Movement;

        #region original code for unselection
        //foreach (var unit in MapGeneratorScript.instance.units)
        //{
        //    selectedUnit = unit;
        //}

        //GameObject tempHex_go = GameObject.Find("Hex_" + selectedUnit.Hex.C + "_" + selectedUnit.Hex.R);

        ////hexes = tempHex_GoCharacter.GetComponentInParent<UnitScript>().Hex.GetNeighbours(currentHitParentObject, 2, MapGeneratorScript.instance.numColumns, MapGeneratorScript.instance.numRows);
        ////hexes = currentHitParentObject.GetComponent<HexComponentScript>().hex.GetNeighbours(currentHitParentObject, 2, MapGeneratorScript.instance.numColumns, MapGeneratorScript.instance.numRows);
        //hexes = selectedUnit.Hex.GetNeighbours(tempHex_go, 5, MapGeneratorScript.instance.numColumns, MapGeneratorScript.instance.numRows);
        //foreach (HexScript hex in hexes)
        //{
        //    GameObject tempHexNeighbour_go = GameObject.Find("Hex_" + hex.C + "_" + hex.R);
        //    if (hex.movementCost == 1)
        //        tempHexNeighbour_go.transform.gameObject.GetComponentInChildren<MeshRenderer>().material.color = Color.white;
        //}
        //tempHex_go.GetComponentInChildren<MeshRenderer>().material.color = Color.white;
        #endregion
        if (selectedHexes != null)
        {
            foreach (var hex in selectedHexes)
            {
                hex.transform.gameObject.GetComponentInChildren<MeshRenderer>().material.color = Color.white;
            }
        }
    }
    #endregion
}



