using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class WaveData
{
    public string waveName;
    public int enemyCount;
    
    [Header("Spawn Settings")]
    public float minSpawnInterval = 1f;
    public float maxSpawnInterval = 3f;
    
    public int minSpawnCount = 1;
    public int maxSpawnCount = 2; // Adjust based on difficulty, prevent spawning too many at once

    [Header("Win Condition")]
    [Tooltip("If > 0, wave ends after this time. If 0, wave ends when all enemies are dead.")]
    public float waveDuration = 0f;

    public List<GameObject> waveSpecificPrefabs; 
}

public class WaveManager : MonoBehaviour
{
    public EnemySpawner spawner;
    public List<WaveData> waves;
    public float timeBetweenWaves = 5f;

    private int currentWaveIndex = 0;
    private int spawnedInCurrentWave = 0;

    void Start()
    {
        if (spawner == null)
        {
            spawner = FindFirstObjectByType<EnemySpawner>();
        }
        
        if (spawner != null)
        {
            spawner.enabled = false; 
        }

        StartCoroutine(StartNextWave());
    }

    IEnumerator StartNextWave()
    {
        if (currentWaveIndex >= waves.Count)
        {
            Debug.Log("All waves complete!");
            yield break;
        }

        Debug.Log($"Starting Wave {currentWaveIndex + 1}: {waves[currentWaveIndex].waveName}");
        spawnedInCurrentWave = 0;
        
        yield return StartCoroutine(RunWave(waves[currentWaveIndex]));
    }

    IEnumerator RunWave(WaveData wave)
    {
        float waveTimer = 0f;
        bool useTimer = wave.waveDuration > 0;

        // While loop condition:
        // 1. We still have enemies to spawn
        // 2. OR (if time based) time is not up
        while (spawnedInCurrentWave < wave.enemyCount)
        {
            if (useTimer && waveTimer >= wave.waveDuration)
            {
                Debug.Log("Wave Time Limit Reached! Stopping spawns.");
                break;
            }

            // Calculate random batch size
            int batchSize = Random.Range(wave.minSpawnCount, wave.maxSpawnCount + 1);
            int remaining = wave.enemyCount - spawnedInCurrentWave;
            if (batchSize > remaining) batchSize = remaining;

            // Spawn the batch
            for (int i = 0; i < batchSize; i++)
            {
                if (spawner != null)
                {
                    spawner.SpawnEnemy(); 
                }
                spawnedInCurrentWave++;
                yield return new WaitForSeconds(0.2f); 
                waveTimer += 0.2f; // Increment timer during batch delay
            }

            // Wait for random interval before next batch
            float waitTime = Random.Range(wave.minSpawnInterval, wave.maxSpawnInterval);
            
            // Wait while updating timer
            float elapsed = 0f;
            while (elapsed < waitTime)
            {
                elapsed += Time.deltaTime;
                waveTimer += Time.deltaTime;
                
                if (useTimer && waveTimer >= wave.waveDuration) break;

                yield return null;
            }
        }

        // Completion Condition:
        if (useTimer)
        {
             // Wait until duration finishes OR all enemies dead?
             // User asked: "Wave passed from Time Limit"
             // usually means: survive until time is up. 
             // If time is up, wave ends immediately (even if enemies alive? or just stop spawning?)
             // Let's assume: Time Limit = Stop Spawning.
             // Then usually we wait for timer to fully complete if we finished spawning early.
             
             while (waveTimer < wave.waveDuration)
             {
                 waveTimer += Time.deltaTime;
                 yield return null;
             }
             Debug.Log("Wave Completed by Time Limit.");
        }
        else
        {
             // Classic Mode: Wait for all enemies to die
             yield return new WaitUntil(() => GameObject.FindObjectsByType<Enemy>(FindObjectsSortMode.None).Length == 0);
             Debug.Log("Wave Completed by Clearing Enemies.");
        }
        
        currentWaveIndex++;
        yield return new WaitForSeconds(timeBetweenWaves);
        StartCoroutine(StartNextWave());
    }
}
