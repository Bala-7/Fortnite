using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireWeapon : Weapon
{
    [Tooltip("Time (in seconds) that player has to wait between reloading and shooting.")]
    [Range(0.2f, 3.0f)]
    public float reloadTime;

    [Tooltip("Bullet amount in one cartridge.")]
    public int cartridgeSize;
    private int currentBullets;

    public Transform shootSrc;
    public GameObject bulletPrefab;

    public AudioClip reloadSnd;


    protected new void Awake()
    {
        base.Awake();
        currentBullets = cartridgeSize;
        GetComponent<MeshRenderer>().enabled = false;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void Fire()
    {
        if (canShoot)
        {
            Vector3 shootPos = transform.position + _player.GetComponent<CameraMovement>().GetForwardDirection() * 3.0f + new Vector3(0, -0.1f, 0);
            //Vector3 shootPos = _player.GetComponent<CameraMovement>().GetCamera().transform.position + _player.GetComponent<CameraMovement>().GetForwardDirection() * 3.0f + new Vector3(0, -0.1f, 0);
            //Vector3 shootPos = transform.parent.parent.position + _player.GetComponent<CameraMovement>().GetForwardDirection() * 1.0f + new Vector3(0, -0.1f, 0);
            // Create the bullet, sets the damage it will cause, and add some velocity to it
            GameObject go = Instantiate(bulletPrefab, shootPos, Quaternion.Euler(90, 0, 0));
            Rigidbody rb = go.GetComponent<Rigidbody>();
            go.GetComponent<Bullet>().SetDamage(damage);

            rb.velocity = _player.GetComponent<CameraMovement>().GetForwardDirection() * 40.0f;

            Debug.Log("Firing " + transform.name);

            // Plays weapon's shooting animation
            //_ac.Play("Shoot");

            currentBullets--;

            /*_as.Stop();
            _as.clip = shootSnd;
            _as.Play();
            */

            if (currentBullets > 0)
            {
                // Enables shooting again depending on cadence value
                canShoot = false;
                Invoke("EnableShoot", 1.0f / cadence);
            }
            else
            {
                Reload();
            }
        }
    }


    public override void Reload()
    {

       if (currentBullets < cartridgeSize)
        {
            canShoot = false;
            Invoke("EnableShoot", reloadTime);

            currentBullets = cartridgeSize;

            /*_as.Stop();
            _as.clip = reloadSnd;
            _as.Play();*/
        }
    }

    public int GetCartridgeSize() { return cartridgeSize; }
    public int GetCurrentBullets() { return currentBullets; }
}
