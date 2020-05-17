using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildableObject : MonoBehaviour
{
    List<MeshCollider> mcs;
    // Start is called before the first frame update
    void Awake()
    {
        mcs = new List<MeshCollider>(gameObject.GetComponentsInChildren<MeshCollider>());
        foreach (MeshCollider mc in mcs)
        {
            mc.enabled = false;
        }
        GetComponent<BoxCollider>().enabled = false;
        Invoke("ActivateCollider", 1.0f);
    }


    void ActivateCollider() {
        GetComponent<BoxCollider>().enabled = true;
    }
}
