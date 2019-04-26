using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioPlay : MonoBehaviour
{
    public AudioClip steps, swipe, airSwipe;
    public AudioSource sound;
    private void Start() {
        sound.loop = false;
    }
    
    public void footstep() {
        sound.PlayOneShot(steps, 0.2f);
    }

    public void sword() {
        sound.PlayOneShot(swipe);
    }

    public void airSword() {
        sound.PlayOneShot(airSwipe);
    }
}
