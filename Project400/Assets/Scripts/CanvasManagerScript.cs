using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasManagerScript : MonoBehaviour {

    public GameObject actionPanel;

    #region Singleton
    public static CanvasManagerScript _instance;
    private void Awake()
    {
        if (_instance == null)
            _instance = this;
    }
    #endregion

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Activation()
    {
        actionPanel.SetActive(true);
    }

    public void Deactivation()
    {
        actionPanel.SetActive(false);
    }
}
