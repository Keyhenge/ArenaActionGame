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
    public BoxCollider sword;
    public AudioSource audioSource;
    public AudioClip gunshot;

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
    public Text scoreText2;
    public RawImage crosshairs;
    public Text healthText;
    public Text ammoText;

    [Header("Camera Shake")]
    public float shakeGroundAttack = 3f;        // Shake on hit with ground attack
    public float shakeAirAttack = 3f;           // Shake on hit with air attack
    public float shakeDamage = 2f;              // Shake on damage taken
    public float shakeKill = 4f;                // Shake on enemy kill
    public float shakeTime = 0.5f;              // Time shake lasts

    [Header("Game Stats")]
    public int maxHealth = 5;                   // Max health allowed/restored
    public int maxAmmo = 5;                     // Max ammo allowed/restored
    private float invincible = 0f;              // Container for invincible frames
    private int health;                         // Remaining health
    private int ammo;                           // Remaining ammo
    public int score;


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

        health = maxHealth;
        ammo = maxAmmo;
        score = 0;
    }


    void Start()
    {
        health = maxHealth;
        anim.applyRootMotion = false;
        inAir = true;

        //never sleep so that OnCollisionStay() always reports for ground check
        rbody.sleepThreshold = 0f;

        auxCam.enabled = false;
        mainCam.enabled = true;
        crosshairs.enabled = false;
        anim.speed = animationSpeed;
        changedState = true;
    }


    void Update()
    {
        // If player has no health left, they're dead
        if (health <= 0)
        {
            deathScreen.playerDied = true;
        }

        // Update UI elements
        healthText.text = "Health: " + health;
        ammoText.text = "Ammo: " + ammo;
        scoreText.text = "Score: " + score;
        scoreText2.text = "Score: " + score;

        //onCollisionStay() doesn't always work for checking if the character is grounded from a playability perspective
        //Uneven terrain can cause the player to become technically airborne, but so close the player thinks they're touching ground.
        //Therefore, an additional raycast approach is used to check for close ground
        if (CharacterCommon.CheckGroundNear(this.transform.position, 0.5f, 1f, out closeToJumpableGround))
        {
            inAir = false;
            airAttack = true;
            anim.SetBool("airAttack", true);
        }
        else
        {
            inAir = true;
        }

        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Swipe") || anim.GetCurrentAnimatorStateInfo(0).IsName("Air Swipe"))
        {
            sword.enabled = true;
        } else
        {
            sword.enabled = false;
        }

        // Event-based inputs need to be handled in Update()
        if (cinput.enabled)
        {
            // Attack
            if (cinput.Action && (anim.GetCurrentAnimatorStateInfo(0).IsName("Ground") || anim.GetCurrentAnimatorStateInfo(0).IsName("Swipe Recovery")))
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
                anim.SetBool("airAttack", airAttack);
            }
            if (anim.GetCurrentAnimatorStateInfo(0).IsName("Swipe"))
            {
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

            // Aim/Fire
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
            if (Input.GetKeyDown(KeyCode.Mouse0) && aiming && anim.GetCurrentAnimatorStateInfo(0).IsName("Aiming Rifle"))
            {
                if (ammo > 0)
                {
                    Debug.Log("Player: Fire");
                    anim.SetTrigger("attack");
                    FireRifle();
                    ammo--;
                }
                else
                {
                    Debug.Log("Player: Out of Ammo");
                }
            }

            // Attack recovery
            if (anim.GetCurrentAnimatorStateInfo(0).IsName("Air Swipe Recovery") || anim.GetCurrentAnimatorStateInfo(0).IsName("Swipe Recovery"))
            {
                changedState = true;
            }

            anim.SetBool("aiming", aiming);
        }

        // Switch between first-person and third-person
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Aim Rifle") || anim.GetCurrentAnimatorStateInfo(0).IsName("Aiming Rifle") || anim.GetCurrentAnimatorStateInfo(0).IsName("Fire Rifle"))
        {
            auxCam.enabled = true;
            mainCam.enabled = false;
            crosshairs.enabled = true;
            Debug.DrawRay(auxCam.transform.position, auxCam.transform.forward * 40, Color.green);
        }
        else
        {
            auxCam.enabled = false;
            mainCam.enabled = true;
            crosshairs.enabled = false;
        }
    }


    void FixedUpdate()
    {
        if (invincible > 0)
        {
            invincible -= Time.deltaTime;
        }

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
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Swipe") || (anim.GetCurrentAnimatorStateInfo(0).IsName("Aim Rifle") || anim.GetCurrentAnimatorStateInfo(0).IsName("Aiming Rifle") || anim.GetCurrentAnimatorStateInfo(0).IsName("Fire Rifle")))
        {
            movement = new Vector3();
            xzVel = 0f;
            moveHeight = 0f;
        }

        // Calculate character translation
        if (xzVel > 0)
        {
            // Calculate movement of camera and character independent of camera
            // Calculate camera position on flat plane
            Vector3 flatCameraRelative = mainCam.transform.TransformDirection(movementXZ);
            flatCameraRelative = new Vector3(flatCameraRelative.x, 0, flatCameraRelative.z);
            flatCameraRelative.Normalize();
            if (testObject.activeSelf)
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
            rbody.transform.rotation = Quaternion.RotateTowards(rbody.transform.rotation, Quaternion.LookRotation(flatCameraRelative), maxTurnSpeed * Time.deltaTime * 10);
        }
        else if (anim.GetCurrentAnimatorStateInfo(0).IsName("Aim Rifle") || anim.GetCurrentAnimatorStateInfo(0).IsName("Aiming Rifle") || anim.GetCurrentAnimatorStateInfo(0).IsName("Fire Rifle"))
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
        //inAir = true;
    }


    /* Actions */
    void Jump()
    {
        if (!inAir && Input.GetKeyDown("space"))
        {
            moveHeight = jumpHeight;
        }
    }

    void FireRifle()
    {
        RaycastHit hit = new RaycastHit();
        audioSource.PlayOneShot(gunshot, 0.5f);

        if (Physics.Raycast(auxCam.transform.position, auxCam.transform.forward, out hit, 10000f, 1 << 10))
        {
            Debug.Log("Player: Hit enemy with Rifle");
            GameObject enemy = hit.collider.gameObject;

            // Brute
            if (enemy.transform.root.GetChild(0).GetComponent<BasicEnemyMovement>() != null)
            {
                BasicEnemyMovement brute = enemy.transform.root.GetChild(0).GetComponent<BasicEnemyMovement>();
                if (brute.getHealth() <= 10)
                {
                    Debug.Log("Player: Killed Brute");
                    score++;
                    spawner.KilledEnemy();
                }
                brute.Hit(10);
            }

            // Buffer
            if (enemy.transform.root.GetChild(0).GetComponent<BufferAI>() != null)
            {
                BufferAI buffer = enemy.transform.root.GetChild(0).GetComponent<BufferAI>();
                if (buffer.getHealth() <= 10)
                {
                    Debug.Log("Player: Killed Buffer");
                    score++;
                    spawner.KilledEnemy();
                }
                buffer.Hit(10);
            }

            Debug.Log("SOMETHING");


            // Flyer
            if (enemy.transform.root.transform.GetChild(0).transform.GetChild(0).GetComponent<FlyerAI>() != null)
            {
                
                FlyerAI flyer = enemy.transform.root.transform.GetChild(0).transform.GetChild(0).GetComponent<FlyerAI>();
                if (flyer.getHealth() <= 10)
                {
                    Debug.Log("Player: Killed Flyer");
                    score++;
                    spawner.KilledEnemy();
                }
                flyer.Hit(10);
            }
        }
        auxCamTarget.GetComponent<AuxCameraController>().shake(shakeAirAttack / 4f);
    }


    /* Collisions */
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.gameObject.tag == "ground")
        {
            inAir = false;
            moveHeight = 0;
        }
        else if (collision.transform.gameObject.tag == "enemy")
        {
            TakeDamage();
            Bounce(collision, 5f, 1000f);
        }
    }
    private void OnCollisionStay(Collision collision)
    {
        /*if (collision.transform.gameObject.tag == "ground" && inAir)
        {
            moveHeight = 2;
        }*/
    }
    private void OnCollisionExit(Collision collision)
    {
        inAir = true;
    }


    //NOTE: MAKE THESE INTO SEPARATE FUNCTIONS LATER
    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log(other.transform.gameObject.name);
        //Debug.Log("changedState: " + changedState);
        // Hit Brute
        if (other.transform.gameObject.name == "Head" && (anim.GetCurrentAnimatorStateInfo(0).IsName("Swipe") || anim.GetCurrentAnimatorStateInfo(0).IsName("Air Swipe")) && changedState)
        {
            Debug.Log("Player: Hit Brute with Sword");
            // Do a bounce if airborne hit
            if (anim.GetCurrentAnimatorStateInfo(0).IsName("Air Swipe"))
            {
                mainCam.GetComponent<CameraController>().shake(shakeAirAttack, shakeTime);
                rbody.velocity = Vector3.zero;
                //rbody.angularVelocity = Vector3.zero;
                moveHeight = bounceHeight;
                Bounce(other, 15f, 1000f);
            }
            else if (anim.GetCurrentAnimatorStateInfo(0).IsName("Swipe"))
            {
                mainCam.GetComponent<CameraController>().shake(shakeGroundAttack, shakeTime);
            }

            // Head -> pSphere11 -> Brute
            BasicEnemyMovement brute = other.transform.gameObject.
                transform.parent.gameObject.
                transform.parent.gameObject.
                GetComponent<BasicEnemyMovement>();
            if (brute.getHealth() == 1)
            {
                Debug.Log("Player: Killed Brute");
                mainCam.GetComponent<CameraController>().shake(shakeKill, shakeTime+0.1f);
                score++;
                spawner.KilledEnemy();
            }
            brute.Hit();

            changedState = false;
        }

        // Hit Buffer
        else if (other.transform.root.transform.GetChild(0).gameObject.name == "Buffer" && (anim.GetCurrentAnimatorStateInfo(0).IsName("Swipe") || anim.GetCurrentAnimatorStateInfo(0).IsName("Air Swipe")) && changedState)
        {
            Debug.Log("Player: Hit Buffer with Sword");
            // Do a bounce if airborne hit
            if (anim.GetCurrentAnimatorStateInfo(0).IsName("Air Swipe"))
            {
                mainCam.GetComponent<CameraController>().shake(shakeAirAttack, shakeTime);
                rbody.velocity = Vector3.zero;
                moveHeight = bounceHeight;
                Bounce(other, 15f, 700f);
            }
            else if (anim.GetCurrentAnimatorStateInfo(0).IsName("Swipe"))
            {
                mainCam.GetComponent<CameraController>().shake(shakeGroundAttack, shakeTime);
            }

            // pCylinder5 -> Buffer
            BufferAI buffer = other.transform.gameObject.
                transform.parent.gameObject.
                GetComponent<BufferAI>();
            if (buffer.getHealth() == 1)
            {
                Debug.Log("Player: Killed Buffer");
                mainCam.GetComponent<CameraController>().shake(shakeKill, shakeTime + 0.1f);
                score++;
                spawner.KilledEnemy();
            }
            buffer.Hit();

            changedState = false;
        }

        // Hit Flyer
        else if (other.transform.root.transform.GetChild(0).gameObject.name == "Flyer_Anim" && (anim.GetCurrentAnimatorStateInfo(0).IsName("Swipe") || anim.GetCurrentAnimatorStateInfo(0).IsName("Air Swipe")) && changedState)
        {
            Debug.Log("Player: Hit Flyer with Sword");
            // Do a bounce if airborne hit
            if (anim.GetCurrentAnimatorStateInfo(0).IsName("Air Swipe"))
            {
                mainCam.GetComponent<CameraController>().shake(shakeAirAttack, shakeTime);
                rbody.velocity = Vector3.zero;
                moveHeight = bounceHeight;
                Bounce(other, 15f, 700f);
            }
            else if (anim.GetCurrentAnimatorStateInfo(0).IsName("Swipe"))
            {
                mainCam.GetComponent<CameraController>().shake(shakeGroundAttack, shakeTime);
            }

            // something -> Flyer_root -> Flyer_Anim -> Flyer
            FlyerAI flyer = other.transform.root.
                transform.GetChild(0).
                transform.GetChild(0).GetComponent<FlyerAI>();
            if (flyer.getHealth() == 1)
            {
                Debug.Log("Player: Killed Flyer");
                mainCam.GetComponent<CameraController>().shake(shakeKill, shakeTime + 0.1f);
                score++;
                spawner.KilledEnemy();
            }
            flyer.Hit();

            changedState = false;
        }

        // Was hit by enemy
        else if (other.transform.tag == "enemyDamage")
        {
            TakeDamage();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        // Hit Buffer
        if (other.transform.gameObject.name == "pCylinder5" && (anim.GetCurrentAnimatorStateInfo(0).IsName("Swipe") || anim.GetCurrentAnimatorStateInfo(0).IsName("Air Swipe")) && changedState)
        {
            Debug.Log("Player: Hit Buffer with Sword");
            // Do a bounce if airborne hit
            if (anim.GetCurrentAnimatorStateInfo(0).IsName("Air Swipe"))
            {
                mainCam.GetComponent<CameraController>().shake(shakeAirAttack, shakeTime);
                rbody.velocity = Vector3.zero;
                //rbody.angularVelocity = Vector3.zero;
                moveHeight = bounceHeight;
                Bounce(other, 15f, 700f);
            }
            else if (anim.GetCurrentAnimatorStateInfo(0).IsName("Swipe"))
            {
                mainCam.GetComponent<CameraController>().shake(shakeGroundAttack, shakeTime);
            }

            // pCylinder5 -> Buffer
            BufferAI buffer = other.transform.gameObject.
                transform.parent.gameObject.
                GetComponent<BufferAI>();
            if (buffer.getHealth() == 1)
            {
                Debug.Log("Player: Killed Buffer");
                mainCam.GetComponent<CameraController>().shake(shakeKill, shakeTime + 0.1f);
                score++;
                spawner.KilledEnemy();
            }
            buffer.Hit();

            changedState = false;
        }
    }

    /* Interactions */
    private void TakeDamage()
    {
        if (invincible <= 0)
        {
            Debug.Log("Player: Took Damage");
            health -= 1;
            if (health > 0)
            {
                mainCam.GetComponent<CameraController>().shake(shakeDamage, shakeTime);
            }
            invincible = 1f;
        }
    }

    public void ResetHealth()
    {
        Debug.Log("Player: Picked up Health");
        health = maxHealth;
    }
    public void ResetAmmo()
    {
        Debug.Log("Player: Picked up Ammo");
        ammo = maxAmmo;
    }

    public void awardPoints(int points)
    {
        Debug.Log("Player: Gained Points");
        score += points;
    }

    // col:     Collision the bounce is based off of
    // xzMult:  Multiplier applied to x and z planes
    // force:   Amount of force applied to player
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
