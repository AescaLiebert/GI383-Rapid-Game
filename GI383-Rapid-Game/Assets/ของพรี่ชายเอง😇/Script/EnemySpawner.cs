using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public Transform player;
    public float timeBetweenSpawns = 2f;
    public float spawnDistanceMin = 10f;
    public float spawnDistanceMax = 15f;

    [Header("Enemy Prefabs")]
    public List<GameObject> enemyPrefabs;

    private float spawnTimer;
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
        if (player == null)
        {
            Player p = FindFirstObjectByType<Player>();
            if (p != null) player = p.transform;
        }

        // Auto-assign Ground layer if possible
        if (groundLayer.value == 0) // If Nothing is selected
        {
            int groundLayerIndex = LayerMask.NameToLayer("Ground");
            if (groundLayerIndex != -1)
            {
                groundLayer = 1 << groundLayerIndex;
                Debug.Log($"EnemySpawner: Auto-assigned Ground Layer to 'Ground' (Index {groundLayerIndex})");
            }
            else
            {
                Debug.LogError("EnemySpawner: Layer 'Ground' not found! Please check your Tags and Layers settings.");
            }
        }
    }

    void Update()
    {
        if (enemyPrefabs.Count == 0 || player == null) return;

        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0)
        {
            SpawnEnemy();
            spawnTimer = timeBetweenSpawns;
        }
    }

    public void SpawnEnemy()
    {
        if (enemyPrefabs.Count == 0)
        {
            Debug.LogWarning("EnemySpawner: No Enemy Prefabs assigned!");
            return;
        }
        if (player == null)
        {
            Player p = FindFirstObjectByType<Player>();
            if (p != null) player = p.transform;
            
            if (player == null)
            {
                Debug.LogError("EnemySpawner: Player not found!");
                return;
            }
        }

        // Try to get a valid spawn position
        if (TryGetSpawnPosition(out Vector2 spawnPos))
        {
             GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];
             Instantiate(prefab, spawnPos, Quaternion.identity);
        }
        else
        {
            // Debug.Log("Could not find valid spawn position (no ground found).");
        }
    }

    [Header("Ground Check Layer")]
    public LayerMask groundLayer;

    bool TryGetSpawnPosition(out Vector2 spawnPosition)
    {
        spawnPosition = Vector2.zero;
        if (mainCamera == null) mainCamera = Camera.main;

        float camHeight = 2f * mainCamera.orthographicSize;
        float camWidth = camHeight * mainCamera.aspect;
        float buffer = 5f; // Increased from 2f to spawn further out
        float minDistance = (camWidth / 2f) + buffer;
        float extraDistance = Random.Range(0, spawnDistanceMax - spawnDistanceMin); 
        float distance = minDistance + extraDistance;

        // Determine potential Left and Right positions
        float leftX = mainCamera.transform.position.x - distance;
        float rightX = mainCamera.transform.position.x + distance;

        // Check if they are valid (Raycast hits ground)
        bool leftValid = CheckGround(leftX, out float leftY);
        bool rightValid = CheckGround(rightX, out float rightY);

        if (leftValid && rightValid)
        {
            // Both valid, pick random
            if (Random.value > 0.5f)
                spawnPosition = new Vector2(leftX, leftY);
            else
                spawnPosition = new Vector2(rightX, rightY);
            return true;
        }
        else if (leftValid)
        {
            spawnPosition = new Vector2(leftX, leftY);
            return true;
        }
        else if (rightValid)
        {
            spawnPosition = new Vector2(rightX, rightY);
            return true;
        }

        return false;
    }

    bool CheckGround(float xPos, out float yPos)
    {
        yPos = 0;
        
        // Start ray from above the top of the camera view to ensure we catch high platforms
        float rayStartY = mainCamera.transform.position.y + mainCamera.orthographicSize + 5f; 
        
        Vector2 rayOrigin = new Vector2(xPos, rayStartY);
        
        // Visualize the ray
        Debug.DrawRay(rayOrigin, Vector2.down * 50f, Color.red, 1f);

        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, 50f, groundLayer);

        if (hit.collider != null)
        {
            // NEW CHECK: Prevent spawning on high invisible walls or way above player
            // Only allow spawn if the ground is within reasonable Y range of the player
            float yDiff = Mathf.Abs(hit.point.y - player.position.y);
            if (yDiff > 5f) // Adjust this threshold based on your game's verticality
            {
               Debug.Log($"Ignored spawn at X:{xPos} because it is too high/low ({yDiff} units) relative to player.");
                return false;
            }

            yPos = hit.point.y;
            return true;
        }
        return false;
    }
}
