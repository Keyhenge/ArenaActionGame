using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyerProjectile : MonoBehaviour
{
    public Animator anim;
    private Rigidbody rbody;

    private void Start()
    {
        rbody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Explosion"))
        {
            rbody.velocity = this.transform.forward * 0;
            Destroy(this.transform.root.gameObject, anim.GetCurrentAnimatorStateInfo(0).length - 0.05f);
        }
        else
        {
            //this.transform.position = this.transform.position + this.transform.forward * 0.1f;
            rbody.velocity = this.transform.forward * 25;
        }
    }

    /* Collisions */
    private void OnTriggerEnter(Collider c)
    {
        anim.SetTrigger("click");
    }

    private void OnTriggerStay(Collider c)
    {
        anim.SetTrigger("click");
    }
}
