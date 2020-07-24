using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageUI : MonoBehaviour
{
    private SpriteRenderer leftN;
    private SpriteRenderer rightN;

    public List<Sprite> numbers;

    private Animator _ac;

    private void Awake()
    {
        _ac = GetComponent<Animator>();
        leftN = transform.GetChild(1).gameObject.GetComponent<SpriteRenderer>();
        rightN = transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        transform.LookAt(Fortnite_ThirdPersonInput.s.transform);
        transform.Rotate(Vector3.up, 180.0f);
    }

    public void ShowDamage(int damage) {
        if (damage > 0 && damage < 100) {
            int left = damage / 10;
            int right = damage % 10;

            leftN.sprite = numbers[left];
            rightN.sprite = numbers[right];

            _ac.Play("DamageUI_Show");        
        }
    }

}
