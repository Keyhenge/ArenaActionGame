using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Object Attributes")]
    public GameObject player;
    public Material mat;
    private Camera cam;

    [Space(10)]

    [Header("Camera Movement")]
    public float xsen = 1.5f;
    public float ysen = 1f;
    public float distanceToPlayer = 20f;
    private float offset;
    public float offsetControl = 40f;
    public float maxDistance = 30f;
    public float minDistance = -10f;
    private float curX;
    private float curY;
    public float Y_MIN_ANGLE = -25f;
    public float Y_MAX_ANGLE = 45f;

    [Space(10)]

    [Header("Transparency")]
    public Material TransparentMaterial = null;
    public float FadeInTimeout = 0.6f;
    public float FadeOutTimeout = 0.2f;
    public float TargetTransparency = 0.3f;

    [Space(10)]

    [Header("Camera Shake")]
    public float shakeDecrease = 2f;
    private float shakeTime = 0f;
    private float shakeAmountInit = 0f;
    private float shakeAmount = 0f;
    private Vector3 originalPos;

    private Transform camTransform;

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
        offset = 0f;
        cam = Camera.main;
    }

    private void Update()
    {
        // Camera Movement
        // Borrowed from https://www.youtube.com/watch?v=Ta7v27yySKs
        //transform.position = player.transform.position + offset;
        curX += Input.GetAxis("Mouse X") * xsen;
        curY -= Input.GetAxis("Mouse Y") * ysen;
        curY = Mathf.Clamp(curY, Y_MIN_ANGLE, Y_MAX_ANGLE);
        offset = (distanceToPlayer * curY)/offsetControl;
        offset = Mathf.Clamp(offset, minDistance, maxDistance);

        // Transparency
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

        // Camera Shake
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
        camTransform.position = Vector3.up * 5f + player.transform.position + Quaternion.Euler(curY, curX, 0) * (new Vector3(0, 0, -distanceToPlayer - offset));
        camTransform.LookAt(Vector3.up * 5f + player.transform.position);
    }

    public void shake(float amount)
    {
        shakeAmountInit = amount;
        shakeTime = 1f;
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(source, destination, mat);
    }

    public Camera getCamera()
    {
        return cam;
    }
}
