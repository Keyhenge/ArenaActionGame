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
    private GameObject brute;
  
    //public Animator anim;

    void Awake()
    {
        // Set up the references.
        //player = GameObject.FindGameObjectWithTag("Player").transform;
        playerHealth = player.getHealth();
        nav = GetComponent<NavMeshAgent>();
        nav.enabled = true;
        brute = GameObject.FindGameObjectWithTag("brute");
    }

    // Update is called once per frame
    void Update()
    {
        // If the enemy and the player have health left...
        if (enemyHealth > 0 && playerHealth > 0)
        {
            // ... set the destination of the nav mesh agent to the player.
            //nav.SetDestination(player.getPosition());
            nav.destination = player.getPosition();
            if (Vector3.Distance(nav.transform.position, GameObject.FindGameObjectWithTag("Player").transform.position) <= 6)
            {
                halo.enabled = true;
            }
        }
        // Otherwise...
        else
        {
            // ... disable the nav mesh agent.
            nav.enabled = false;
        }
    }
}
