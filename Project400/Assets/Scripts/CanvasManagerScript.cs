using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasManagerScript : MonoBehaviour {

    public GameObject actionPanel;
    public GameObject enengyTextPanel;
    public Text energyText;

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
        enengyTextPanel.SetActive(true);
    }

    public void Deactivation()
    {
        actionPanel.SetActive(false);
        enengyTextPanel.SetActive(false);
    }

    public void DisplayEnergyUsage(int PotentialEnergyUsage, int TotalUsableEnergy)
    {
        energyText.text = PotentialEnergyUsage + " out of " + TotalUsableEnergy;
    }
}
