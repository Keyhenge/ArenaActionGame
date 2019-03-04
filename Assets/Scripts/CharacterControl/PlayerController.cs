//Used from CS 4455 Milestone 1

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(Animator), typeof(Rigidbody), typeof(CapsuleCollider))]
[RequireComponent(typeof(CharacterInputController))]
public class PlayerController : MonoBehaviour
{
    [Header("Attached Objects")]
    public ParticleSystem impact;
    public ParticleSystem pickup;
    public GameObject jumpTrigger;
    public GameObject cameraObject;
    private Rigidbody rbody;
    private Animator anim;
    private CharacterInputController cinput;

    [Header("Cameras")]
    public Camera mainCam;
    public Camera auxCam;
    public GameObject auxCamTarget;
    public GameObject testObject;
    private Camera cam;

    // Object Properties
    // Speed-related
    [Header("Object Properties")]
    public float speed = 1f;
    public float maxSpeed = 10f;
    public float maxTurnSpeed = 100f;
    public float animationSpeed = 1f;
        // Jump-related
    private bool inAir;
    private float moveHeight;
    private bool closeToJumpableGround;
        // Gun-related
    private bool aiming;

    [Header("Text")]
    public Text pauseText;
    public Text winText;
    public Text titleText;

    [Header("Camera Shake")]
    public float shakeThreshold = 4f;
    public float shakeDecrease = 10f;
    private bool thresholdReached = false;
    private float mostRecentYVel = 0f;

    [Header("Game Stats")]
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
        anim.applyRootMotion = false;
        inAir = true;

        //never sleep so that OnCollisionStay() always reports for ground check
        rbody.sleepThreshold = 0f;

        cam = mainCam;
        //auxCam.enabled = false;
        //mainCam.enabled = true;
        anim.speed = animationSpeed;
    }


    void Update()
    {
        // Event-based inputs need to be handled in Update()
        if (cinput.enabled)
        {
            // Attack
            if (cinput.Action)
            {
                Debug.Log("Attack");
                anim.SetTrigger("attack");
            }

            // Jump
            if (Input.GetKeyDown(KeyCode.Space) && !inAir)
            {
                Debug.Log("Jump");
                Jump();
            }

            // Aim
            if (Input.GetKey(KeyCode.Mouse1) && !aiming)
            {
                Debug.Log("Aim");
                aiming = true;
            }
            if (!(Input.GetKey(KeyCode.Mouse1)) && aiming)
            {
                Debug.Log("Stop aiming");
                aiming = false;
            }

            anim.SetBool("aiming", aiming);
        }

        // Switch between first-person and third-person
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Aim Rifle") || anim.GetCurrentAnimatorStateInfo(0).IsName("Aiming Rifle"))
        {
            auxCam.enabled = true;
            mainCam.enabled = false;
        }
        else
        {
            auxCam.enabled = false;
            mainCam.enabled = true;
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
        float xzVel = moveCalc.magnitude;
        Vector3 movement = new Vector3(moveCalc.x, moveHeight, moveCalc.y);
        Vector3 movementXZ = new Vector3(moveCalc.x, 0, moveCalc.y);


        anim.SetFloat("xz-vel", xzVel, 1f, Time.deltaTime * 10f);
        // If in attack animation, only allow movement on transition back to "Ground"/"Falling"
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Swipe") || (anim.GetCurrentAnimatorStateInfo(0).IsName("Aim Rifle") || anim.GetCurrentAnimatorStateInfo(0).IsName("Aiming Rifle")))
        {
            movement = new Vector3();
            xzVel = 0f;
            moveHeight = 0f;
        }

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
        if (CharacterCommon.CheckGroundNear(this.transform.position, 0.1f, 1f, out closeToJumpableGround))
        {
            inAir = false;
        }

        // Calculate character translation
        if (xzVel > 0)
        {
            // Calculate movement of camera and character independent of camera
            // Calculate camera position on flat plane
            Vector3 flatCameraRelative = cam.transform.TransformDirection(movementXZ);
            flatCameraRelative = new Vector3(flatCameraRelative.x, 0, flatCameraRelative.z);
            flatCameraRelative.Normalize();
            if (testObject.active)
            {
                testObject.transform.position = flatCameraRelative * 2 + this.transform.position;
            }

            this.transform.Translate(Vector3.forward * Time.deltaTime * maxSpeed);
            rbody.MovePosition(rbody.position + this.transform.forward * Time.deltaTime * maxSpeed);
            // Calculate character rotation
            this.transform.rotation = Quaternion.RotateTowards(this.transform.rotation, Quaternion.LookRotation(flatCameraRelative), maxTurnSpeed * Time.deltaTime);
            rbody.transform.rotation = Quaternion.RotateTowards(rbody.transform.rotation, Quaternion.LookRotation(flatCameraRelative), maxTurnSpeed * Time.deltaTime);
        }
        else if (anim.GetCurrentAnimatorStateInfo(0).IsName("Aim Rifle") || anim.GetCurrentAnimatorStateInfo(0).IsName("Aiming Rifle"))
        {
            Vector3 rotateTarget = new Vector3(auxCamTarget.transform.position.x - this.transform.position.x, 0, auxCamTarget.transform.position.z - this.transform.position.z);

            // Calculate character/camera rotation
            this.transform.rotation = Quaternion.RotateTowards(this.transform.rotation, Quaternion.LookRotation(rotateTarget), maxTurnSpeed * Time.deltaTime);
            rbody.transform.rotation = Quaternion.RotateTowards(rbody.transform.rotation, Quaternion.LookRotation(rotateTarget), maxTurnSpeed * Time.deltaTime);
        }
        if (moveHeight > 0)
        {
            //this.transform.Translate(Vector3.up * Time.deltaTime * moveHeight);
            //rbody.MovePosition(rbody.position + this.transform.up * Time.deltaTime * moveHeight);
            rbody.AddForce(0, moveHeight, 0, ForceMode.Impulse);
        }

        anim.SetBool("inAir", inAir);
        
        //anim.SetBool("aiming", aiming);

        //clear for next OnCollisionStay() callback
        moveHeight = 0;
        inAir = true;
    }


    /* Actions */
    void Jump()
    {
        if (!inAir && Input.GetKeyDown("space"))
        {
            moveHeight = 70f;
        }
    }


    /* Collisions */
    void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.gameObject.tag == "ground")
        {
            EventManager.TriggerEvent<PlayerLandsEvent, Vector3, float>(collision.contacts[0].point, collision.impulse.magnitude);
        }
        moveHeight = 0;
    }

    void OnCollisionStay(Collision collision)
    {
        inAir = false;
        moveHeight = 0;
    }

    void OnCollisionExit(Collision collision)
    {
        inAir = true;
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

    // Returns transform position of the player
    public Animator getAnimator()
    {
        return anim;
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
