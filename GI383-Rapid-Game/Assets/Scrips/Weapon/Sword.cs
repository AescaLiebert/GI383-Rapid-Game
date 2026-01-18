using UnityEngine;

public class Sword : Weapon
{
    [Header("Sword Settings")]
    public Transform attackPoint;
    public Vector2 attackArea = new Vector2(1f, 0.5f);
    public LayerMask enemyLayers;
    

    public GameObject swordPrefab;




    protected override void PerformAttack()
    {
        {
            if (swordPrefab != null)
            {
                // 1. เก็บเอฟเฟกต์ที่เสกออกมาใส่ตัวแปรไว้ก่อน (ชื่อ currentEffect)
                GameObject currentEffect = Instantiate(swordPrefab, attackPoint.position, attackPoint.rotation);

                // 2. เช็คว่าดาบ/ตัวละคร กำลังหันซ้ายอยู่หรือเปล่า? (ดูจาก lossyScale.x)
                // lossyScale คือขนาดจริงในโลก (ถ้าน้อยกว่า 0 แปลว่ากลับด้านอยู่)
                if (transform.lossyScale.x < 0)
                {
                    // 3. กลับด้านเอฟเฟกต์ตาม
                    Vector3 newScale = currentEffect.transform.localScale;
                    newScale.x *= -1; // คูณ -1 เพื่อกลับด้าน
                    newScale.y *= -1; // *ทริค: บางทีถ้ากลับแค่ X ภาพจะเพี้ยน ถ้าเป็นภาพหมุนๆ ให้ลองกลับ Y ด้วย หรือลองแค่ X ก่อน
                    currentEffect.transform.localScale = newScale;
                }
            }

            // 2. ตรวจจับศัตรูทั้งหมดที่อยู่ในวงกลม
            // OverlapCircleAll(ตำแหน่ง, รัศมี, เลเยอร์ที่จะตรวจ)
            // เพิ่ม attackPoint.eulerAngles.z เป็นตัวที่ 3
            Collider2D[] hitEnemies = Physics2D.OverlapBoxAll(attackPoint.position, attackArea, attackPoint.eulerAngles.z, enemyLayers);

            Debug.Log("Attack");
            // 3. วนลูปสั่งลดเลือดศัตรูทุกตัวที่โดน
            foreach (Collider2D enemy in hitEnemies)
            {
                Debug.Log("ฟันโดน " + enemy.name);

                // ถ้าศัตรูมีสคริปต์ Character หรือ Enemy ให้เรียกฟังก์ชันเจ็บ
                // enemy.GetComponent<Character>()?.TakeDamage(damage);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;

        Gizmos.color = Color.red;
        // บรรทัดนี้จะช่วยให้สี่เหลี่ยมหมุนตามดาบของเราได้
        Gizmos.matrix = Matrix4x4.TRS(attackPoint.position, attackPoint.rotation, Vector3.one);

        // วาดสี่เหลี่ยม (Cube) ที่ตำแหน่ง 0,0 (เพราะเราย้าย matrix มาที่ attackPoint แล้ว)
        Gizmos.DrawWireCube(Vector3.zero, attackArea);
    }
}
