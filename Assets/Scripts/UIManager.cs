using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager s;

    public Text woodTxt;
    public Text bricksTxt;
    public Text metalTxt;

    // Start is called before the first frame update
    void Start()
    {
        s = this;
    }

    // Update is called once per frame
    void Update()
    {
        /*
        woodTxt.text = GameManager.s.GetPlayerWood().ToString();
        bricksTxt.text = GameManager.s.GetPlayerBricks().ToString();
        metalTxt.text = GameManager.s.GetPlayerMetal().ToString();
        */
    }


    public void UpdateWoodText() {
        woodTxt.text = GameManager.s.GetPlayerWood().ToString();
    }

    public void UpdateBricksText()
    {
        bricksTxt.text = GameManager.s.GetPlayerBricks().ToString();
    }

    public void UpdateMetalText()
    {
        metalTxt.text = GameManager.s.GetPlayerMetal().ToString();
    }
}

