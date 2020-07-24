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
    public AnimationCurve GetBounceAnimationCurve() { return _bounceAnimCurve; }
    #endregion
}
