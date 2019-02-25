using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class BasicEnemyMovement : MonoBehaviour
{
    

    public PlayerController player;        // Reference to the player.
    private NavMeshAgent nav;              // Reference to the nav mesh agent.
    public int enemyHealth = 1;
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
                }
                if (playerHealth == 0)
                {
                    state = States.PDead;
                }
                if(Vector3.Distance(nav.transform.position, GameObject.FindGameObjectWithTag("Player").transform.position) <= 6)
                {
                    state = States.Attack;
                }
                break;

            case States.Attack:
                if (playerHealth == 0)
                {
                    state = States.PDead;
                } 
                if (enemyHealth == 0)
                {
                    state = States.Dead;
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
}
