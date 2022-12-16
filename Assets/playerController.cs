using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class playerController : MonoBehaviour
{
    //Private Variables
    private Rigidbody2D rb;
    private Collider2D pc;
    private static readonly int IsMoving = Animator.StringToHash("isMoving");
    private float currentGravity;
    private float startTime = 0.2f;
    private float time = 0;
    private float bufferTime = 0;
    private bool jumpBuffered = false;
    private float coyoteStartTime = 0.05f;
    private float coyoteTime = 0;
    [SerializeField] private LayerMask _layerMask;

    [Header("Movement")] 
    public float movementSpeed = 10;
    public float maximumSpeed = 10;
    public float frictionConstant = 0.5f;
    public float worldGravity = -10f;
    public float jumpPower = 10f;
    public bool inAir = false;
    void Start()
    {
        //Getting the Rigidbody
        rb = gameObject.GetComponent<Rigidbody2D>();
        pc = gameObject.GetComponent<Collider2D>();
        currentGravity = worldGravity;
        //filter.minNormalAngle = -1;
        //filter.maxNormalAngle = 1;
        //filter.layerMask = 8;
    }

    void FixedUpdate()
    {
        //Handles the Horizontal Movement and Jumping
        HorizontalMovement();

        //Gravity
        rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y + currentGravity * Time.deltaTime);
    }

    void HorizontalMovement()
    {
        //Left-Right Input
        if (Input.GetKey("d"))
        {
            //If we are in the air, halve the movement speed of the player
            if (inAir || !(coyoteTime > 0) || !(jumpBuffered))
            {
                rb.AddForce(new Vector2(movementSpeed / 2, 0));
            }
            else
            {
                rb.AddForce(new Vector2(movementSpeed, 0));
            }
            gameObject.transform.localScale =
                new Vector3(4, gameObject.transform.localScale.y, gameObject.transform.localScale.z);
            gameObject.GetComponent<Animator>().SetBool("isMoving", true);
        }
        else if (Input.GetKey("a"))
        {
            //If we are in the air, halve the movement speed of the player
            if (inAir || !(coyoteTime > 0) || !(jumpBuffered))
            {
                rb.AddForce(new Vector2(-movementSpeed / 2, 0));
            }
            else
            {
                rb.AddForce(new Vector2(-movementSpeed, 0));
            }
            
            gameObject.transform.localScale =
                new Vector3(-4, gameObject.transform.localScale.y, gameObject.transform.localScale.z);
            gameObject.GetComponent<Animator>().SetBool("isMoving", true);
        }
        //If we are not moving slow to a stop
        else
        {
            //No Friction in the Air
            if (!inAir)
            {
                rb.velocity = new Vector2((rb.velocity.x / (1 + frictionConstant * Time.deltaTime)), rb.velocity.y);
            }
            gameObject.GetComponent<Animator>().SetBool("isMoving", false);
        }
        
        //Clamp the Horizontal Movement Speed
        rb.velocity = new Vector2(Mathf.Clamp(rb.velocity.x, -maximumSpeed, maximumSpeed), rb.velocity.y);
        
        
    }

    private void Update()
    {
        //Jump Buffering
        if (Input.GetKeyDown("space") && inAir)
        {
            bufferTime = startTime;
            jumpBuffered = true;
        }
        if (bufferTime < 0)
        {
            jumpBuffered = false;
        }
        
        //Handle Jumping
        if ((Input.GetKeyDown("space") && !inAir) || (!inAir && jumpBuffered) || (coyoteTime > 0 && Input.GetKeyDown("space")))
        {
            Jump();
        }
        
        //Update Timers
        time -= Time.deltaTime;
        bufferTime -= Time.deltaTime;
        coyoteTime -= Time.deltaTime;
        
        //While holding down space, and within the timer
        if (Input.GetKey("space") && time > 0)
        {
            //No gravity, increase jump height
            currentGravity = 0;
        }
        //Once the timer wears off, or space is released early turn gravity back on
        else
        {
            currentGravity = worldGravity;
        }

        RaycastHit2D leftHit = Physics2D.Linecast(gameObject.transform.position, new Vector2(gameObject.transform.position.x - 0.35f, gameObject.transform.position.y - 0.75f),_layerMask);
        RaycastHit2D RightHit = Physics2D.Linecast(gameObject.transform.position, new Vector2(gameObject.transform.position.x + 0.35f, gameObject.transform.position.y - 0.75f),_layerMask);
        Debug.DrawLine(gameObject.transform.position, new Vector2(gameObject.transform.position.x - 0.35f, gameObject.transform.position.y - 0.75f));
        Debug.DrawLine(gameObject.transform.position, new Vector2(gameObject.transform.position.x + 0.35f, gameObject.transform.position.y - 0.75f));
        if (leftHit.collider != null && !Input.GetKey("space"))
        {
            Debug.Log(leftHit.collider);
            inAir = false;
        }
        
        //Coyote Jump
        //If we leave the ground and we didn't press the jump we can still jump
        if (!inAir && !Input.GetKey("space"))
        {
            coyoteTime = coyoteStartTime;
        }
        
        
        //Clamp Falling Speed
        rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -30, 100));
    }

    void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpPower);
        inAir = true;
        time = startTime;
    }
}
