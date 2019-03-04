using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class BasicEnemyMovement : MonoBehaviour
{
    public PlayerController player;        // Reference to the player.
    private NavMeshAgent nav;              // Reference to the nav mesh agent.
    public int enemyHealth = 10;
    private int playerHealth;
    public Behaviour halo;
    //public Animator anim;
  
    //public Animator anim;
    public enum States
    {
        Chase,
        Attack,
        PDead,
        Dead
    };
    public States state;

    void Awake()
    {
        playerHealth = player.getHealth();
        nav = GetComponent<NavMeshAgent>();
        nav.enabled = true;
        state = States.Chase;
    }

    // Update is called once per frame
    void Update()
    {
        switch(state)
        {
            case States.Chase:
                chase_player();
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
                if(Vector3.Distance(nav.transform.position, player.getPosition()) <= 6)
                {
                    state = States.Attack;
                    break;
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

            default:
                halo.enabled = false;
                state = States.Chase;
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

    public void hit()
    {
        enemyHealth -= 1;

        if (enemyHealth <= 0)
        {
            Destroy(this.gameObject);
        }
    }
}
