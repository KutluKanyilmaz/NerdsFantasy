using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaveSpawner : MonoBehaviour
{
    [System.Serializable]
    public class Wave
    {
        public GameObject enemyPrefab;
        public int count = 10;
        public float rate = 1f; // Spawns per second
    }

    [Header("Spawn Settings")]
    public float spawnRadius = 20f; // Distance from center
    public Transform centerPoint;   // Usually the player (or world center 0,0,0)

    [Header("Waves")]
    public List<Wave> waves;
    public float timeBetweenWaves = 5f;

    int currentWaveIndex = 0;
    float waveCountdown;
    enum SpawnState { Spawning, Waiting, Counting, Finished }
    SpawnState state = SpawnState.Counting;

    void Start()
    {
        waveCountdown = timeBetweenWaves;
        if (centerPoint == null) centerPoint = transform; // Default to this object's position
    }

    void Update()
    {
        if (state == SpawnState.Finished) return;

        // 1. Check if we need to start the next wave
        if (state == SpawnState.Counting)
        {
            waveCountdown -= Time.deltaTime;
            if (waveCountdown <= 0f)
            {
                StartCoroutine(SpawnWave(waves[currentWaveIndex]));
            }
            return; // Don't do anything else while counting down
        }

        // 2. Check if the wave is cleared
        if (state == SpawnState.Waiting)
        {
            // Simple check: Are there any enemies left alive?
            // Note: This can be expensive if you have 1000s of objects. 
            // Better to track count in a manager, but this works for MVP.
            if (!EnemyIsAlive())
            {
                WaveCompleted();
            }
        }
    }

    IEnumerator SpawnWave(Wave _wave)
    {
        state = SpawnState.Spawning;
        Debug.Log("Spawning Wave");

        for (int i = 0; i < _wave.count; i++)
        {
            SpawnEnemy(_wave.enemyPrefab);
            yield return new WaitForSeconds(1f / _wave.rate);
        }

        state = SpawnState.Waiting;
    }

    void SpawnEnemy(GameObject _enemy)
    {
        // === The Circular Logic ===
        // 1. Pick a random angle (in radians)
        float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;

        // 2. Calculate position using Sine and Cosine
        // x = cx + r * cos(a)
        // z = cz + r * sin(a)
        Vector3 spawnPos = new Vector3(
            centerPoint.position.x + spawnRadius * Mathf.Cos(randomAngle),
            centerPoint.position.y,
            centerPoint.position.z + spawnRadius * Mathf.Sin(randomAngle)
        );

        PoolManager.Instance.Spawn("Enemy", spawnPos, Quaternion.identity);
    }

    void WaveCompleted()
    {
        Debug.Log("Wave Completed!");
        state = SpawnState.Counting;
        waveCountdown = timeBetweenWaves;

        if (currentWaveIndex + 1 < waves.Count)
        {
            currentWaveIndex++;
        }
        else
        {
            Debug.Log("ALL WAVES COMPLETE! LOoping...");
            currentWaveIndex = 0; // Loop back for endless testing
            // state = SpawnState.Finished; // Or use this to stop
        }
    }

    bool EnemyIsAlive()
    {
        // This finds objects with the "Enemy" tag. 
        // MAKE SURE YOUR ENEMY PREFAB IS TAGGED "Enemy"!
        if (GameObject.FindGameObjectsWithTag("Enemy").Length == 0)
        {
            return false;
        }
        return true;
    }

    // Visualizing the Spawn Circle in the Editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        if (centerPoint != null)
        {
            // Draw the spawn circle
            Gizmos.DrawWireSphere(centerPoint.position, spawnRadius);
        }
    }
}