using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BasicEnemyMovement : MonoBehaviour
{
    /* Linked objects */
    private PlayerController player;        // Reference to the player.
    private int playerHealth;
    private NavMeshAgent nav;              // Reference to the nav mesh agent.
    public GameObject aoe;
    public Behaviour halo;
    private Animator anim;
    private GameObject[] allies;
    private GameObject closest;
    public GameObject[] waypoints;
    private NavMeshHit ht;

    /* Stats */
    public int maxEnemyHealth = 3;          // Maximum allowed health
    private int enemyHealth = 3;            // Current health
    private float attackTime;
    private int cwp = -1;

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

        switch (state)
        {
            case States.Chase:
                if (anim.GetCurrentAnimatorStateInfo(0).IsName("Walking") || anim.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
                {
                    chase_player();
                    nav.enabled = true;
                    aoe.SetActive(false);
                    halo.enabled = false;
                    //Debug.Log(playerHealth);
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
                    if (Vector3.Distance(this.transform.position, player.getPosition()) <= 20 || attackTime > 10)
                    {
                        attackTime = 0;
                        state = States.Attack;
                        break;
                    }
                    if (nav.Raycast(player.getPosition(), out ht))
                    {
                        //getWP();
                        state = States.Idle;
                    }
                }
                else if (anim.GetCurrentAnimatorStateInfo(0).IsName("AOE Attack Damage"))
                {
                    nav.enabled = false;
                    aoe.SetActive(true);
                }
                else if (anim.GetCurrentAnimatorStateInfo(0).IsName("AOE Attack"))
                {
                    anim.ResetTrigger("Attack");
                    nav.enabled = false;
                    aoe.SetActive(false);
                }
                break;

            case States.Attack:
                if (playerHealth == 0)
                {
                    state = States.PDead;
                    break;
                } 
                if (enemyHealth == 0)
                {
                    state = States.Dead;
                    break;
                }
                attack();
                state = States.Idle;
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
                if (enemyHealth == 0)
                {
                    state = States.Dead;
                    break;
                }
                if (!nav.Raycast(player.getPosition(), out ht) && ht.distance < 100)
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
        // need to add anim to movement
        /*if (nav.pathPending == false && nav.remainingDistance <= 3)
        {
            nav.ResetPath();
            nav.SetDestination(player.getPosition());
        }*/
        //nav.ResetPath();
        nav.SetDestination(player.getPosition());
        //anim.
    }

    private void attack()
    {
        //add attack funtionality when animation and stuff is done
        halo.enabled = true;
        anim.SetTrigger("Attack");
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
