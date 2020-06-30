using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;
using UnityStandardAssets.Characters.ThirdPerson;

public class Fortnite_ThirdPersonInput : ThirdPersonUserControl
{

    public enum CAMERA_MODE { BASE };   // Camera modes used to change cam settings
    public enum PLAYER_STATE { NORMAL, BUILDING };  // Player game states
    public enum BUILD_STATE { WALL_PREVIEW, FLOOR_PREVIEW, RAMP_PREVIEW };  // States within building mode

    #region Atributos
    // Public
    public Transform chest;

    #region Build prefabs
    public GameObject verticalWallPrefab;
    public GameObject verticalWallPreview;
    public GameObject floorPrefab;
    public GameObject floorPreview;
    public GameObject rampPrefab;
    public GameObject rampPreview;
    #endregion

    // Privates
    private MyTPCharacter tpc;          // Script asociado al modelo del robot. Uso esto solo para poder animarlo
    private CinemachineFreeLook cfl;    // Este es el objeto que se crea en la escena al crear una cámara Cinemachine. Lo usamos para controlar la posición y movimiento de la cámara

    #region PostProcessing
    private PostProcessVolume ppv;
    private ColorGrading pp_cg;
    private LensDistortion pp_ld;
    #endregion
    
    private PLAYER_STATE _state;
    private BUILD_STATE _bState;
    private GameObject _wallPrevInstance;
    private GameObject _floorPrevInstance;
    private GameObject _rampPrevInstance;

    private bool canPick = true;
    #endregion


    private void Awake()
    {
        tpc = FindObjectOfType<MyTPCharacter>();

        _state = PLAYER_STATE.NORMAL;
        _bState = BUILD_STATE.WALL_PREVIEW;

        _wallPrevInstance = Instantiate(verticalWallPreview);
        _wallPrevInstance.SetActive(false);

        _floorPrevInstance = Instantiate(floorPreview);
        _floorPrevInstance.SetActive(false);

        _rampPrevInstance = Instantiate(rampPreview);
        _rampPrevInstance.SetActive(false);
    }

    protected void Update()
    {
        // Input
        bool mouseLeft = Input.GetMouseButton(0); // Clic izquierdo
        bool walking = (m_Move.x != 0 || m_Move.z != 0);    // true if the character is moving
        bool walkingBackwards = Input.GetKey(KeyCode.S);
        bool modeKeyPressed = Input.GetKeyDown(KeyCode.Q);      // true in the frame we press the key
       

        if (modeKeyPressed) {   // Change the mode
            _state = (_state == PLAYER_STATE.NORMAL) ? PLAYER_STATE.BUILDING : PLAYER_STATE.NORMAL;
            _wallPrevInstance.SetActive(false);
            _floorPrevInstance.SetActive(false);
            _rampPrevInstance.SetActive(false);
        }

        // Here starts the state machine for the character
        if (_state == PLAYER_STATE.NORMAL){ // If the player is in the normal state
            if (mouseLeft)
            {
                // Cast a ray to the object we want to pick
                int layerMask = 1 << 8;
                float hitMaxDistance = 1.3f;
                RaycastHit hitInfo;
                bool hit = Physics.Raycast(chest.position, chest.forward, out hitInfo, hitMaxDistance, layerMask);
                Debug.DrawRay(chest.position, chest.forward * hitMaxDistance, (hit) ? Color.black : Color.red);

                // If the object is pickable
                if (canPick)
                {
                    // Does the ray intersect any objects excluding the player layer
                    if (hit)
                    {
                        hitInfo.transform.gameObject.GetComponent<CraftingObject>().Hit(transform.position);
                    }
                    
                    canPick = false;
                    Invoke("EnablePick", 1.0f);
                }
            }
        }
        else if (_state == PLAYER_STATE.BUILDING) { // If the player is in the building state
            #region Input Management in Building Mode
            if (Input.GetKeyDown(KeyCode.Z)) {  // Change to wall preview
                _bState = BUILD_STATE.WALL_PREVIEW;

                _wallPrevInstance.SetActive(true);
                _floorPrevInstance.SetActive(false);
                _rampPrevInstance.SetActive(false);
            }
            else if (Input.GetKeyDown(KeyCode.X)) { // Change to floor preview
                _bState = BUILD_STATE.FLOOR_PREVIEW;

                _wallPrevInstance.SetActive(false);
                _floorPrevInstance.SetActive(true);
                _rampPrevInstance.SetActive(false);
            }
            else if (Input.GetKeyDown(KeyCode.C)) { // Change to ramp preview
                _bState = BUILD_STATE.RAMP_PREVIEW;

                _wallPrevInstance.SetActive(false);
                _floorPrevInstance.SetActive(false);
                _rampPrevInstance.SetActive(true);
            }
            #endregion

            #region States Management in Building Mode
            // Here starts the building sub-state machine
            if (_bState == BUILD_STATE.WALL_PREVIEW) {
                Vector2 playerCell = GetPlayerPositionCell();
                Vector2 lookingDir = GetPlayerLookingDirection();
                Vector2 targetCell = playerCell + lookingDir;
                Vector3 buildPosRot = GetBuildPositionAndRotation(playerCell, lookingDir);
                Vector3 strPos = new Vector3(buildPosRot.x, 0, buildPosRot.y);
                Quaternion strRot = Quaternion.Euler(new Vector3(0, buildPosRot.z, 0));

                _wallPrevInstance.transform.position = strPos;
                _wallPrevInstance.transform.rotation = strRot;
                if (Input.GetMouseButtonDown(0)) {  // If left clic

                    //_wallPrevInstance.SetActive(false);
                    GameObject go = Instantiate(verticalWallPrefab, strPos, strRot);
                }
            }
            else if (_bState == BUILD_STATE.FLOOR_PREVIEW) {
                Vector2 playerCell = GetPlayerPositionCell();
                Vector3 strPos = new Vector3(playerCell.x*4, 0, playerCell.y*4);

                _floorPrevInstance.transform.position = strPos;
                _floorPrevInstance.transform.rotation = Quaternion.identity;
                
                
                if (Input.GetMouseButtonDown(0))
                {  // If left clic

                    //_wallPrevInstance.SetActive(false);
                    GameObject go = Instantiate(floorPrefab, strPos, Quaternion.identity);
                }
            }
            else if (_bState == BUILD_STATE.RAMP_PREVIEW)
            {
                Vector2 playerCell = GetPlayerPositionCell();
                Vector2 lookingDir = GetPlayerLookingDirection();

                Vector3 strPos = new Vector3(playerCell.x * 4 + 2, 2, playerCell.y * 4 + 2);
                Quaternion strRot = Quaternion.Euler(new Vector3((lookingDir.x<0) ? 180 : 0, lookingDir.y*(-90), 45));

                _rampPrevInstance.transform.position = strPos;
                _rampPrevInstance.transform.rotation = strRot;


                if (Input.GetMouseButtonDown(0))
                {  // If left clic
                    GameObject go = Instantiate(rampPrefab, strPos, strRot);

                    RaycastHit hitInfo;
                    int layerMask = 1 << 8;
                    Vector3 flatPos = new Vector3(transform.position.x, 4, transform.position.z);
                    bool hit = Physics.Raycast(flatPos, new Vector3(0, -1, 0), out hitInfo, 6, layerMask);
                    transform.position = new Vector3(transform.position.x, hitInfo.point.y, transform.position.z);
                    
                    
                }
            }

            #endregion
        }

        bool building = (_state == PLAYER_STATE.BUILDING);

        // Animations
        tpc.GetLegsAnimator().SetBool("walking", walking);
        
        tpc.GetBodyAnimator().SetBool("picking", mouseLeft);
        tpc.GetBodyAnimator().SetBool("building", building);
        if (!mouseLeft) tpc.GetBodyAnimator().SetBool("walking", walking);
    }




    private void SetCameraMode(CAMERA_MODE mode) {

        switch (mode) {
            case CAMERA_MODE.BASE:
                { 
                    // TODO: Set cam settings when the character is just walking
                    break;
                }
            
            default: break;
        }
    
    }


    void EnablePick() {
        canPick = true;
    }


    #region Building related methods

    // Returns the (X,Z) cell in which the player is located
    private Vector2 GetPlayerPositionCell() {

        int cellX = ((int)transform.position.x) / 4;
        int cellZ = ((int)transform.position.z) / 4;

        return new Vector2(cellX, cellZ);
    }

    // Returns the (X,Z) cell to which the player is looking at
    private Vector2 GetPlayerLookingDirection()
    {
        if (Mathf.Abs(transform.forward.x) > Mathf.Abs(transform.forward.z))
        {
            float x = Mathf.Sign(transform.forward.x);
            return new Vector2(x, 0);
        }
        else {
            float z = Mathf.Sign(transform.forward.z);
            return new Vector2(0, z);
        }
        
        return new Vector2(0,0);
    }

    // Returns the position and rotation in which the structure will be built. The return Vector3 is in the following format: (PosX, PosZ, Rotation)
    private Vector3 GetBuildPositionAndRotation(Vector2 playerCell, Vector2 lookDir) {
        float x = 0, z = 0, rot = 0;
        if (lookDir.x > 0)
        {
            x = (int)(playerCell.x + 1) * 4;
            z = (int)(playerCell.y + 1) * 4 - 0.6f;
            rot = 0;
        }
        else if (lookDir.x < 0) {
            x = (int)(playerCell.x) * 4;
            z = (int)(playerCell.y + 1) * 4 - 0.6f;
            rot = 0;
        }
        else if (lookDir.y > 0)
        {
            x = (int)(playerCell.x) * 4 + 0.6f;
            z = (int)(playerCell.y + 1) * 4;
            rot = -90;
        }
        else if (lookDir.y < 0)
        {
            x = (int)(playerCell.x) * 4 + 0.6f;
            z = (int)(playerCell.y) * 4;
            rot = -90;
        }


        return new Vector3(x,z,rot);
    }
    
    #endregion
}



