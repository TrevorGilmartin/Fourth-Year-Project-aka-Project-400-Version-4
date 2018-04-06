using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

//[CustomEditor (typeof (MapGeneratorScript))]
public class EditorScript : Editor {

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        MapGeneratorScript map = target as MapGeneratorScript;

        map.GenerateMap();
    }
}
