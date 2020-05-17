using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager s;

    private int _pWood = 0;
    private int _pBricks = 0;
    private int _pMetal = 0;

    [SerializeField]
    private AnimationCurve _bounceAnimCurve;

    // Start is called before the first frame update
    void Awake()
    {
        s = this;
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    #region Crafting materials methods
    public int GetPlayerWood() { return _pWood; }
    public int GetPlayerBricks() { return _pBricks; }
    public int GetPlayerMetal() { return _pMetal; }

    public AnimationCurve GetBounceAnimationCurve() { return _bounceAnimCurve; }

    public void AddWood(int amount) { _pWood += amount; }
    public void AddBricks(int amount) { _pBricks += amount; }
    public void AddMetal(int amount) { _pMetal += amount; }
    #endregion
}
