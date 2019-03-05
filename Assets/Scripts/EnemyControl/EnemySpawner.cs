using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemySpawner : MonoBehaviour
{
    public GameObject[] enemies;
    public float spawnTime = 5f;
    public int maxEnemies = 10;
    public Transform[] spawnPoints;
    public int maxEnemiesPerWave = 20;
    public int currentEnemyCount;
    public int totalEnemyCount;
    public int wave;

    public Light directionalLight;
    public Light spotlight;

    public Text waveCount;

    public enum States
    {
        Spawning,
        Waiting,
        WaveEnd
    };
    public States state;

    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating("Spawn", 1f, spawnTime);
        state = States.Spawning;
        currentEnemyCount = 0;
        totalEnemyCount = 0;
        directionalLight.intensity = 1f;
        spotlight.intensity = 0f;
        wave = 1;
        waveCount.text = "Wave: " + wave.ToString();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!(state == States.WaveEnd) && totalEnemyCount >= maxEnemiesPerWave && currentEnemyCount == 0)
        {
            state = States.WaveEnd;
            EndWave();
            Invoke(nameof(StartWave), 1f);
        }
        else if (state == States.Waiting && currentEnemyCount < maxEnemies)
        {
            InvokeRepeating(nameof(Spawn), 1f, spawnTime);
            state = States.Spawning;
        }
        else if ((state == States.Spawning && currentEnemyCount >= maxEnemies) || (!(state == States.WaveEnd) && totalEnemyCount >= maxEnemiesPerWave))
        {
            CancelInvoke();
            state = States.Waiting;
        }
    }

    void Spawn ()
    {
        Debug.Log("Spawner: Spawned Enemy");
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
        Debug.Log("Spawner: Ending Wave");
        directionalLight.intensity = 0.2f;
        spotlight.intensity = 1f;
        wave++;
        waveCount.text = "Wave: " + wave.ToString();
    }

    private void StartWave()
    {
        Debug.Log("Spawner: Starting Wave");
        directionalLight.intensity = 1f;
        spotlight.intensity = 0f;
        currentEnemyCount = 0;
        totalEnemyCount = 0;
        state = States.Waiting;
    }
}
