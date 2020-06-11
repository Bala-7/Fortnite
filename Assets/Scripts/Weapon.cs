using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{

    public Transform shootSrc;
    public GameObject bulletPrefab;

    [Range(1,120)]    
    public int baseDamage;
    public AnimationCurve DmgDistanceMultiplier;
    public float maxDistance;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public int GetBaseDamage() { return baseDamage; }

    public AnimationCurve GetDmgDistanceCurve() { return DmgDistanceMultiplier; }

    public float GetMaxDistance() { return maxDistance; }

    public Transform GetShootingSource() { return shootSrc; }

    public GameObject GetBullet() { return bulletPrefab; }
}

