using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;
using UnityStandardAssets.Characters.ThirdPerson;

public class Fortnite_ThirdPersonInput : ThirdPersonUserControl
{

    public static Fortnite_ThirdPersonInput s;

    public enum CAMERA_MODE { BASE };   // Camera modes used to change cam settings
    public enum PLAYER_STATE { PICK, BUILDING, WEAPON };  // Player game states
    public enum BUILD_STATE { WALL_PREVIEW, FLOOR_PREVIEW, RAMP_PREVIEW };  // States within building mode
    public enum AIM_STATE { AIM_HIP, AIM_SHOULDER}

    #region Atributos
    // Public
    public Transform chest;

    public GameObject pickaxe;

    #region Health
    private int _health = 100;
    private int _shield = 100;
    #endregion

    #region Movement
    [Range(1.0f, 10.0f)]
    public float walk_speed;
    [Range(1.0f, 10.0f)]
    public float backwards_walk_speed;
    [Range(1.0f, 10.0f)]
    public float strafe_speed;

    [Range(0.1f, 1.5f)]
    public float rotation_speed;

    [Range(2.0f, 10.0f)]
    public float jump_force;
    #endregion

    #region Crafting
    private int _wood = 0;
    private int _bricks = 0;
    private int _metal = 0;
    #endregion

    #region Build prefabs
    public GameObject map;
    public GameObject pencil;
    public GameObject verticalWallPrefab;
    public GameObject verticalWallPreview;
    public GameObject floorPrefab;
    public GameObject floorPreview;
    public GameObject rampPrefab;
    public GameObject rampPreview;
    #endregion

    #region Shooting
    public List<Weapon> weapons;
    private int MAX_WEAPONS_TO_USE = 5;
    private int currentWeapon = 0;
    #endregion

    #region Camera
    private Vector3 camFwd;
    private Camera _cam;
    private CameraMovement _camM;
    #endregion

    // Privates
    private MyTPCharacter tpc;          // Script asociado al modelo del robot. Uso esto solo para poder animarlo
    private CinemachineFreeLook cfl;    // Este es el objeto que se crea en la escena al crear una cámara Cinemachine. Lo usamos para controlar la posición y movimiento de la cámara
    private Rigidbody _rb;

    #region PostProcessing
    private PostProcessVolume ppv;
    private ColorGrading pp_cg;
    private LensDistortion pp_ld;
    #endregion
    
    private PLAYER_STATE _state;
    private BUILD_STATE _bState;
    private AIM_STATE _aState;

    private GameObject _wallPrevInstance;
    private GameObject _floorPrevInstance;
    private GameObject _rampPrevInstance;

    private bool canPick = true;
    #endregion


    private void Awake()
    {
        s = this;

        _rb = GetComponent<Rigidbody>();
        tpc = FindObjectOfType<MyTPCharacter>();
        _camM = GetComponent<CameraMovement>();
        _cam = _camM.GetCamera();


        _wallPrevInstance = Instantiate(verticalWallPreview);
        _wallPrevInstance.SetActive(false);

        _floorPrevInstance = Instantiate(floorPreview);
        _floorPrevInstance.SetActive(false);

        _rampPrevInstance = Instantiate(rampPreview);
        _rampPrevInstance.SetActive(false);

    }

    public MyTPCharacter GetTPC() { return tpc; }

    protected new void Start()
    {
        base.Start();
        InitializePlayer();
    }

    private void InitializePlayer() {
        
        // Initialize player states
        _state = PLAYER_STATE.PICK;
        _bState = BUILD_STATE.WALL_PREVIEW; // Default state for first time player goes into building mode
        _aState = AIM_STATE.AIM_HIP;

        DisableWeapons();

        // Disable any building prefab instance
        _wallPrevInstance.SetActive(false);
        _floorPrevInstance.SetActive(false);
        _rampPrevInstance.SetActive(false);
    }

    protected void Update()
    {
        GetComponent<Rigidbody>().velocity = Vector3.Scale(GetComponent<Rigidbody>().velocity, new Vector3(0, 1, 0));
        // Input
        bool mouseLeft = Input.GetMouseButton(0); // Left click
        bool mouseRightHeld = Input.GetMouseButton(1); // Right click
        bool mouseRightReleased = Input.GetMouseButtonUp(1);
        
        bool walking = (m_Move.x != 0 || m_Move.z != 0);    // true if the character is moving
        bool walkingBackwards = Input.GetKey(KeyCode.S);
        bool modeKeyPressed = Input.GetKeyDown(KeyCode.Q);      // true in the frame we press the key


        bool jump = Input.GetKeyDown(KeyCode.Space);
        if (jump) {
            _rb.AddForce(Vector3.up * jump_force, ForceMode.Impulse);
        }

        if (Input.GetKeyDown(KeyCode.P)) {
            DealDamage(32);
        }

        // Watch if we need to change de mode
        HandleModeChange();

        // Here starts the state machine for the character
        if (_state == PLAYER_STATE.PICK)
        { // If the player is in the normal state
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
                        hitInfo.transform.gameObject.GetComponent<CraftingObject>().Hit(transform.position, this);
                    }

                    canPick = false;
                    Invoke("EnablePick", 1.0f);
                }
            }
        }
        else if (_state == PLAYER_STATE.BUILDING)
        { // If the player is in the building state
            #region Input Management in Building Mode
            if (Input.GetKeyDown(KeyCode.Z))
            {  // Change to wall preview
                _bState = BUILD_STATE.WALL_PREVIEW;

                _wallPrevInstance.SetActive(true);
                _floorPrevInstance.SetActive(false);
                _rampPrevInstance.SetActive(false);
            }
            else if (Input.GetKeyDown(KeyCode.X))
            { // Change to floor preview
                _bState = BUILD_STATE.FLOOR_PREVIEW;

                _wallPrevInstance.SetActive(false);
                _floorPrevInstance.SetActive(true);
                _rampPrevInstance.SetActive(false);
            }
            else if (Input.GetKeyDown(KeyCode.C))
            { // Change to ramp preview
                _bState = BUILD_STATE.RAMP_PREVIEW;

                _wallPrevInstance.SetActive(false);
                _floorPrevInstance.SetActive(false);
                _rampPrevInstance.SetActive(true);
            }
            #endregion

            #region States Management in Building Mode
            // Here starts the building sub-state machine
            if (_bState == BUILD_STATE.WALL_PREVIEW)
            {
                Vector2 playerCell = GetPlayerPositionCell();
                Vector2 lookingDir = GetPlayerLookingDirection();
                Vector2 targetCell = playerCell + lookingDir;
                Vector3 buildPosRot = GetBuildPositionAndRotation(playerCell, lookingDir);

                int yCell = (int)(transform.position.y / 4);
                if ((int)(transform.position.y) % 4 > 2)
                {
                    yCell++;
                }
                Vector3 strPos = new Vector3(buildPosRot.x, yCell * 4, buildPosRot.y);
                Quaternion strRot = Quaternion.Euler(new Vector3(0, buildPosRot.z, 0));

                
                _wallPrevInstance.transform.position = strPos;
                _wallPrevInstance.transform.rotation = strRot;
                if (Input.GetMouseButtonDown(0))
                {  // If left clic

                    //_wallPrevInstance.SetActive(false);
                    GameObject go = Instantiate(verticalWallPrefab, strPos, strRot);
                }
            }
            else if (_bState == BUILD_STATE.FLOOR_PREVIEW)
            {
                Vector2 lookingCell = GetPlayerLookingCell();

                Vector2 playerCell = GetPlayerPositionCell();
                Vector2 lookingDir = GetPlayerLookingDirection();
                Vector2 targetCell = playerCell + lookingDir;
                int yCell = (int)(transform.position.y / 4);
                if ((int)(transform.position.y) % 4 > 2) {
                    yCell++;
                }
                Vector3 strPos = new Vector3(lookingCell.x * 4, yCell * 4, lookingCell.y * 4);

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
                Vector2 lookingCell = GetPlayerLookingCell();

                int yCell = (int)(transform.position.y / 4);
                if ((int)(transform.position.y) % 4 > 2)
                {
                    yCell++;
                }

                Vector3 strPos = new Vector3(lookingCell.x * 4 + 2, yCell* 4 + 2, lookingCell.y * 4 + 2);
                Quaternion strRot = Quaternion.Euler(new Vector3((lookingDir.x < 0) ? 180 : 0, lookingDir.y * (-90), 45));

                _rampPrevInstance.transform.position = strPos;
                _rampPrevInstance.transform.rotation = strRot;


                if (Input.GetMouseButtonDown(0))
                {  // If left clic
                    GameObject go = Instantiate(rampPrefab, strPos, strRot);

                    RaycastHit hitInfo;
                    int layerMask = 1 << 8;
                    Vector3 flatPos = new Vector3(transform.position.x, (yCell + 1) * 4, transform.position.z);
                    bool hit = Physics.Raycast(flatPos, new Vector3(0, -1, 0), out hitInfo, 6, layerMask);
                    transform.position = new Vector3(transform.position.x, hitInfo.point.y, transform.position.z);
                }
            }

            #endregion
        }
        else if (_state == PLAYER_STATE.WEAPON) {
            if (mouseRightHeld)
                _camM.SwitchToAimMode();
            else if (mouseRightReleased) {
                _camM.SwitchToWeaponMode();
            }

            if (mouseLeft) {
                weapons[currentWeapon].Fire();
            }
        }

        bool building = (_state == PLAYER_STATE.BUILDING);

        // Animations
        tpc.GetLegsAnimator().SetBool("walking", walking);
        
        tpc.GetBodyAnimator().SetBool("picking", mouseLeft);
        if (!mouseLeft) tpc.GetBodyAnimator().SetBool("walking", walking);
    }


    private void HandleModeChange() {
        bool buildModeKeyPressed = Input.GetKeyDown(KeyCode.Q);      // true in the frame we press the key
        bool pickModeKeyPressed = Input.GetKeyDown(KeyCode.F);
        bool mouseRight = Input.GetMouseButton(1); // Right click

        int weaponKeyPressed = WeaponKeyPressed();


        if (buildModeKeyPressed)
        {   // If we want to change the current mode by pressing Q
            _state = PLAYER_STATE.BUILDING;

            DisableWeapons();

            pickaxe.gameObject.SetActive(false);
            map.gameObject.SetActive(true);
            pencil.gameObject.SetActive(true);

            // Plays animation showing map and pencil
            tpc.GetBodyAnimator().Play("Body_Build");

        }
        else if (pickModeKeyPressed) {
            _state = PLAYER_STATE.PICK;

            DisableWeapons();



            pickaxe.gameObject.SetActive(true);
            map.gameObject.SetActive(false);
            pencil.gameObject.SetActive(false);

            _wallPrevInstance.SetActive(false);
            _floorPrevInstance.SetActive(false);
            _rampPrevInstance.SetActive(false);

            UIManager.s.SelectPickaxe();

            tpc.GetBodyAnimator().Play("Body_Pick_Idle");
        }
        else if (weaponKeyPressed != -1)
        {
            // Show weapon
            _state = PLAYER_STATE.WEAPON;
            _aState = AIM_STATE.AIM_HIP;


            // Hide other elements (pickaxe, map, etc)
            map.SetActive(false);
            pencil.SetActive(false);
            pickaxe.SetActive(false);
            _wallPrevInstance.SetActive(false);
            _floorPrevInstance.SetActive(false);
            _rampPrevInstance.SetActive(false);


            weapons[currentWeapon].gameObject.SetActive(false);
            currentWeapon = weaponKeyPressed;
            weapons[currentWeapon].gameObject.SetActive(true);
            weapons[currentWeapon].ShowWeapon();

            UIManager.s.SelectWeapon(currentWeapon);

        }
    }

    private int WeaponKeyPressed() {
        int whickKey = -1;

        int start = (int) KeyCode.Alpha1;
        int end = start + Mathf.Min(weapons.Count, MAX_WEAPONS_TO_USE);
        for (int i = start; i < end; ++i)
            if (Input.GetKeyDown((KeyCode)i))
                return i - start;

        return whickKey;
    }

    private void DisableWeapons()
    {
        foreach (Weapon w in weapons)
        {
           w.gameObject.SetActive(false);  // Disable every weapon 
        }
    }


    protected new void FixedUpdate() {
        // Gets the input
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        bool jump = Input.GetButtonDown("Jump");

        // Calculate camera relative directions to move:
        camFwd = Vector3.Scale(_cam.transform.forward, new Vector3(1, 1, 1)).normalized;
        Vector3 camFlatFwd = Vector3.Scale(_cam.transform.forward, new Vector3(1, 0, 1)).normalized;
        Vector3 flatRight = new Vector3(_cam.transform.right.x, 0, _cam.transform.right.z);

        Vector3 m_CharForward = Vector3.Scale(camFlatFwd, new Vector3(1, 0, 1)).normalized;
        Vector3 m_CharRight = Vector3.Scale(flatRight, new Vector3(1, 0, 1)).normalized;


        // Draws a ray to show the direction the player is aiming at
        //Debug.DrawLine(transform.position, transform.position + camFwd * 5f, Color.red);

        // Move the player (movement will be slightly different depending on the camera type)
        float w_speed;
        Vector3 move = Vector3.zero;
        w_speed = (v > 0) ? walk_speed : backwards_walk_speed;
        move = v * m_CharForward * w_speed + h * m_CharRight * strafe_speed;

        transform.position += move * Time.deltaTime;    // Move the actual player
        m_Move = move;
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
    }

    private Vector2 GetPlayerLookingCell() {
        Vector2 targetCell = Vector2.zero;
        Vector2 playerCell = GetPlayerPositionCell();

        // Bit shift the index of the layer (8) to get a bit mask
        int layerMask = 1 << 8;

        // This would cast rays only against colliders in layer 8.
        // But instead we want to collide against everything except layer 8. The ~ operator does this, it inverts a bitmask.
        layerMask = ~layerMask;

        RaycastHit hit;
        // Does the ray intersect any objects excluding the player layer
        if (Physics.Raycast(_cam.transform.position, _camM.GetForwardDirection(), out hit, 4.0f))
        {
            Debug.DrawRay(_cam.transform.position, _camM.GetForwardDirection() * hit.distance, Color.yellow);
            
            return playerCell;
        }
        else
        {
            Debug.DrawRay(_cam.transform.position, _camM.GetForwardDirection() * 4.0f, Color.white);
            
            Vector2 lookingDir = GetPlayerLookingDirection();

            return playerCell + lookingDir;
        }


        return targetCell;
    
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

    #region Craft materials methods

    public int GetWood() { return _wood; }

    public int GetBricks() { return _bricks; }

    public int GetMetal() { return _metal; }

    public void AddWood(int amount) { _wood += amount; }
    public void AddBricks(int amount) { _bricks += amount; }
    public void AddMetal(int amount) { _metal += amount; }

    #endregion

    #region Health related methods
    public int GetHealth() { return _health; }

    public void DealDamage(int dmg) {
        if (_shield > 0)
        {
            if (_shield > dmg)
            {
                _shield -= dmg;
            }
            else
            {
                _shield = 0;
                _health -= (dmg - _shield);
            }
        }
        else {
            _health -= dmg;
        }

        // Fix possible wrong values
        if (_shield < 0) _shield = 0;
        if (_health < 0) _health = 0;

        UIManager.s.UpdateHealthAndShield(_health, _shield);
    }

    #endregion


    public float GetForwardAngleRadar() { 

        return transform.eulerAngles.y;
    }
}



