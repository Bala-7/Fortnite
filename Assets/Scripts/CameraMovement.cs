using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public Camera _cam;

    public enum CAMERA_TYPE { FREE_LOOK, LOCKED }

    public CAMERA_TYPE type = CAMERA_TYPE.FREE_LOOK;

    [Range(0.1f, 2.0f)]
    public float sensitivity;
    public bool invertXAxis;
    public bool invertYAxis;

    public bool useLookAt;
    public Transform lookAt;

    #region Camera Transitions
    bool inTransition;
    private CameraState startState;
    private CameraState endState;
    private float transitionTime = 0.0f;

    public Transform aimCam;
    private struct CameraState {
        public Vector3 position;
        public Vector3 rotation;
        public Transform lookAt;
        public float time;
    }
    #endregion

    private void Awake()
    {
        if (type == CAMERA_TYPE.LOCKED) {
            _cam.transform.parent = transform;
        }
    }

    private void FixedUpdate()
    {
        if (!inTransition)
        {
            // Read input
            float h = Input.GetAxis("Mouse X");
            float v = Input.GetAxis("Mouse Y");

            // Settings
            h = (invertXAxis) ? (-h) : h;
            v = (invertYAxis) ? (-v) : v;

            // Orbit the camera around the character
            if (h != 0)
            {   // Horizontal movement 
                if (type == CAMERA_TYPE.LOCKED) transform.Rotate(Vector3.up, h * 90 * sensitivity * Time.deltaTime);
                else if (type == CAMERA_TYPE.FREE_LOOK) _cam.transform.RotateAround(transform.position, transform.up, h * 90 * sensitivity * Time.deltaTime);
            }
            if (v != 0)
            {   // Vertical movement
                _cam.transform.RotateAround(transform.position, transform.right, v * 90 * sensitivity * Time.deltaTime);
            }

            if(useLookAt) _cam.transform.LookAt(lookAt);
            // Fix Z-rotation issues
            Vector3 ea = _cam.transform.rotation.eulerAngles;
            _cam.transform.rotation = Quaternion.Euler(new Vector3(ea.x, ea.y, 0));
        }
        else {
            float t = (Time.time - startState.time) / (endState.time - startState.time);
            _cam.transform.position = Vector3.Lerp(startState.position, endState.position, t);
            _cam.transform.eulerAngles = Vector3.Lerp(startState.rotation, endState.rotation, t);


            if (useLookAt) _cam.transform.LookAt(lookAt); 
            if (t >= 1)
                inTransition = false;
        }
    }

    private void Update()
    {
        /*if (Input.GetKeyDown(KeyCode.Q)) {
            
        }*/
    }

    public Camera GetCamera() { return _cam; }


    public void TransitionTo(Vector3 finalPosition, Vector3 finalRotation, Transform finalLookAt, float duration) {
        startState.position = _cam.transform.position;
        startState.rotation = _cam.transform.rotation.eulerAngles;
        if(useLookAt) startState.lookAt = lookAt;
        startState.time = Time.time;

        endState.position = finalPosition;
        endState.rotation = finalRotation;
        if (useLookAt) endState.lookAt = finalLookAt;
        endState.time = startState.time + duration;
        
        transitionTime = duration;
        inTransition = true;

    }

    public Vector3 GetForwardDirection() { return _cam.transform.forward; }

    public void SwitchToAimMode() {
        //TransitionTo(aimCam.position, aimCam.rotation.eulerAngles, lookAt, 0.1f);
        _cam.fieldOfView = 30;
    }

    public void SwitchToWeaponMode() {
        _cam.fieldOfView = 60;
    }

}
