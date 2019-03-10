using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BufferAI : MonoBehaviour
{
    private PlayerController player;        // Reference to the player.
    private NavMeshAgent nav;              // Reference to the nav mesh agent.
    public GameObject aoe;
    public int enemyHealth = 1;
    private int playerHealth;
    public Behaviour halo;
    private int cwp = -1;
    public GameObject[] waypoints;
    private NavMeshHit ht;
    private Animator anim;
    private float attackTime = 10;
    private GameObject[] allies;
    private GameObject closest;
  
    public enum States
    {
        Shield,
        PDead,
        Dead,
        Idle
    };
    public States state;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("player").GetComponent<PlayerController>();
        allies = GameObject.FindGameObjectsWithTag("enemy");
        nav = this.transform.root.GetComponent<NavMeshAgent>();
        nav.enabled = true;
        anim = GetComponent<Animator>();
        state = States.Idle;
    }

    // Update is called once per frame
    void Update()
    {
        playerHealth = player.getHealth();
        //Debug.Log("playerHealth: " + playerHealth);

        switch (state)
        {

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
                allies = GameObject.FindGameObjectsWithTag("enemy");
                if (enemyHealth == 0)
                {
                    state = States.Dead;
                    break;
                }
                getClosestAlly();
                if (closest.activeSelf && closest.GetComponent<BasicEnemyMovement>().getHealth() > 0 && attackTime <= 0) 
                {
                    state = States.Shield;
                    break;
                }

                if (nav.pathPending == false && nav.remainingDistance <= 2)
                {
                    setNextWaypoint();
                }
                  
                break;

            case States.Shield:
                //anim.SetTrigger("shield");
                if (closest.activeSelf)
                {
                    closest.GetComponent<BasicEnemyMovement>().doubleHealth();
                    attackTime = 10;
                }
                //closest.GetComponent<BasicEnemyMovement>().doubleHealth();
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
                state = States.Idle;
                break;

            default:
                halo.enabled = false;
                state = States.Idle;
                break;
        }

    }

    private void FixedUpdate()
    {
        attackTime -= 1 * Time.deltaTime;
    }

    private void getClosestAlly()
    {
        closest = allies[0];
        for (int i = 0; i < allies.Length; i++)
        {
            if (closest.GetComponent<BasicEnemyMovement>().getHealth() > allies[i].GetComponent<BasicEnemyMovement>().getHealth())
            {
                closest = allies[i];
            }
        }
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
