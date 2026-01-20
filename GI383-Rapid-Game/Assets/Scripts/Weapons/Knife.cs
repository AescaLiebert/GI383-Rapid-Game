using UnityEngine;

public class Knife: Weapon
{

    public GameObject bulletPrefab;
    public Transform firePoint;
    protected override void PerformAttack()
    {
        if (bulletPrefab != null && firePoint != null)
        {
            
            Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        }
        else
        {
            Debug.LogWarning("Missing Bullet Prefab or Fire Point in Gun script");
        }
    }

    public override void Turn(bool isRight)
    {
        if (firePoint == null) return;

        
        Vector3 pos = firePoint.localPosition;
        pos.x = isRight ? Mathf.Abs(pos.x) : -Mathf.Abs(pos.x);
        firePoint.localPosition = pos;

        Vector3 rot = firePoint.localEulerAngles;
        rot.y = isRight ? 0f : 180f;
        firePoint.localEulerAngles = rot;
    }
}
