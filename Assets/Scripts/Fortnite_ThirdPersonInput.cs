using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;
using UnityStandardAssets.Characters.ThirdPerson;
using UnityStandardAssets.CrossPlatformInput;

public class Fortnite_ThirdPersonInput : ThirdPersonUserControl
{

    public enum CAMERA_MODE { BASE , AIM};   // Camera modes used to change cam settings
    public enum PLAYER_STATE { NORMAL, BUILDING , AIM };  // Player game states
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

    #region Aim State
    private Transform aimTarget;        // Objeto que situaremos en el centro de la pantalla al apuntar, y hacia el que mirará el personaje.
    private int aimDistance = 5;        // Distancia a la que situamos 'aimTarget' frente a nuestro personaje.
    public GameObject bulletPrefab;
    public Transform shootSrc;
    #endregion

    #region Build State
    private BUILD_STATE _bState;
    private GameObject _wallPrevInstance;
    private GameObject _floorPrevInstance;
    private GameObject _rampPrevInstance;
    private bool canPick = true;
    #endregion
    
    #endregion
    

    private void Awake()
    {
        cfl = FindObjectOfType<CinemachineFreeLook>();
        tpc = FindObjectOfType<MyTPCharacter>();

        _state = PLAYER_STATE.NORMAL;
        _bState = BUILD_STATE.WALL_PREVIEW;

        _wallPrevInstance = Instantiate(verticalWallPreview);
        _wallPrevInstance.SetActive(false);

        _floorPrevInstance = Instantiate(floorPreview);
        _floorPrevInstance.SetActive(false);

        _rampPrevInstance = Instantiate(rampPreview);
        _rampPrevInstance.SetActive(false);

        aimTarget = GameObject.FindGameObjectWithTag("AimTarget").transform;
        
    }


    private new void FixedUpdate()
    {
        if (_state != PLAYER_STATE.AIM)
        {
            base.FixedUpdate();
        }
        else {
            float h = CrossPlatformInputManager.GetAxis("Horizontal");
            float v = CrossPlatformInputManager.GetAxis("Vertical");


            // calculate camera relative direction to move:
            m_CamForward = Vector3.Scale(m_Cam.forward, new Vector3(1, 0, 1)).normalized;
            Vector3 flatFwd = new Vector3(m_Cam.forward.x, 0, m_Cam.forward.z);
            Vector3 flatRight = new Vector3(m_Cam.right.x, 0, m_Cam.right.z);

            Vector3 m_CharForward = Vector3.Scale(flatFwd, new Vector3(1, 0, 1)).normalized;
            Vector3 m_CharRight = Vector3.Scale(flatRight, new Vector3(1, 0, 1)).normalized;

            m_Move = v * m_CharForward + h * m_CharRight;


            //Debug.Log(v + " | " + h);
            Debug.DrawLine(chest.position, chest.position + flatFwd * 5f, Color.red);


            transform.position += m_Move * 2.5f * Time.deltaTime;

            // Se coloca el 'aimTarget' en el centro de la cámara. Nuestro personaje mirará hacia él para apuntar
            aimTarget.position = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height / 2, aimDistance));


            // Rotamos al personaje para que apunte al 'aimTarget'. En concreto lo que apunta a 'aimTarget' es el pecho (chest) del personaje.
            Vector3 aimVector = aimTarget.position - chest.position;
            Quaternion rotation = Quaternion.LookRotation(aimVector, Vector3.up);
            tpc.transform.rotation = rotation;

            
        }
    }

    protected void Update()
    {
        // Input
        bool mouseLeft = Input.GetMouseButton(0); // Clic izquierdo
        bool walking = (m_Move.x != 0 || m_Move.z != 0);    // true if the character is moving
        bool walkingBackwards = Input.GetKey(KeyCode.S);
        bool buildModeKey = Input.GetKeyDown(KeyCode.Q);      // true in the frame we press the key
        
        
        bool aimPressed = Input.GetMouseButtonDown(1);    // true en el frame en que se pulsa el botón de apuntar
        bool aimHold = Input.GetMouseButton(1);              // true mientras el botón de apuntar esté pulsado
        bool aimReleased = Input.GetMouseButtonUp(1);

        if (buildModeKey && _state != PLAYER_STATE.AIM) {   // Change the mode
            _state = (_state == PLAYER_STATE.NORMAL) ? PLAYER_STATE.BUILDING : PLAYER_STATE.NORMAL;
            _wallPrevInstance.SetActive(false);
            _floorPrevInstance.SetActive(false);
            _rampPrevInstance.SetActive(false);
        }
        if (aimPressed && _state == PLAYER_STATE.NORMAL)
        {
            _state = PLAYER_STATE.AIM;
            SetCameraMode(CAMERA_MODE.AIM);
        }
        else if(aimReleased && _state == PLAYER_STATE.AIM){
            _state = PLAYER_STATE.NORMAL;
            tpc.transform.localRotation = Quaternion.identity;
            SetCameraMode(CAMERA_MODE.BASE);
        }

        // Here starts the state machine for the character
        if (_state == PLAYER_STATE.NORMAL)
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
                        hitInfo.transform.gameObject.GetComponent<CraftingObject>().Hit(transform.position);
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
                Vector3 strPos = new Vector3(buildPosRot.x, 0, buildPosRot.y);
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
                Vector2 playerCell = GetPlayerPositionCell();
                Vector3 strPos = new Vector3(playerCell.x * 4, 0, playerCell.y * 4);

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
                Quaternion strRot = Quaternion.Euler(new Vector3((lookingDir.x < 0) ? 180 : 0, lookingDir.y * (-90), 45));

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
        else if (_state == PLAYER_STATE.AIM) {
            if (mouseLeft)
            {
                Shoot();
            }

            tpc.GetBodyAnimator().SetBool("shooting", mouseLeft);
        }


        bool building = (_state == PLAYER_STATE.BUILDING);

        
        // Animations
        tpc.GetLegsAnimator().SetBool("walking", walking);
        
        tpc.GetBodyAnimator().SetBool("aiming", aimHold);
        tpc.GetBodyAnimator().SetBool("picking", mouseLeft);
        tpc.GetBodyAnimator().SetBool("building", building);
        if (!mouseLeft) tpc.GetBodyAnimator().SetBool("walking", walking);

        
    }


    void Shoot() {
        Vector3 fwd = Vector3.Scale(m_Cam.forward, new Vector3(1, 0, 1)).normalized;

        GameObject bullet = Instantiate(bulletPrefab, shootSrc.position, Quaternion.identity);
        Rigidbody bulletRB = bullet.GetComponent<Rigidbody>();
        bulletRB.velocity = fwd * 25.0f;
        
    }

    private void SetCameraMode(CAMERA_MODE mode) {

        switch (mode) {
            case CAMERA_MODE.BASE:
                {
                    // Top Ring
                    cfl.m_Orbits[0].m_Height = 4.5f;
                    cfl.m_Orbits[0].m_Radius = 1.75f;

                    // Middle Ring
                    cfl.m_Orbits[1].m_Radius = 3.64f;

                    // Bottom Ring
                    cfl.m_Orbits[2].m_Height = 0.4f;

                    cfl.GetRig(0).GetCinemachineComponent<CinemachineComposer>().m_ScreenX = 0.4f;
                    cfl.GetRig(1).GetCinemachineComponent<CinemachineComposer>().m_ScreenX = 0.4f;
                    cfl.GetRig(2).GetCinemachineComponent<CinemachineComposer>().m_ScreenX = 0.4f;
                    break;
                }
            case CAMERA_MODE.AIM:
                {
                    
                    // Top Ring
                    cfl.m_Orbits[0].m_Height = 3.75f;
                    cfl.m_Orbits[0].m_Radius = 2.0f;

                    // Middle Ring
                    cfl.m_Orbits[1].m_Height = 3.0f;
                    cfl.m_Orbits[1].m_Radius = 2.75f;

                    // Bottom Ring
                    cfl.m_Orbits[2].m_Height = 1.5f;
                    cfl.m_Orbits[2].m_Radius = 2.5f;

                    cfl.GetRig(0).GetCinemachineComponent<CinemachineComposer>().m_ScreenX = 0.4f;
                    cfl.GetRig(1).GetCinemachineComponent<CinemachineComposer>().m_ScreenX = 0.4f;
                    cfl.GetRig(2).GetCinemachineComponent<CinemachineComposer>().m_ScreenX = 0.4f;

                    //m_Cam.gameObject.SetActive(false);
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



