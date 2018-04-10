using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitViewScript : MonoBehaviour
{

    Vector3 oldPosition;
    Vector3 newPosition;
    public string Name;
    public int Number;

    Vector3 currentVelocity;
    float smoothTime = .5f;

    bool move = false;

    private void Start()
    {
        newPosition = this.transform.position + Vector3.right;
    }

    public void ReceiveName(string name)
    {
        Name = name;
        Debug.LogError(Name);
    }

    public void ReceiveNumber(int number)
    {
        Number = number;
        Debug.LogError(Number);
    }

    public void OnUnitMoved(HexScript oldHex, HexScript newHex)
    {
        // This GameObject is supposed to be a child of the hex we are
        // standing in. This ensures that we are in the correct place
        // in the hierachy
        // Our correct position when we aren't moving, is to be at
        // 0,0 local position relative to our parent.

        // TODO: Get rid of VerticalOffset and instead use a raycast to determine correct height
        // on each frame.
        move = true;
        oldPosition = oldHex.PositionFromCamera();
        newPosition = newHex.PositionFromCamera();

    }

    private void Update()
    {
        if (move == true)
            this.transform.position = Vector3.SmoothDamp(this.transform.position, new Vector3(newPosition.x,1,newPosition.z), ref currentVelocity, smoothTime);
    }
}
