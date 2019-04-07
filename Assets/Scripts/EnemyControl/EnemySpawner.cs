using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemySpawner : MonoBehaviour
{
    /* Arena */
    public GameObject arena;
    //private Transform[] spawnPoints;        // Spawn points for enemies
    //private Transform[] mineSpawns;         // Spawn points for mines
    private List<Transform> spawnPoints;
    private List<Transform> mineSpawns;

    /* Enemies */
    public GameObject[] enemies;            // List of enemy prefabs that can be spawned
    public float spawnTime = 5f;            // Time between enemy spawns
    public int maxEnemies = 10;             // Max amount of enemies allowed in the scene at once
    public int maxEnemiesPerWave = 20;      // Amount of enemies that will be spawned per wave
    public int currentEnemyCount;          // Amount of enemies currently in the scene
    public int totalEnemyCount;            // Count of total enemies spawned so far this wave

    /* Items */
    public GameObject[] items;              // List of item prefabs that can be spawned between waves
    public Transform itemSpawn;             // Spawn point for items
    public GameObject skip;

    /* Landmines */
    public GameObject mine;                 // Link to landmine prefab
    public int mineNumber = 1;              // How many mines are spawned per wave
    private GameObject[] currentMines;      // Links to the mines in the scene already. For right now, must be less than 10

    /* Wave stats */
    private int wave = 1;                   // Current wave count
    public int waveBonus = 5;               // Bonus to score for completing a wave
    public float timeBetweenWaves = 5f;     // Time between waves in seconds
    public Text waveCount;                  // Link to UI element describing current wave

    /* Attached Objects */
    public Light directionalLight;          // Link to directional light used at all times
    public Light spotlight;                 // Link to spotlight used during wave interim
    private PlayerController player;        // Link to player

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
        spawnPoints = new List<Transform>();
        mineSpawns = new List<Transform>();

        foreach (Transform child in arena.transform.Find("Enemy Spawns"))
        {
            spawnPoints.Add(child);
        }

        foreach (Transform child in arena.transform.Find("Landmine Spawns"))
        {
            mineSpawns.Add(child);
        }

        player = GameObject.FindGameObjectWithTag("player").GetComponent<PlayerController>();
        currentMines = new GameObject[10];
        StartWave();
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
        Debug.Log("\tSpawner: Spawned Enemy");
        int spawnPoint = Random.Range(0, spawnPoints.Count);
        int enemy = Random.Range(0, enemies.Length);
        Instantiate(enemies[enemy], spawnPoints[spawnPoint].position, spawnPoints[spawnPoint].rotation);
        currentEnemyCount++;
        totalEnemyCount++;
        Debug.Log("\tSpawner: currentEnemyCount: " + currentEnemyCount);
        Debug.Log("\tSpawner: totalEnemyCount: " + totalEnemyCount);
    }

    public void KilledEnemy()
    {
        currentEnemyCount--;
    }

    private void EndWave()
    {
        Debug.Log("\tSpawner: Ending Wave");
        CancelInvoke();

        skip.SetActive(true);
        directionalLight.intensity = 0.2f;
        spotlight.intensity = 1f;
        wave++;
        waveCount.text = "Wave: " + wave.ToString();

        player.awardPoints(waveBonus);

        waveBonus++;
        if (maxEnemies < 10)
        {
            maxEnemies++;
        }
        maxEnemiesPerWave += 2;
        if (mineNumber < 10)
        {
            mineNumber++;
        }

        // Get rid of mines
        for (int i = 0; i < mineNumber; i++)
        {
            if (currentMines[i] != null)
            {
                Destroy(currentMines[i]);
            }
        }

        // Spawn in item
        int itemNum = Random.Range(0, items.Length);
        GameObject item = Instantiate(items[itemNum], itemSpawn.position, itemSpawn.rotation);

        // If player ignores item, award bonus points
        if (item.transform.GetChild(0).GetComponent<CollectableHealth>() != null)
        {
            Debug.Log("\tSpawner: Spawned Health");
            item.transform.GetChild(0).GetComponent<CollectableHealth>().extraPoints(timeBetweenWaves);
        }
        else if (item.transform.GetChild(0).GetComponent<CollectableAmmo>() != null)
        {
            Debug.Log("\tSpawner: Spawned Ammo");
            item.transform.GetChild(0).GetComponent<CollectableAmmo>().extraPoints(timeBetweenWaves);
        }
    }

    private void StartWave()
    {
        skip.SetActive(false);
        Debug.Log("\tSpawner: Starting Wave");
        Debug.Log("\tSpawner: wave: " + wave);
        directionalLight.intensity = 1f;
        spotlight.intensity = 0f;
        currentEnemyCount = 0;
        totalEnemyCount = 0;
        state = States.Waiting;

        for (int i = 0; i < mineNumber; i++)
        {
            Debug.Log("\tSpawner: Spawned Mine");
            int mineSpawn = Random.Range(0, mineSpawns.Count);
            GameObject newMine = Instantiate(mine, mineSpawns[mineSpawn].position, mineSpawns[mineSpawn].rotation);
            currentMines[i] = newMine;
        }
    }
}
