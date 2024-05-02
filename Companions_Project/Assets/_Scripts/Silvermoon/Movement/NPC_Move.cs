using System;
using UnityEngine;

public class ObjectMovementDetector : MonoBehaviour
{
    private Vector3 previousPosition;

    private Animator animator;

    void Start()
    {
        previousPosition = transform.position;
        animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        float distance = Vector3.Distance(transform.position, previousPosition);

        if (distance > 0.001f)
        {
            animator.SetBool("isMoving", true);
        }
        else
        {
            animator.SetBool("isMoving", false);
        }

        
        previousPosition = transform.position;
    }
}