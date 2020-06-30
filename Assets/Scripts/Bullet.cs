using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private Vector3 _startPos;
    private Player _player;
    private Weapon _weapon;

    // Start is called before the first frame update
    void Start()
    {
        _startPos = transform.position;
        Invoke("DestroyBullet", 2.0f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetPlayer(Player p) { _player = p; }
    public void SetWeapon(Weapon w) { _weapon = w; }


    void DestroyBullet() {
        Destroy(this.gameObject);
    }

    void OnTriggerEnter(Collider other)
        {
        if (other.gameObject.CompareTag("Enemy")) {

            int damage = 0;
            float d = Vector3.Distance(_startPos, other.transform.position);
            if (d > _weapon.GetMaxDistance())
            {
                damage = (int)(_weapon.GetBaseDamage() * _weapon.GetDmgDistanceCurve().Evaluate(1.0f));
            }
            else {
                damage = (int)(_weapon.GetBaseDamage() * _weapon.GetDmgDistanceCurve().Evaluate(d / _weapon.GetMaxDistance()));
            }

            other.gameObject.GetComponent<Enemy>().DealDamage(damage); ;
            Destroy(gameObject);
        }
        
    }
}
