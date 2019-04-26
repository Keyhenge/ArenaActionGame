using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioPlay : MonoBehaviour
{
    public AudioClip steps, swipe, airSwipe, rifle;
    public AudioSource sound;
    private void Start() {
        sound.loop = false;
    }
    
    public void footstep() {
        sound.PlayOneShot(steps, 0.4f);
    }

    public void sword() {
        sound.PlayOneShot(swipe);
    }

    public void airSword() {
        sound.PlayOneShot(airSwipe);
    }

    public void shot() {
        sound.PlayOneShot(rifle);
    }
}
