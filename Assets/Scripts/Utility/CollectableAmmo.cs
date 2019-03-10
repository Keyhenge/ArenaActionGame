using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectableAmmo : MonoBehaviour
{
    private PlayerController player;
    private Animator anim;
    public int ignoreItemBonus = 3;

    void Awake()
    {
        anim = GetComponent<Animator>();
        if (anim == null)
            Debug.Log("Animator could not be found");

        player = GameObject.FindGameObjectWithTag("player").GetComponent<PlayerController>();
    }

    void OnTriggerEnter(Collider c)
    {
        if (c.attachedRigidbody != null && c.transform.root.gameObject.tag == "player")
        {
            HealthCollector hc = c.transform.root.gameObject.GetComponent<HealthCollector>();
            if (hc != null)
            {
                hc.ReceiveAmmo();
                anim.SetTrigger("pickedUp");
                Destroy(this.transform.root.gameObject, this.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length);
                //EventManager.TriggerEvent<BombBounceEvent, Vector3>(c.transform.position);
            }
        }
    }

    public void extraPoints(float timeBetweenWaves)
    {
        Invoke(nameof(DestroyForPoints), timeBetweenWaves);
    }

    private void DestroyForPoints()
    {
        player.awardPoints(ignoreItemBonus);
        Destroy(this.transform.root.gameObject);
    }
    
}
