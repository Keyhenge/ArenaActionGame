using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplodableMine : MonoBehaviour
{
    public Animator anim;

    void Awake()
    {
        //player = GameObject.FindGameObjectWithTag("player").GetComponent<PlayerController>();
    }

    private void Update()
    {
        AnimatorStateInfo animation = anim.GetCurrentAnimatorStateInfo(0);
        if (animation.IsName("Explosion"))
        {
            Destroy(this.transform.root.gameObject, animation.length);
        }
    }

    void OnTriggerEnter(Collider c)
    {
        if (c.attachedRigidbody != null && c.transform.root.gameObject.tag == "player")
        {
            anim.SetTrigger("click");
            //EventManager.TriggerEvent<BombBounceEvent, Vector3>(c.transform.position);
        }
    }
}
