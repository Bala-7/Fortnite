using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageUI : MonoBehaviour
{
    public List<Sprite> numbers;
    
    private SpriteRenderer leftN;
    private SpriteRenderer rightN;

    private Animator _ac;   // Animator controller
    private Transform _player;

    private void Awake()
    {
        _ac = GetComponent<Animator>();
        leftN = transform.GetChild(0).GetComponent<SpriteRenderer>();
        rightN = transform.GetChild(1).GetComponent<SpriteRenderer>();
        _player = GameObject.FindGameObjectWithTag("Player").transform;
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(_player.position);
        transform.Rotate(0,180,0);
    }


    public void ShowDamage(int dmg) {
        if (dmg > 0 && dmg < 100) {
            int right = dmg / 10;
            int left = dmg % 10;

            leftN.sprite = numbers[left];
            rightN.sprite = numbers[right];

            transform.LookAt(_player.position);

            _ac.Play("DmgUI_ShowDmg");
        }
    }

}
