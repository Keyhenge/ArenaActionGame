using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject[] enemies;
    public float spawnTime = 5f;
    public int maxEnemies = 10;
    public Transform[] spawnPoints;
    public bool spawning;
    public int maxEnemiesPerWave = 20;
    public int currentEnemyCount;
    public int totalEnemyCount;

    public Light directionalLight;
    public Light spotlight;

    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating("Spawn", 1f, spawnTime);
        spawning = true;
        currentEnemyCount = 0;
        totalEnemyCount = 0;
        directionalLight.intensity = 1f;
        spotlight.intensity = 0f;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (totalEnemyCount >= maxEnemiesPerWave && currentEnemyCount == 0)
        {
            EndWave();
            Invoke(nameof(StartWave), 1f);
        }
        else if (!spawning && currentEnemyCount < maxEnemies)
        {
            InvokeRepeating(nameof(Spawn), 1f, spawnTime);
            spawning = true;
        }
        else if ((spawning && currentEnemyCount >= maxEnemies) || totalEnemyCount >= maxEnemiesPerWave)
        {
            CancelInvoke();
            spawning = false;
        }
    }

    void Spawn ()
    {
        int spawnPoint = Random.Range(0, spawnPoints.Length);
        int enemy = Random.Range(0, enemies.Length);
        Instantiate(enemies[enemy], spawnPoints[spawnPoint].position, spawnPoints[spawnPoint].rotation);
        currentEnemyCount++;
        totalEnemyCount++;
    }

    public void KilledEnemy()
    {
        currentEnemyCount--;
    }

    private void EndWave()
    {
        Debug.Log("Ending Wave");
        directionalLight.intensity = 0.2f;
        spotlight.intensity = 1f;
    }

    private void StartWave()
    {
        Debug.Log("Starting Wave");
        directionalLight.intensity = 1f;
        spotlight.intensity = 0f;
        currentEnemyCount = 0;
        totalEnemyCount = 0;
    }
}
