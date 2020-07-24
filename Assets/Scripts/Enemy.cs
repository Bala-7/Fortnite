using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    private int health = 100;
    private Animator _ac;

    public DamageUI dmgUIPrefab;

    private void Awake()
    {
        _ac = GetComponent<Animator>();

    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Damage(int dmg) {
        if (health > 0)
        {
            health -= dmg;

            DamageUI ui = Instantiate(dmgUIPrefab, transform.position + new Vector3(0,2,0), Quaternion.identity);
            ui.ShowDamage(dmg);
        }
        else
        {
            _ac.Play("Death");
        }
    }
}
