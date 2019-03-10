using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BufferAI : MonoBehaviour
{
    /* Linked objects */
    private PlayerController player;        // Reference to the player.
    public Behaviour halo;
    private Animator anim;
    private GameObject[] allies;
    private GameObject closest;

    /* Stats */
    public int maxEnemyHealth = 1;          // Maximum allowed health
    private int enemyHealth = 1;            // Current health
    public float setBuffTime = 10;          // Time between buffs
    private float buffTime;                 // Storage for time since last buff

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
        allies = GameObject.FindGameObjectsWithTag("brute");
        anim = GetComponent<Animator>();
        state = States.Idle;


    }

    // Update is called once per frame
    void Update()
    {
        allies = GameObject.FindGameObjectsWithTag("brute");
        switch (state)
        {
            case States.PDead:
                halo.enabled = false;
                //player is dead
                break;

            case States.Dead:
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

                if (allies.Length > 0)
                {
                    //Debug.Log(allies.Length);
                    getClosestAlly();
                    if (closest.activeSelf && closest.GetComponent<BasicEnemyMovement>().getHealth() > 0 && buffTime >= setBuffTime)
                    {
                        buffTime = 0;
                        state = States.Shield;
                        break;
                    }
                }
                  
                break;

            case States.Shield:
                //anim.SetTrigger("shield");
                if (closest.activeSelf)
                {
                    Debug.Log("Buffer: Shielded Ally");
                    halo.enabled = true;
                    closest.GetComponent<BasicEnemyMovement>().fullHealth();
                }

                if (enemyHealth == 0)
                {
                    state = States.Dead;
                    break;
                }
                if (player.getHealth() == 0)
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
        buffTime += 1 * Time.deltaTime;
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
}
