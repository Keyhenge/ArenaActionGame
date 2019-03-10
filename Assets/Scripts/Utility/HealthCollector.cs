using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class HealthCollector : MonoBehaviour
{
    private PlayerController player;

    private void Start()
    {
        player = GetComponent<PlayerController>();
    }

    public void ReceiveHealth()
    {
        player.ResetHealth();
    }
    public void ReceiveAmmo()
    {
        player.ResetAmmo();
    }
}
