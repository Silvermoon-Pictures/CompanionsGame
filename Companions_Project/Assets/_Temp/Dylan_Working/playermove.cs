using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playermove : MonoBehaviour
{

    public CharacterController controller;
    public float speed = 12f;
    public float gravity = -9.8f;
    public Transform groundCheck;
    public float grroundDistance = 0.4f;
    public LayerMask groundMask;
    public float jumpHeight = 5f;
    Vector3 velocity;
    bool isGrounded;
    public Animator animator;

   

    // Update is called once per frame
    void Update()
    {

        isGrounded = Physics.CheckSphere(groundCheck.position, grroundDistance, groundMask);

        if (isGrounded && velocity.y <0)
        {
            velocity.y = -2f;



        }


        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis ("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;

        controller.Move(move * speed * Time.deltaTime);

        animator.SetFloat("Speed", Mathf.Abs(x));
        animator.SetFloat("Speed2", z);

        if ( Input.GetButtonDown("Jump")&& isGrounded ) {
        
        
            velocity.y = Mathf.Sqrt(jumpHeight * -2 * gravity );


        }



        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}
