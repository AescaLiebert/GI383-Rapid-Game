using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public enum ItemType { Health, XP }
    
    public ItemType type;
    public int amount = 10;
    
    [Header("Visuals")]
    // Optional: add float bobbing or rotation logic here
    public float bobSpeed = 2f;
    public float bobHeight = 0.5f;
    
    private Vector3 startPos;
    
    void Start()
    {
        startPos = transform.position;
    }
    
    void Update()
    {
        // Simple visual bobbing
        float newY = startPos.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerStats stats = other.GetComponent<PlayerStats>();
            if (stats != null)
            {
                ApplyEffect(stats);
                Destroy(gameObject);
            }
        }
    }

    private void ApplyEffect(PlayerStats stats)
    {
        switch (type)
        {
            case ItemType.Health:
                stats.Heal(amount);
                break;
            case ItemType.XP:
                stats.AddXP(amount);
                break;
        }
    }
}
