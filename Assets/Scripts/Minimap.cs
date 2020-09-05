using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

public class Minimap : MonoBehaviour
{
    private Transform _cursor;
    public Transform floor;
    public Transform minimap;

    private void Awake()
    {
        _cursor = transform.GetChild(0);
    }

    // Update is called once per frame
    void Update()
    {
        // Rotates the cursor
        _cursor.eulerAngles = new Vector3( 90, Fortnite_ThirdPersonInput.s.transform.eulerAngles.y, 0);

        // Moves the camera, and hence the cursor as well
        Vector3 pPos = Fortnite_ThirdPersonInput.s.transform.position;
        pPos = new Vector3(pPos.x, 0, pPos.z);
        

        transform.position = new Vector3(997.5f,0,1002.5f) + pPos * 0.01f + 2*Vector3.up;
        //Debug.Log("Player position is " + pPos);
        //Debug.Log("Minimap camera position is " + transform.position);

        
    }
}
