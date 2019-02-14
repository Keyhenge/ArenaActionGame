//Used from CS 4455 Milestone 1

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(Animator), typeof(Rigidbody), typeof(CapsuleCollider))]
[RequireComponent(typeof(CharacterInputController))]
public class PlayerController : MonoBehaviour
{
    //Attached objects
    private Rigidbody rbody;
    private Animator anim;
    private CharacterInputController cinput;
    public ParticleSystem impact;
    public ParticleSystem pickup;
    public GameObject jumpTrigger;
    public GameObject cameraObject;
    private CameraController cam;

    //private Transform leftFoot;
    //private Transform rightFoot;

    // Object Properties
    public float speed = 1f;
    public float maxSpeed = 10f;
    public float maxTurnSpeed = 100f;
    private bool inAir;
    private float moveHeight;

    // Useful if you implement jump in the future...
    public float jumpableGroundNormalMaxAngle = 45f;
    public bool closeToJumpableGround;
    public bool isGrounded;

    //Text
    public Text pauseText;
    public Text winText;
    public Text titleText;

    // Shake
    public float shakeThreshold = 4f;
    public float shakeDecrease = 10f;
    private bool thresholdReached = false;
    private float mostRecentYVel = 0f;

    // Game stats
    public int health;


    void Awake()
    {

        anim = GetComponent<Animator>();

        if (anim == null)
            Debug.Log("Animator could not be found");

        rbody = GetComponent<Rigidbody>();

        if (rbody == null)
            Debug.Log("Rigid body could not be found");

        cinput = GetComponent<CharacterInputController>();

        if (cinput == null)
            Debug.Log("CharacterInputController could not be found");

    }


    void Start()
    {
        //example of how to get access to certain limbs
        //leftFoot = this.transform.Find("mixamorig:Hips/mixamorig:LeftUpLeg/mixamorig:LeftLeg/mixamorig:LeftFoot");
        //rightFoot = this.transform.Find("mixamorig:Hips/mixamorig:RightUpLeg/mixamorig:RightLeg/mixamorig:RightFoot");

        /*if (leftFoot == null || rightFoot == null)
            Debug.Log("One of the feet could not be found");*/

        anim.applyRootMotion = false;

        isGrounded = false;

        //never sleep so that OnCollisionStay() always reports for ground check
        rbody.sleepThreshold = 0f;
    }


    void Update()
    {
        // Event-based inputs need to be handled in Update()
        if (cinput.enabled && cinput.Action)
        {
            if (cinput.Action)
                Debug.Log("Action pressed");

            anim.SetTrigger("attack");
            //anim.speed = animationSpeed;
        }


    }


    void FixedUpdate()
    {
        // Calculate movement vector
        float moveHorizontal = 0f;
        float moveVertical = 0f;
        moveHorizontal = Input.GetAxis("Horizontal");
        moveVertical = Input.GetAxis("Vertical");
        Vector2 moveCalc = new Vector2(moveHorizontal, moveVertical);
        moveCalc.Normalize();
        Vector3 movement = new Vector3(moveCalc.x, moveHeight, moveCalc.y);
        float inputMove = movement.magnitude;
        moveHeight = 0;

        /* Allows for shake on jump
        if (rbody.velocity.y * -1 > shakeThreshold)
        {
            thresholdReached = true;
        }
        else
        {
            thresholdReached = false;
        }
        mostRecentYVel = rbody.velocity.y;*/

        //onCollisionStay() doesn't always work for checking if the character is grounded from a playability perspective
        //Uneven terrain can cause the player to become technically airborne, but so close the player thinks they're touching ground.
        //Therefore, an additional raycast approach is used to check for close ground
        if (CharacterCommon.CheckGroundNear(this.transform.position, jumpableGroundNormalMaxAngle, 0.1f, 1f, out closeToJumpableGround))
            isGrounded = true;

        if (inputMove > 0)
        {
            this.transform.Translate(Vector3.forward * inputMove * Time.deltaTime * maxSpeed);
            //It's supposed to be safe to not scale with Time.deltaTime (e.g. framerate correction) within FixedUpdate()
            //If you want to make that optimization, you can precompute your velocity-based translation using Time.fixedDeltaTime
            //We use rbody.MovePosition() as it's the most efficient and safest way to directly control position in Unity's Physics
            rbody.MovePosition(rbody.position + this.transform.forward * inputMove * Time.deltaTime * maxSpeed);

            this.transform.rotation = Quaternion.RotateTowards(this.transform.rotation, Quaternion.LookRotation(movement), maxTurnSpeed * Time.deltaTime);
            //Most characters use capsule colliders constrained to not rotate around X or Z axis
            //However, it's also good to freeze rotation around the Y axis too. This is because friction against walls/corners
            //can turn the character. This errant turn is disorienting to players. 
            //Luckily, we can break the frozen Y axis constraint with rbody.MoveRotation()
            //BTW, quaternions multiplied has the effect of adding the rotations together
            rbody.transform.rotation = Quaternion.RotateTowards(rbody.transform.rotation, Quaternion.LookRotation(movement), maxTurnSpeed * Time.deltaTime);
        }


        //anim.SetFloat("velx", inputTurn); 
        //anim.SetFloat("vely", inputForward);
        //anim.SetBool("isFalling", !isGrounded);


        //clear for next OnCollisionStay() callback
        isGrounded = false;


    }

    /* Actions */
    void Jump()
    {
        if (!inAir && Input.GetKeyDown("space"))
        {
            moveHeight = 30;
        }
    }

    /* Collisions */

    //This is a physics callback
    void OnCollisionStay(Collision collision)
    {
        isGrounded = true;
    }

    void OnCollisionExit(Collision collision)
    {
        isGrounded = false;
    }

    //This is a physics callback
    void OnCollisionEnter(Collision collision)
    {

        /*if (collision.transform.gameObject.tag == "ground")
        {
            //EventManager.TriggerEvent<PlayerLandsEvent, Vector3, float>(collision.contacts[0].point, collision.impulse.magnitude);
        }*/

    }

    /* Getter methods */
    
    // Returns health of the player
    public int getHealth()
    {
        return health;
    }

    // Returns transform position of the player
    public Vector3 getPosition()
    {
        return this.transform.position;
    }

    /* Setter methods */

    // Sets health of the player
    public void setHealth(int h)
    {
        health = h;
    }

    // Sets transform position of the player
    public void getPosition(Vector3 v)
    {
        this.transform.position = v;
    }

}
