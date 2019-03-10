using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemySpawner : MonoBehaviour
{
    public GameObject[] enemies;            // List of enemy prefabs that can be spawned
    public GameObject[] items;              // List of item prefabs that can be spawned between waves
    public float spawnTime = 5f;            // Time between spawns
    public int maxEnemies = 10;             // Max amount of enemies allowed on screen at once
    public Transform[] spawnPoints;         // List of spawn points
    public Transform itemSpawn;             // Spawn point for items
    public int maxEnemiesPerWave = 20;      // Amount of enemies that will be spawned per wave
    private int currentEnemyCount;          // Amount of enemies currently on screen
    private int totalEnemyCount;            // Count of total enemies spawned so far this wave
    private int wave;                       // Current wave count
    public int waveBonus = 3;               // Bonus to score for completing a wave
    public float timeBetweenWaves = 5f;     // Time between waves in seconds

    public Light directionalLight;          // Link to directional light used at all times
    public Light spotlight;                 // Link to spotlight used during wave interim

    public Text waveCount;                  // Link to UI element describing current wave
    private PlayerController player;        //

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

        player = GameObject.FindGameObjectWithTag("player").GetComponent<PlayerController>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!(state == States.WaveEnd) && totalEnemyCount >= maxEnemiesPerWave && currentEnemyCount == 0)
        {
            state = States.WaveEnd;
            EndWave();
            Invoke(nameof(StartWave), timeBetweenWaves);
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

        player.awardPoints(waveBonus);

        // Spawn in item
        int itemNum = Random.Range(0, items.Length);
        GameObject item = Instantiate(items[itemNum], itemSpawn.position, itemSpawn.rotation);

        // If player ignores item, award bonus points
        if (item.transform.GetChild(0).GetComponent<CollectableHealth>() != null)
        {
            Debug.Log("Spawner: Spawned Health");
            item.transform.GetChild(0).GetComponent<CollectableHealth>().extraPoints(timeBetweenWaves);
        }
        else if (item.transform.GetChild(0).GetComponent<CollectableAmmo>() != null)
        {
            Debug.Log("Spawner: Spawned Health");
            item.transform.GetChild(0).GetComponent<CollectableAmmo>().extraPoints(timeBetweenWaves);
        }
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
