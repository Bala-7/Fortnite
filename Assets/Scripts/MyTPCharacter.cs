using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyTPCharacter : MonoBehaviour
{
    public Transform legs;
    public Transform body;
    private Animator legsAnimator;
    private Animator bodyAnimator;

    private void Awake()
    {
        legsAnimator = legs.gameObject.GetComponent<Animator>();
        bodyAnimator = body.gameObject.GetComponent<Animator>();
    }


    public Animator GetLegsAnimator() { return legsAnimator; }

    public Animator GetBodyAnimator() { return bodyAnimator; }




}
