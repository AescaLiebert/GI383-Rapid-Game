using UnityEngine;

public class KnifeProjectile : MonoBehaviour
{
    private float direction;
    private float speed;
    private int damage;

    public void Setup(float dir, float spd, int dmg)
    {
        direction = dir;
        speed = spd;
        damage = dmg;
        Destroy(gameObject, 5f); 
    }

    void Update()
    {
        transform.Translate(Vector2.right * direction * speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // ใช้ Tag หรือ Layer ตาม Enemy.cs ของคุณ
        if (other.CompareTag("Enemy"))
        {
            // เรียกฟังก์ชัน TakeDamage ของศัตรู
            other.GetComponent<Enemy>()?.TakeDamage(damage);
            // *ไม่ Destroy* ตัวมีด เพื่อให้ทะลุศัตรูไปเลยตามโจทย์
        }
    }
}