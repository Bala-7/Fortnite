using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CraftingObject : MonoBehaviour
{
    public enum OBJECT_TYPE { WOOD = 0, BRICK, METAL }; // Type of object

    public enum OBJECT_STATE { NORMAL = 0, ANIMATING }; // States for bounce animation

    public int health;
    public int amountToPlayer;
    public OBJECT_TYPE type;
    private OBJECT_STATE _state;


    #region Attributes for bounce animation    
    private AnimationCurve _bounceAnim;
    private Vector3 startScale;
    private float currentTime;
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        _state = OBJECT_STATE.NORMAL;
        _bounceAnim = GameManager.s.GetBounceAnimationCurve();
        startScale = transform.localScale;
    }



    // Update is called once per frame
    void Update()
    {
        if(_state == OBJECT_STATE.ANIMATING) { 
            currentTime += Time.deltaTime;
            //_bounceAnim.Evaluate(currentTime);
            float scale = 1 - _bounceAnim.Evaluate(currentTime);

            Vector3 objScale = startScale * scale;

            transform.localScale = objScale;
        }
    }


    public void Hit(Vector3 from)
    {
        // Bounce stuff
        _state = OBJECT_STATE.ANIMATING;
        currentTime = 0;

        // Stats stuff
        health--;
        if (health <= 0)
        {
            UpdatePlayerAmount();
            Destroy(this.gameObject);
        }

    }

    protected void UpdatePlayerAmount() {
        switch (type) {
            case OBJECT_TYPE.WOOD: {
                    GameManager.s.AddWood(amountToPlayer);
                    UIManager.s.UpdateWoodText();
                    break; 
                }
            case OBJECT_TYPE.BRICK: {
                    GameManager.s.AddBricks(amountToPlayer);
                    UIManager.s.UpdateBricksText();
                    break; 
                }
            case OBJECT_TYPE.METAL: {
                    GameManager.s.AddMetal(amountToPlayer);
                    UIManager.s.UpdateMetalText();
                    break; 
                }

        }
    }
}
