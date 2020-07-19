using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private float lifetime = 3.0f;
    private int damage;


    // Start is called before the first frame update
    void Start()
    {
        Invoke("DestroyBullet", lifetime);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetDamage(int dmg)
    {
        damage = dmg;
    }

    private void DestroyBullet() { Destroy(this.gameObject); }


    void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.gameObject.layer == 9 || collision.transform.tag == "Enemy")
        {
            Enemy e = collision.transform.gameObject.GetComponent<Enemy>();
            e.Damage(damage);
        }

        DestroyBullet();
    }
}
