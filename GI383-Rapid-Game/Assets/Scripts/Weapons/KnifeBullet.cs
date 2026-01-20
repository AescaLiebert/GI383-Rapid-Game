using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Settings")]
    public float speed = 20f;

    
    public int damage = 5;
    
    
    public float lifeTime = 2f; 

    [Header("Requirements")]
    public Rigidbody2D rb;

    void Start()
    {
        
        rb.linearVelocity = transform.right * speed;

       
        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        
        Enemy enemy = hitInfo.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage); 
            Destroy(gameObject);      
            return;
        }

        
        if (!hitInfo.CompareTag("Player") && !hitInfo.isTrigger)
        {
            Destroy(gameObject); 
        }
    }
}