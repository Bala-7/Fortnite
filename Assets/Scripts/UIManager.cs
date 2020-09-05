using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager s;
    private Fortnite_ThirdPersonInput _p;

    private Resolution _res;

    private Text woodTxt;
    private Text bricksTxt;
    private Text metalTxt;

    private Image pickaxeImg;
    private List<Image> weaponsImgs;
    private Image frameImg;

    private Text healthTxt;
    private Image healthBar;
    private Text shieldTxt;
    private Image shieldBar;

    #region Radar
    public Texture2D radarAtlasTexture;
    private Sprite[] radarPieces;
    private List<Image> radarInstances;
    private GameObject radarParent;
    private float _prevPlayerFwd = 0;
    #endregion


    private void Awake()
    {
        s = this;

        _p = FindObjectOfType<Fortnite_ThirdPersonInput>();
        _res = Screen.currentResolution;

        // Init craft elements amounts texts
        Transform bi = transform.Find("BuildMaterials");
        woodTxt = bi.Find("WoodIcon").Find("Txt").GetComponent<Text>();
        bricksTxt = bi.Find("BrickIcon").Find("Txt").GetComponent<Text>();
        metalTxt = bi.Find("MetalIcon").Find("Txt").GetComponent<Text>();

        // Init weapons icons
        Transform wi = transform.Find("WeaponsUI").Find("Icons");
        pickaxeImg = wi.Find("PickaxeIcon").GetComponent<Image>();

        weaponsImgs = new List<Image>();
        for (int i = 0; i < 5; ++i) {
            string w = "Weapon" + (i + 1);
            weaponsImgs.Add(wi.Find(w).GetComponent<Image>());
        }
        frameImg = wi.Find("Frame").GetComponent<Image>();

        Transform healthParent = transform.Find("PlayerHealth");
        healthTxt = healthParent.Find("HealthText").GetComponent<Text>();
        shieldTxt = healthParent.Find("ShieldText").GetComponent<Text>();

        healthBar = healthParent.Find("HealthBar").GetComponent<Image>();
        shieldBar = healthParent.Find("ShieldBar").GetComponent<Image>();

        // Creates the radar
        radarParent = transform.Find("Radar").gameObject;
        CreaterRadarPieces();


    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        /*
        woodTxt.text = GameManager.s.GetPlayerWood().ToString();
        bricksTxt.text = GameManager.s.GetPlayerBricks().ToString();
        metalTxt.text = GameManager.s.GetPlayerMetal().ToString();
        */

        UpdateRadar();
    }


    public void UpdateHealthAndShield(int newH, int newS)
    {
        healthTxt.text = newH.ToString();
        shieldTxt.text = newS.ToString();

        float hPerc = newH / 100.0f;
        float sPerc = newS / 100.0f;

        float hScale = healthBar.rectTransform.localScale.x * hPerc;
        healthBar.rectTransform.localScale = new Vector3(hScale, 1, 1);

        float sScale = shieldBar.rectTransform.localScale.x * sPerc;
        shieldBar.rectTransform.localScale = new Vector3(sScale, 1, 1);


    }

    public void SelectWeapon(int n) {
        if (n >= 0 && n < 5) {
            frameImg.rectTransform.position = weaponsImgs[n].rectTransform.position;
        }
    }

    public void SelectPickaxe() { frameImg.rectTransform.position = pickaxeImg.rectTransform.position; }

    public void UpdateWoodText() {
        woodTxt.text = _p.GetWood().ToString();
    }
    public void UpdateBricksText()
    {
        bricksTxt.text = _p.GetBricks().ToString();
    }
    public void UpdateMetalText()
    {
        metalTxt.text = _p.GetMetal().ToString();
    }

    #region Radar
    private void CreaterRadarPieces() {
        radarInstances = new List<Image>();

        foreach (Transform child in radarParent.transform) {
            radarInstances.Add(child.GetComponent<Image>());
        }
    

    }


    private void UpdateRadar() {
        float playerFwd = _p.GetForwardAngleRadar();
        float dif = playerFwd - _prevPlayerFwd; // Difference of player's forward between this frame and the previous one

        float leftMargin = -600 + 960;
        float rightMargin = 600 + 960;

        float alphaLeftMargin = leftMargin + 400;
        float alphaRightMargin = rightMargin - 400;

        for (int i = 0; i < radarInstances.Count; ++i)
        {
            // Moves the instance depending on the difference
            radarInstances[i].rectTransform.position -= new Vector3(dif * 50.0f / 15.0f, 0, 0);
            
            // If an element reach the side of the radar box, it is moved to the other side, to make the compass cyclic
            // The '960' in the formula is there because Canvas default X position is 960, and the 'rectTransform.position' is calculated from parent transforms.
            if (radarInstances[i].rectTransform.position.x >= rightMargin)
                radarInstances[i].rectTransform.position -= new Vector3(1200, 0, 0);
            else if (radarInstances[i].rectTransform.position.x <= leftMargin) 
                radarInstances[i].rectTransform.position += new Vector3(1200, 0, 0);

            // Modifies alpha value of the image so that imgs near the extremes of the box become semi-transparent
            Color tempColor = radarInstances[i].color;
            float boxPerc = (radarInstances[i].rectTransform.position.x - alphaLeftMargin) / (alphaRightMargin - alphaLeftMargin);
            tempColor.a = 1 - Mathf.Abs(0.5f - boxPerc);
            radarInstances[i].color = tempColor;

        }

        _prevPlayerFwd = playerFwd;
    }
    #endregion
}

