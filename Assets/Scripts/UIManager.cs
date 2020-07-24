using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager s;
    private Fortnite_ThirdPersonInput _p;

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


    private void Awake()
    {
        s = this;

        _p = FindObjectOfType<Fortnite_ThirdPersonInput>();

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
}

