using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class BasicEnemyMovement : MonoBehaviour
{
    public PlayerController player;        // Reference to the player.
    private NavMeshAgent nav;              // Reference to the nav mesh agent.
    public int enemyHealth = 3;
    private int playerHealth;
    public Behaviour halo;
    private int cwp = -1;
    public GameObject[] waypoints;
    private NavMeshHit ht;
    //public Animator anim;
  
    //public Animator anim;
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
        playerHealth = player.getHealth();
        Debug.Log("playerHealth: " + playerHealth);
        nav = GetComponent<NavMeshAgent>();
        nav.enabled = true;
        state = States.Idle;
    }

    // Update is called once per frame
    void Update()
    {
        switch(state)
        {
            case States.Chase:
                chase_player();
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
                if(Vector3.Distance(nav.transform.position, player.getPosition()) <= 6)
                {
                    state = States.Attack;
                    break;
                }
                if (nav.Raycast(player.getPosition(), out ht))
                {
                    getWP();
                    state = States.Idle;
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
                if (!nav.Raycast(player.getPosition(), out ht) && ht.distance < 115)
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

    private void chase_player()
    {
        // need to add anim to movement
        if (nav.pathPending == false && nav.remainingDistance <= 3)
        {
            nav.ResetPath();
            nav.SetDestination(player.getPosition());
        }
        //nav.ResetPath();
        //nav.SetDestination(player.getPosition());
        //anim.
    }

    private void attack()
    {
        //add attack funtionality when animation and stuff is done
        halo.enabled = true;
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
            Destroy(this.gameObject.transform.root.gameObject);
        }
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
