using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FlyerAI : MonoBehaviour
{
    /* Linked objects */
    public Behaviour halo;
    public GameObject[] waypoints;
    public GameObject projectile;

    private NavMeshAgent nav;              // Reference to the nav mesh agent.
    private PlayerController player;        // Reference to the player.
    private int playerHealth;
    private Animator anim;
    private NavMeshHit ht;

    /* Stats */
    public int maxEnemyHealth = 3;          // Maximum allowed health
    public int enemyHealth;            // Current health
    private int cwp = -1;
    private float attackTime;
    public float timeBetweenAttacks = 5f;
    public bool fire = false;               // Animated variable; when true, spawns a projectile
    private bool attacking = false;

    //material stuff
    public Material attackM;
    public Material chaseM;
    public Material idleM;
    public Material deadM;

    public enum States
    {
        Chase,
        Attack,
        PDead,
        Dead,
        Idle
    };
    public States state;

    void Start()
    {
        enemyHealth = maxEnemyHealth;
        waypoints = GameObject.FindGameObjectsWithTag("wp");
        player = GameObject.FindGameObjectWithTag("player").GetComponent<PlayerController>();
        nav = this.transform.root.GetComponent<NavMeshAgent>();
        nav.enabled = true;
        anim = GetComponent<Animator>();
        state = States.Idle;
    }

    // Update is called once per frame
    void Update()
    {
        playerHealth = player.getHealth();
        float xzVel = nav.velocity.magnitude;
        anim.SetFloat("xz-vel", xzVel, 1f, Time.deltaTime * 10f);

        Ray ray = new Ray(this.transform.position, player.getPosition() + Vector3.up * 4 - this.transform.position);
        RaycastHit rayInfo = new RaycastHit();

        if (fire)
        {
            //fire = false;
            Instantiate(projectile, this.transform.position, Quaternion.LookRotation((player.getPosition() - this.transform.position).normalized * 5 + new Vector3(Random.value, Random.value, Random.value)));
            Instantiate(projectile, this.transform.position, Quaternion.LookRotation((player.getPosition() - this.transform.position).normalized * 3 + new Vector3(Random.value, Random.value, Random.value)));
            state = States.Idle;
            attacking = false;
        }

        switch (state)
        {
            case States.Chase:
                if (anim.GetCurrentAnimatorStateInfo(0).IsName("Idle") || anim.GetCurrentAnimatorStateInfo(0).IsName("Flying"))
                {
                    chase_player();
                    nav.enabled = true;
                    halo.enabled = false;
                    if (enemyHealth == 0)
                    {
                        state = States.Dead;
                        break;
                    }
                    if (playerHealth == 0)
                    {
                        state = States.PDead;
                        break;
                    }

                    if (attackTime > timeBetweenAttacks)
                    {
                        Debug.DrawRay(this.transform.position, (player.getPosition() + Vector3.up * 4 - this.transform.position).normalized * 70, Color.red);
                        if (Physics.Raycast(ray, out rayInfo, 70))
                        {
                            if (rayInfo.collider.tag == "player")
                            {
                                state = States.Attack;
                                break;
                            }
                            else
                            {
                                state = States.Idle;
                            }
                        }
                    }
                    else
                    {
                        Debug.DrawRay(this.transform.position, (player.getPosition() + Vector3.up * 4 - this.transform.position).normalized * 70, Color.green);
                    }

                    /*if (nav.Raycast(player.getPosition(), out ht))
                    {
                        state = States.Idle;
                    }*/
                }
                else if (anim.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
                {
                    anim.ResetTrigger("attack");
                    nav.enabled = false;
                }
                break;

            case States.Attack:
                if (!attacking)
                {
                    attack();
                }
                else
                {
                    nav.transform.rotation = Quaternion.Slerp(nav.transform.rotation, Quaternion.LookRotation(player.getPosition() - this.transform.position), 300 * Time.deltaTime);
                }
                attacking = true;
                break;

            case States.PDead:
                halo.enabled = false;
                nav.enabled = false;
                //player is dead
                break;

            case States.Dead:
                nav.enabled = false;
                halo.enabled = false;
                // need to figure out what we want to do when enemy is dead
                //ie death animation, deletion, etc. 
                break;

            case States.Idle:
                halo.enabled = false;
                attackTime = 0;

                if (enemyHealth == 0)
                {
                    state = States.Dead;
                    break;
                }

                Debug.DrawRay(this.transform.position, (player.getPosition() + Vector3.up * 4 - this.transform.position).normalized * 70, Color.blue);
                if (Physics.Raycast(ray, out rayInfo, 70) && rayInfo.collider.tag == "player")
                {
                    state = States.Chase;
                }
                else
                {
                    //getWP();
                    if (nav.pathPending == false && nav.remainingDistance <= 2)
                    {
                        setNextWaypoint();
                    }
                    // setNextWaypoint();
                }
                break;

            default:
                halo.enabled = false;
                state = States.Idle;
                break;
        }

    }

    private void FixedUpdate()
    {
        attackTime += 1 * Time.deltaTime;
    }

    private void chase_player()
    {
        NavMesh.FindClosestEdge(player.getPosition() + (this.transform.position - player.getPosition()).normalized * 3, out ht, NavMesh.AllAreas);
        Debug.DrawLine(ht.position, ht.position*1.001f);

        nav.SetDestination(ht.position);
    }

    private void attack()
    {
        //add attack funtionality when animation and stuff is done
        attackTime = 0;
        halo.enabled = true;
        anim.SetTrigger("attack");
    }

    /* Health-related methods */
    public int getHealth()
    {
        return enemyHealth;
    }

    public void Hit()
    {
        enemyHealth -= 1;

        if (enemyHealth <= 0)
        {
            Destroy(this.transform.root.gameObject);
        }
    }
    public void Hit(int damage)
    {
        enemyHealth -= damage;

        if (enemyHealth <= 0)
        {
            Destroy(this.transform.root.gameObject);
        }
    }

    public void fullHealth()
    {
        enemyHealth = maxEnemyHealth;
    }

    private void setNextWaypoint()
    {
        if (waypoints.Length == 0)
        {
            return;
        }
        if (waypoints.Length - 1 == cwp)
        {
            cwp = 0;
        }
        else
        {
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

    }
}
