using UnityEngine;
using System.Collections;

public class WaveManager : MonoBehaviour
{
    public EnemySpawner spawner;

    [Header("Wave Configuration (ตั้งค่าพื้นฐาน)")]
    [Tooltip("ระยะเวลาพักระหว่าง Wave (วินาที)")]
    public float timeBetweenWaves = 5f;
    [Tooltip("ระยะห่างระหว่างการเกิดของศัตรูแต่ละตัวใน Wave เดียวกัน")]
    public float spawnInterval = 0.5f;

    [Header("Endless Difficulty Settings (การเพิ่มความยาก)")]
    [Tooltip("จำนวนศัตรูเริ่มต้นใน Wave 1")]
    public int baseEnemyCount = 5;
    [Tooltip("จำนวนศัตรูที่จะเพิ่มขึ้นในแต่ละ Wave (เช่น ใส่ 2 คือเพิ่มทีละ 2 ตัว)")]
    public int enemyCountIncrement = 2;

    [Header("Stat Multipliers (ตัวคูณความเก่ง)")]
    [Tooltip("เปอร์เซ็นต์เลือดที่เพิ่มขึ้นต่อ Wave (เช่น 0.1 คือเพิ่ม 10%)")]
    public float hpIncreasePercentage = 0.1f;
    [Tooltip("เปอร์เซ็นต์ความเร็วที่เพิ่มขึ้นต่อ Wave (เช่น 0.05 คือเพิ่ม 5%)")]
    public float speedIncreasePercentage = 0.05f;

    private int currentWave = 0;

    void Start()
    {
        if (spawner == null)
        {
            spawner = FindFirstObjectByType<EnemySpawner>();
        }

        
        StartCoroutine(StartNextWave());
    }

    IEnumerator StartNextWave()
    {
        currentWave++;
        Debug.Log($"--- Starting Wave {currentWave} ---");

        //คำนวณจำนวนศัตรูในรอบนี้
        
        int enemiesToSpawn = baseEnemyCount + ((currentWave - 1) * enemyCountIncrement);

        // คำนวณค่าพลังที่เพิ่มขึ้น
        float currentHpMultiplier = 1f + ((currentWave - 1) * hpIncreasePercentage);
        float currentSpeedMultiplier = 1f + ((currentWave - 1) * speedIncreasePercentage);

        // เริ่มเสกศัตรูทีละตัว
        for (int i = 0; i < enemiesToSpawn; i++)
        {
            if (spawner != null)
            {
                
                GameObject enemyObj = spawner.SpawnEnemy();

                if (enemyObj != null)
                {
                    Enemy enemyScript = enemyObj.GetComponent<Enemy>();
                    if (enemyScript != null)
                    {
                        
                        // เพิ่ม HP (ปัดเศษเป็น int)
                        enemyScript.hp = Mathf.RoundToInt(enemyScript.hp * currentHpMultiplier);

                        // เพิ่ม Speed
                        enemyScript.moveSpeed *= currentSpeedMultiplier;
                    }
                }
            }
            // รอแป๊บนึงก่อนเสกตัวถัดไป 
            yield return new WaitForSeconds(spawnInterval);
        }

        //  รอจนกว่าศัตรูทั้งหมดจะตาย (Endless Condition)
        yield return new WaitUntil(() => GameObject.FindObjectsByType<Enemy>(FindObjectsSortMode.None).Length == 0);

        Debug.Log($"Wave {currentWave} Cleared!");

        //  พักก่อนขึ้น Wave ถัดไป
        yield return new WaitForSeconds(timeBetweenWaves);

        // วนลูปเริ่ม Wave ถัดไป 
        StartCoroutine(StartNextWave());
    }
}