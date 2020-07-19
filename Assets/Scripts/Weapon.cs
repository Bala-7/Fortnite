using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    protected AudioSource _as;
    public AudioClip shootSnd;


    protected GameObject _player;

    [Tooltip("Times per second this weapon can be shot/activated.")]
    public float cadence;
    protected bool canShoot = true;

    [Tooltip("Damage amount per hit.")]
    [Range(1, 100)]
    public int damage;

    [Tooltip("Time (in seconds) that player has to wait between showing the weapon and shooting.")]
    [Range(0.2f, 1.0f)]
    public float equipTime;

    protected void Awake()
    {
        _as = GetComponent<AudioSource>();
        _player = GameObject.FindGameObjectsWithTag("Player")[0];
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public abstract void Fire();

    public virtual void ShowWeapon()
    {
        GetComponent<MeshRenderer>().enabled = true;
        _player.GetComponent<Fortnite_ThirdPersonInput>().GetTPC().GetBodyAnimator().Play("Body_" + name + "_Show");
        canShoot = false;
        Invoke("EnableShoot", equipTime);
    }

    public abstract void Reload();

    protected void EnableShoot() { canShoot = true; }
}
