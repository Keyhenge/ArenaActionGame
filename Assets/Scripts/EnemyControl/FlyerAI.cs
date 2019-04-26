using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class FlyerAI : MonoBehaviour
{
    /* Linked objects */
    public Behaviour halo;
    public GameObject[] waypoints;
    public GameObject projectile;

    //private NavMeshAgent nav;              // Reference to the nav mesh agent.
    private PlayerController player;        // Reference to the player.
    private int playerHealth;
    private Animator anim;
    //private NavMeshHit ht;
    private Light light;
    public Slider healthBar;
    private Rigidbody rbody;

    /* Stats */
    public int maxEnemyHealth = 3;          // Maximum allowed health
    public int enemyHealth;            // Current health
    private int cwp = -1;
    private float attackTime;
    public float timeBetweenAttacks = 5f;
    public bool fire = false;               // Animated variable; when true, spawns a projectile
    private bool attacking = false;
    public float maxSpeed = 10f;
    public float vel = 0;

    //material stuff
    public Material attackM;
    public Material chaseM;
    public Material idleM;
    public Material deadM;

    public enum States {
        Chase,
        Attack,
        PDead,
        Dead,
        Idle
    };
    public States state;

    void Start() {
        enemyHealth = maxEnemyHealth;
        waypoints = GameObject.FindGameObjectsWithTag("wp");
        player = GameObject.FindGameObjectWithTag("player").GetComponent<PlayerController>();
        rbody = this.transform.root.GetChild(0).GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        state = States.Chase;
        light = GetComponent<Light>();
    }

    // Update is called once per frame
    void Update() {
        healthBar.value = (float)enemyHealth / (float)maxEnemyHealth;

        playerHealth = player.getHealth();
        float vel = rbody.velocity.magnitude;
        anim.SetFloat("vel", vel, 1f, Time.deltaTime * 10f);
    }

    private void FixedUpdate() {
        light.enabled = false;
        attackTime += 1 * Time.deltaTime;

        if (fire) {
            //fire = false;
            Instantiate(projectile, rbody.transform.position, Quaternion.LookRotation(rbody.transform.forward * 5 + new Vector3(Random.value, Random.value, Random.value)));
            Instantiate(projectile, rbody.transform.position, Quaternion.LookRotation(rbody.transform.forward * 2 + new Vector3(Random.value, Random.value, Random.value)));
            state = States.Idle;
            attacking = false;
        }

        Ray currentDirection = new Ray(rbody.transform.position, rbody.velocity);
        Ray ray = new Ray(rbody.transform.position, player.getPosition() + Vector3.up * 4 - rbody.transform.position);
        Ray ground = new Ray(rbody.transform.position + Vector3.down * 2, Vector3.down * 10);
        RaycastHit rayInfo = new RaycastHit();

        Debug.DrawRay(rbody.transform.position + Vector3.down * 2, (Vector3.down * 10).normalized * 5, Color.magenta);
        if (Physics.Raycast(ground, out rayInfo, 10) && rayInfo.collider.tag == "ground") {
            flyAboveGround(ground, rayInfo);
        } else if (!Physics.Raycast(ground, out rayInfo, 20)) {
            rbody.AddForce(new Vector3(0, -10, 0));
        }

        Debug.DrawRay(rbody.transform.position, rbody.velocity.normalized * 5, Color.magenta);
        if (Physics.Raycast(currentDirection, out rayInfo, 5)) {
            avoidBumping(currentDirection, rayInfo);
        }

        switch (state) {
            case States.Chase:
                if (anim.GetCurrentAnimatorStateInfo(0).IsName("Idle") || anim.GetCurrentAnimatorStateInfo(0).IsName("Flying")) {
                    Physics.Raycast(ray, out rayInfo, 10);
                    Debug.DrawRay(rbody.transform.position, (player.getPosition() + Vector3.up * 4 - rbody.transform.position).normalized * 10, Color.magenta);
                    chasePlayer(ray, rayInfo);
                    halo.enabled = false;
                    if (enemyHealth == 0) {
                        state = States.Dead;
                        break;
                    }

                    if (attackTime > timeBetweenAttacks) {
                        Debug.DrawRay(rbody.transform.position, (player.getPosition() + Vector3.up * 4 - rbody.transform.position).normalized * 50, Color.red);
                        if (Physics.Raycast(ray, out rayInfo, 50)) {
                            if (rayInfo.collider.tag == "player") {
                                state = States.Attack;
                                break;
                            }
                        }
                    } else {
                        Debug.DrawRay(rbody.transform.position, (player.getPosition() + Vector3.up * 4 - rbody.transform.position).normalized * 70, Color.green);
                    }

                    if (Physics.Raycast(ray, out rayInfo, 50)) {
                        if (rayInfo.collider.tag == "player" && rayInfo.distance > 5) {
                            rbody.rotation = Quaternion.Slerp(rbody.transform.rotation, Quaternion.LookRotation(player.getPosition() - this.transform.position), 2 * Time.deltaTime);
                        }
                    }
                } else if (anim.GetCurrentAnimatorStateInfo(0).IsName("Attack")) {
                    anim.ResetTrigger("attack");
                }
                

                break;

            case States.Attack:
                Physics.Raycast(ray, out rayInfo, 10);
                Debug.DrawRay(rbody.transform.position, (player.getPosition() + Vector3.up * 4 - rbody.transform.position).normalized * 10, Color.magenta);
                chasePlayer(ray, rayInfo);

                if (Physics.Raycast(ray, out rayInfo, 50)) {
                    if (rayInfo.collider.tag == "player" && rayInfo.distance > 8) {
                        rbody.rotation = Quaternion.Slerp(rbody.transform.rotation, Quaternion.LookRotation(player.getPosition() - this.transform.position), Time.deltaTime);
                    }
                }

                if (!attacking) {
                    attack();
                } else {
                    //nav.transform.rotation = Quaternion.Slerp(nav.transform.rotation, Quaternion.LookRotation(player.getPosition() - this.transform.position), 300 * Time.deltaTime);
                }
                attacking = true;
                break;

            case States.Dead:
                halo.enabled = false;
                // need to figure out what we want to do when enemy is dead
                //ie death animation, deletion, etc. 
                break;

            case States.Idle:
                rest();
                halo.enabled = false;
                attackTime = 0;

                if (enemyHealth == 0) {
                    state = States.Dead;
                    break;
                }

                Debug.DrawRay(rbody.transform.position, (player.getPosition() + Vector3.up * 4 - rbody.transform.position).normalized * 70, Color.blue);
                if (Physics.Raycast(ray, out rayInfo, 70) && rayInfo.collider.tag == "player") {
                    state = States.Chase;
                }
                break;

            default:
                halo.enabled = false;
                state = States.Idle;
                break;
        }
    }

    private void chasePlayer(Ray ray, RaycastHit rayInfo) {
        if (rbody.velocity.magnitude < maxSpeed) {
            if (rayInfo.collider) {
                if (rayInfo.collider.tag == "player") {
                    Debug.Log("1");
                    rbody.AddForce(ray.direction.normalized * -1);
                } else {
                    Debug.Log("2");
                    rbody.AddForce((ray.direction.normalized + new Vector3(0, 10, 0)).normalized * 1.5f);
                }
            } else {
                Debug.Log("3");
                rbody.AddForce(ray.direction.normalized * 2);
            }
        } else {
            rbody.AddForce(rbody.velocity.normalized * -(rbody.velocity.magnitude - maxSpeed));
        }

        vel = rbody.velocity.magnitude;
    }

    private void flyAboveGround(Ray ray, RaycastHit rayInfo) {
        if (rayInfo.distance < 5 && rbody.velocity.magnitude < maxSpeed) {
            rbody.AddForce((new Vector3(0, 1, 0) * (1 - rayInfo.distance / 5) * 2).normalized);
        } else {
            rbody.AddForce(rbody.velocity.normalized * -(rbody.velocity.magnitude - maxSpeed));
        }
    }

    private void avoidBumping(Ray ray, RaycastHit rayInfo) {
        rbody.AddForce(rbody.velocity.normalized * -(10-rayInfo.distance));
    }

    private void rest() {
        rbody.AddForce(rbody.velocity.normalized * -1);
    }

    private void attack() {
        //add attack funtionality when animation and stuff is done
        attackTime = 0;
        halo.enabled = true;
        anim.SetTrigger("attack");
    }

    /* Health-related methods */
    public int getHealth() {
        return enemyHealth;
    }

    public void Hit() {
        enemyHealth -= 1;

        if (enemyHealth <= 0) {
            Destroy(this.transform.root.gameObject);
        }
    }
    public void Hit(int damage) {
        enemyHealth -= damage;

        if (enemyHealth <= 0) {
            Destroy(this.transform.root.gameObject);
        }
    }

    public void fullHealth() {
        enemyHealth = maxEnemyHealth;
        light.enabled = true;
    }

    /*private void setNextWaypoint() {
        if (waypoints.Length == 0) {
            return;
        }

        if (waypoints.Length - 1 == cwp) {
            cwp = 0;
        } else {
            cwp = cwp + 1;
        }

        nav.SetDestination(waypoints[cwp].transform.position);
        //anim.SetFloat("vely", nm.velocity.magnitude / nm.speed);
    }
    private void getWP()
    {
        waypoints = GameObject.FindGameObjectsWithTag("wp");
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (Vector3.Distance(waypoints[i].transform.position, nav.transform.position) < Vector3.Distance(waypoints[cwp].transform.position, nav.transform.position))
            {
                cwp = i;
            }
        }

    }*/
}
