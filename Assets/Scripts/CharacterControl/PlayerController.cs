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
    public ParticleSystem impact;               // Particle system for hitting the floor
    public ParticleSystem pickup;               // Particle system for picking up item       
    public EnemySpawner spawner;
    private Rigidbody rbody;                    // Rigidbody reference
    private Animator anim;                      // Animator reference
    private CharacterInputController cinput;    // Input reference 

    [Header("Cameras")]
    public Camera mainCam;                      // Main third-person camera
    public Camera auxCam;                       // Auxillary first-person camera
    public GameObject auxCamTarget;             // Target for auxillary first-person camera
    public GameObject testObject;               // Test object for character rotation issues                     

    [Header("Object Properties")]
    // Speed-related
        public float maxSpeed = 10f;            // Max translation speed of character
        public float maxTurnSpeed = 100f;       // Max rotation speed of character
        public float animationSpeed = 1f;       // Animation speed multiplier
    // Jump-related
        public float jumpHeight = 40f;          // Force applied to character when jumping off of ground
        public float bounceHeight = 20f;        // Force applied to character when hitting enemy
        private bool inAir;                     // Whether character is in air (true) or on ground (false)
        private bool jumping;                   // Whether character has initiated a jump (reset on full transition to jumping animation state)
        private float moveHeight;               // Upward force on character
        private bool closeToJumpableGround;     // Whether character is close to ground or not
    // Gun-related
        private bool aiming;                    // Whether character is aiming
    // Animation-related
        private bool changedState;              // Whether character has changed state since last hit
    // Attack/input-related
        private bool airAttack;                 // Whether character can still attack in air
        public float noInputTime = 0.3f;        // Time after attack connects in which player cannot affect movement
        private float noInput;                  // Storage for above

    // Reserved sections for future updates
    [Header("UI")]
    public DeathMenuController deathScreen;
    public Text scoreText;
    private int score = 0;

    [Header("Camera Shake")]
    public float shakeGroundAttack = 10f;
    public float shakeAirAttack = 15f;
    public float shakeDamage = 5f;
    public float shakeDecrease = 10f;

    [Header("Game Stats")]
    public int maxHealth = 2;                   // Max health
    public int health;                          // Remaining health
    public int ammo;                            // Remaining ammo


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
        health = maxHealth;

        auxCam.enabled = false;
        mainCam.enabled = true;
        anim.speed = animationSpeed;
        changedState = true;
    }


    void Update()
    {
        if (health <= 0)
        {
            deathScreen.playerDied = true;
        }

        //onCollisionStay() doesn't always work for checking if the character is grounded from a playability perspective
        //Uneven terrain can cause the player to become technically airborne, but so close the player thinks they're touching ground.
        //Therefore, an additional raycast approach is used to check for close ground
        if (CharacterCommon.CheckGroundNear(this.transform.position, 0.1f, 1f, out closeToJumpableGround))
        {
            inAir = false;
            airAttack = true;
            anim.SetBool("airAttack", true);
        }
        else
        {
            inAir = true;
        }

        // Event-based inputs need to be handled in Update()
        if (cinput.enabled)
        {
            // Attack
            if (cinput.Action && anim.GetCurrentAnimatorStateInfo(0).IsName("Ground"))
            {
                Debug.Log("Player: Attack");
                anim.SetTrigger("attack");
            }
            if (cinput.Action && airAttack && (jumping || inAir))
            {
                Debug.Log("Player: Air Attack");
                airAttack = false;
                anim.SetTrigger("attack");
            }
            if (anim.GetCurrentAnimatorStateInfo(0).IsName("Air Swipe"))
            {
                //Debug.Log("Air swiped");
                anim.SetBool("airAttack", airAttack);
            }
            if (anim.GetCurrentAnimatorStateInfo(0).IsName("Swipe"))
            {
                //Debug.Log("Swiped");
                anim.ResetTrigger("attack");
            }

            // Jump
            if (Input.GetKeyDown(KeyCode.Space) && !inAir)
            {
                Debug.Log("Player: Jump");
                jumping = true;
                Jump();
            }
            if (anim.GetCurrentAnimatorStateInfo(0).IsName("Falling"))
            {
                jumping = false;
            }

            // Aim
            if (Input.GetKey(KeyCode.Mouse1) && !aiming)
            {
                Debug.Log("Player: Aim");
                aiming = true;
            }
            if (!(Input.GetKey(KeyCode.Mouse1)) && aiming)
            {
                Debug.Log("Player: Stop aiming");
                aiming = false;
            }

            // Attack recovery
            if (anim.GetCurrentAnimatorStateInfo(0).IsName("Air Swipe Recovery") || anim.GetCurrentAnimatorStateInfo(0).IsName("Swipe Recovery"))
            {
                changedState = true;
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
        float moveHorizontal = 0f;
        float moveVertical = 0f;
        Vector2 moveCalc = new Vector2();
        float xzVel = 0f;
        Vector3 movement = new Vector3();
        Vector3 movementXZ = new Vector3();

        // Calculate movement vector
        if (noInput <= 0f)
        {
            moveHorizontal = Input.GetAxis("Horizontal");
            moveVertical = Input.GetAxis("Vertical");
            moveCalc = new Vector2(moveHorizontal, moveVertical);
            moveCalc.Normalize();
            xzVel = moveCalc.magnitude;
            movement = new Vector3(moveCalc.x, moveHeight, moveCalc.y);
            movementXZ = new Vector3(moveCalc.x, 0, moveCalc.y);
        }
        else // Decay no input time
        {
            noInput -= 1f * Time.deltaTime;
        }

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

        // Calculate character translation
        if (xzVel > 0)
        {
            // Calculate movement of camera and character independent of camera
            // Calculate camera position on flat plane
            Vector3 flatCameraRelative = mainCam.transform.TransformDirection(movementXZ);
            flatCameraRelative = new Vector3(flatCameraRelative.x, 0, flatCameraRelative.z);
            flatCameraRelative.Normalize();
            if (testObject.active)
            {
                testObject.transform.position = flatCameraRelative * 2 + this.transform.position;
            }

            //this.transform.Translate(Vector3.forward * Time.deltaTime * maxSpeed);
            rbody.AddForce(this.transform.forward * Time.deltaTime * maxSpeed * 500f);
            Vector3 xzVelVector = new Vector3(rbody.velocity.x, 0, rbody.velocity.z);
            if (xzVelVector.magnitude > maxSpeed)
            {
                rbody.AddForce((xzVelVector/xzVelVector.magnitude * maxSpeed) - xzVelVector * 8f);
            }

            // Calculate character rotation
            rbody.transform.rotation = Quaternion.RotateTowards(rbody.transform.rotation, Quaternion.LookRotation(flatCameraRelative), maxTurnSpeed * Time.deltaTime);
            if (inAir)
            {
                rbody.transform.rotation = Quaternion.RotateTowards(rbody.transform.rotation, Quaternion.LookRotation(flatCameraRelative), maxTurnSpeed * Time.deltaTime);
            }
        }
        else if (anim.GetCurrentAnimatorStateInfo(0).IsName("Aim Rifle") || anim.GetCurrentAnimatorStateInfo(0).IsName("Aiming Rifle"))
        {
            Vector3 rotateTarget = new Vector3(auxCamTarget.transform.position.x - this.transform.position.x, 0, auxCamTarget.transform.position.z - this.transform.position.z);

            // Calculate character/camera rotation
            rbody.transform.rotation = Quaternion.RotateTowards(rbody.transform.rotation, Quaternion.LookRotation(rotateTarget), maxTurnSpeed * Time.deltaTime);
        }
        if (moveHeight > 0)
        {
            rbody.AddForce(0, moveHeight, 0, ForceMode.Impulse);
        }

        anim.SetBool("inAir", inAir);

        //clear for next OnCollisionStay() callback
        moveHeight = 0;
        inAir = true;
    }


    /* Actions */
    void Jump()
    {
        if (!inAir && Input.GetKeyDown("space"))
        {
            moveHeight = jumpHeight;
        }
    }


    /* Collisions */
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.gameObject.tag == "ground")
        {
            //EventManager.TriggerEvent<PlayerLandsEvent, Vector3, float>(collision.contacts[0].point, collision.impulse.magnitude);
            inAir = false;
            moveHeight = 0;
        } else if (collision.transform.gameObject.tag == "enemy")
        {
            Debug.Log("Player: Contact Damage");
            health -= 1;
            Bounce(collision, 5f, 1000f);
        }
    }
    private void OnCollisionStay(Collision collision)
    {
        if (collision.transform.gameObject.tag == "ground")
        {
            inAir = false;
            moveHeight = 0;
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        inAir = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Hit Brute head
        if (other.transform.gameObject.name == "Head" && (anim.GetCurrentAnimatorStateInfo(0).IsName("Swipe") || anim.GetCurrentAnimatorStateInfo(0).IsName("Air Swipe")) && changedState)
        {
            Debug.Log("Player: Hit Brute");
            // Do a bounce if airborne hit
            if (anim.GetCurrentAnimatorStateInfo(0).IsName("Air Swipe"))
            {
                rbody.velocity = Vector3.zero;
                //rbody.angularVelocity = Vector3.zero;
                moveHeight = bounceHeight;
                Bounce(other, 15f, 1000f);
            }

            // Head -> pSphere11 -> Brute
            BasicEnemyMovement brute = other.transform.gameObject.
                transform.parent.gameObject.
                transform.parent.gameObject.
                GetComponent<BasicEnemyMovement>();
            if (brute.getHealth() == 1)
            {
                Debug.Log("Player: Killed Brute");
                score++;
                scoreText.text = "Score: " + score.ToString();
                spawner.KilledEnemy();
            }
            brute.hit();

            changedState = false;
        }
    }

    private void Bounce(Collision col, float xzMult, float force)
    {
        Vector3 dir = this.transform.position - col.gameObject.transform.position;
        dir = dir.normalized;
        dir = new Vector3(dir.x * xzMult, dir.y, dir.z * xzMult);
        rbody.AddForce(dir.normalized * force);
        noInput = noInputTime;
    }
    private void Bounce(Collider col, float xzMult, float force)
    {
        Vector3 dir = this.transform.position - col.gameObject.transform.position;
        dir = dir.normalized;
        dir = new Vector3(dir.x * xzMult, dir.y, dir.z * xzMult);
        rbody.AddForce(dir.normalized * force);
        noInput = noInputTime;
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
    public void setPosition(Vector3 v)
    {
        this.transform.position = v;
    }

}
