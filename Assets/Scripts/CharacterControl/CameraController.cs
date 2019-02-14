﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    // Object attributes
    private Camera cam;
    public GameObject player;
    private Vector3 offset;
    public Material mat;

    // Camera movement
    public float sensitivity = 10f;
    private float distanceToPlayer;
    private float curX;
    private float curY;
    public float Y_MIN_ANGLE = 10f;
    public float Y_MAX_ANGLE = 80f;

    // Transparency
    public Material TransparentMaterial = null;
    public float FadeInTimeout = 0.6f;
    public float FadeOutTimeout = 0.2f;
    public float TargetTransparency = 0.3f;

    // Camera Shake
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
        offset = transform.position - player.transform.position;
        distanceToPlayer = offset.magnitude;
        cam = Camera.main;
        //camTransform = Camera.main.transform;
    }

    private void Update()
    {
        // Camera Movement
        // Borrowed from https://www.youtube.com/watch?v=Ta7v27yySKs
        //transform.position = player.transform.position + offset;
        curX += Input.GetAxis("Mouse X");
        curY -= Input.GetAxis("Mouse Y");
        curY = Mathf.Clamp(curY, Y_MIN_ANGLE, Y_MAX_ANGLE);

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
        camTransform.position = Vector3.up * 10f + player.transform.position + Quaternion.Euler(curY, curX, 0) * (new Vector3(0, 0, -distanceToPlayer));
        camTransform.LookAt(player.transform.position);
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
