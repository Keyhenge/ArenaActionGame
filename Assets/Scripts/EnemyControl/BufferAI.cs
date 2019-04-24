using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class BufferAI : MonoBehaviour
{
    /* Linked objects */
    private PlayerController player;        // Reference to the player.
    public Behaviour halo;
    private Animator anim;
    private List<GameObject> allies;
    private GameObject closest;
    public ParticleSystem particleBuffing;
    public Slider healthBar;

    /* Stats */
    public int maxEnemyHealth = 1;          // Maximum allowed health
    private int enemyHealth = 1;            // Current health
    public float setBuffTime = 10;          // Time between buffs
    private float buffTime;                 // Storage for time since last buff

    //material stuff
    public Material shieldM;
    public Material idleM;
    public Material deadM;
    
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
        anim = GetComponent<Animator>();
        state = States.Idle;
        allies = new List<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        healthBar.value = (float)enemyHealth / (float)maxEnemyHealth;

        GameObject[] brutes = GameObject.FindGameObjectsWithTag("brute");
        for (int i = 0; i < brutes.Length; i++)
        {
            allies.Add(brutes[i]);
        }

        GameObject[] flyers = GameObject.FindGameObjectsWithTag("flyer");
        for (int i = 0; i < flyers.Length; i++)
        {
            allies.Add(flyers[i]);
        }

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
                if (!particleBuffing.isPlaying)
                {
                    particleBuffing.Play();
                }

                if (enemyHealth == 0)
                {
                    state = States.Dead;
                    break;
                }

                if (allies.Count > 0)
                {
                    //Debug.Log(allies.Length);
                    getClosestAlly();
                    if (closest.activeSelf && buffTime >= setBuffTime)
                    {
                        buffTime = 0;
                        state = States.Shield;
                        break;
                    }
                }
                  
                break;

            case States.Shield:
                //anim.SetTrigger("shield");
                halo.enabled = true;

                if (closest.activeSelf)
                {
                    Debug.Log("Buffer: Shielded Ally");
                    particleBuffing.Stop();
                    if (closest.GetComponent<BasicEnemyMovement>() != null)
                    {
                        closest.GetComponent<BasicEnemyMovement>().fullHealth();
                    }
                    else if (closest.GetComponent<FlyerAI>() != null)
                    {
                        closest.GetComponent<FlyerAI>().fullHealth();
                    }
                    
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

        allies.Clear();
    }

    private void FixedUpdate()
    {
        buffTime += 1 * Time.deltaTime;
    }

    private void getClosestAlly()
    {
        closest = allies[0];
        for (int i = 0; i < allies.Count; i++)
        {
            if (closest.GetComponent<BasicEnemyMovement>() != null)
            {
                if (allies[i].GetComponent<BasicEnemyMovement>() != null)
                {
                    if (closest.GetComponent<BasicEnemyMovement>().getHealth() > allies[i].GetComponent<BasicEnemyMovement>().getHealth())
                    {
                        closest = allies[i];
                    }
                }
                else if (allies[i].GetComponent<FlyerAI>() != null)
                {
                    if (closest.GetComponent<BasicEnemyMovement>().getHealth() > allies[i].GetComponent<FlyerAI>().getHealth())
                    {
                        closest = allies[i];
                    }
                }
            }
            else if (closest.GetComponent<FlyerAI>() != null)
            {
                if (allies[i].GetComponent<BasicEnemyMovement>() != null)
                {
                    if (closest.GetComponent<FlyerAI>().getHealth() > allies[i].GetComponent<BasicEnemyMovement>().getHealth())
                    {
                        closest = allies[i];
                    }
                }
                else if (allies[i].GetComponent<FlyerAI>() != null)
                {
                    if (closest.GetComponent<FlyerAI>().getHealth() > allies[i].GetComponent<FlyerAI>().getHealth())
                    {
                        closest = allies[i];
                    }
                }
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
    public void Hit(int damage)
    {
        enemyHealth -= damage;

        if (enemyHealth <= 0)
        {
            Destroy(this.transform.root.gameObject);
        }
    }
}
