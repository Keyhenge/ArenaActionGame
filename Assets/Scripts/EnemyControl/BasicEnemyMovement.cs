using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class BasicEnemyMovement : MonoBehaviour
{
    /*
    Transform player;               // Reference to the player's position.
    PlayerHealth playerHealth;      // Reference to the player's health.
    EnemyHealth enemyHealth;        // Reference to this enemy's health.
    NavMeshAgent nav;               // Reference to the nav mesh agent.
    */

    public PlayerController player;        // Reference to the player.
    private NavMeshAgent nav;              // Reference to the nav mesh agent.
    public int enemyHealth = 1;
    private int playerHealth;

    void Awake()
    {
        // Set up the references.
        //NEED TO: create nav mesh, once player and enemy health and etc r created add reference

        //player = GameObject.FindGameObjectWithTag("Player").transform;
        playerHealth = player.getHealth();
        nav = GetComponent<NavMeshAgent>();
        nav.enabled = true;
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
        }
        // Otherwise...
        else
        {
            // ... disable the nav mesh agent.
            nav.enabled = false;
        }
    }
}
