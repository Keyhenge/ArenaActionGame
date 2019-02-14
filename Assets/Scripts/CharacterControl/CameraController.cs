using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject player;
    private Vector3 offset;
    private float distanceToPlayer;
    public Material mat;

    public Material TransparentMaterial = null;
    public float FadeInTimeout = 0.6f;
    public float FadeOutTimeout = 0.2f;
    public float TargetTransparency = 0.3f;

    private float shakeTime = 0f;
    private float shakeAmountInit = 0f;
    private float shakeAmount = 0f;
    public float shakeDecrease = 2f;
    Vector3 originalPos;

    public Transform camTransform;

    //private Transform obstruction;

    //Keeps track of obstacles that had been faded out so that they can fade back in
    private List<GameObject> obstacleList;

    void Awake()
    {
        if (camTransform == null)
        {
            camTransform = GetComponent(typeof(Transform)) as Transform;
        }
    }

    // Start is called before the first frame update
    private void Start()
    {
        //obstruction = player.transform;
        offset = transform.position - player.transform.position;
        distanceToPlayer = offset.magnitude;
    }

    private void Update()
    {
        transform.position = player.transform.position + offset;
        /*// Borrowed from https://answers.unity.com/questions/44815/make-object-transparent-when-between-camera-and-pl.html
        // Makes objects between the camera and player transparent
        RaycastHit[] hits; // you can also use CapsuleCastAll() 
                           // TODO: setup your layermask it improve performance and filter your hits. 
        hits = Physics.RaycastAll(transform.position, transform.forward, distanceToPlayer - 1f);
        foreach (RaycastHit hit in hits)
        {
            Renderer R = hit.collider.GetComponent<Renderer>();
            if (R == null)
            {
                continue;
            }
            // no renderer attached? go to next hit 
            // TODO: maybe implement here a check for GOs that should not be affected like the player
            // NOTE: The reason it's functioning properly right now is because player rendering is set to opaque
            AutoTransparent AT = R.GetComponent<AutoTransparent>();
            if (AT == null) // if no script is attached, attach one
            {
                AT = R.gameObject.AddComponent<AutoTransparent>();
                AT.TransparentMaterial = TransparentMaterial;
                AT.FadeInTimeout = FadeInTimeout;
                AT.FadeOutTimeout = FadeOutTimeout;
                AT.TargetTransparency = TargetTransparency;
            }
            AT.BeTransparent(); // get called every frame to reset the falloff
        }

        originalPos = player.transform.position + offset;
        // Borrowed from https://gist.github.com/ftvs/5822103
        // Shakes camera for a period of time
        if (shakeTime > 0)
        {
            Vector3 shakePos = originalPos + Random.insideUnitSphere * shakeAmount;
            transform.position = shakePos;
            shakeTime -= Time.deltaTime * shakeDecrease;
            shakeAmount = shakeAmountInit * Mathf.Clamp(1f - shakeTime, 0, 1f) / 1f;
        }
        else
        {
            shakeTime = 0f;
            transform.position = originalPos;
        }*/
    }

    private void LateUpdate()
    {
        //addToObstacles();
    }

    public void shake(float amount)
    {
        //Debug.Log("Happening 2");
        shakeAmountInit = amount;
        shakeTime = 1f;
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(source, destination, mat);
    }
}
