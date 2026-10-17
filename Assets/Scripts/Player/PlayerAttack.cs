using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField] GameObject projectilePrefab;
    [SerializeField] float attackInterval = 0.6f;
    [SerializeField] float attackRange = 12f;
    [SerializeField] float damage = 10f;
    [SerializeField] float projectileSpeed = 15f;
    [SerializeField] int pierceCount = 0;
    [SerializeField] float projectileLifetime = 1.2f; // 사거리 ≈ 속도 × 수명
    [SerializeField] float muzzleOffset = 0.6f;

    float timer;

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer > 0f || projectilePrefab == null) return;

        var target = TargetFinder.FindNearest(transform.position, attackRange);
        if (target == null) return;

        Vector3 dir = target.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return;
        dir.Normalize();

        timer = attackInterval;
        var obj = ObjectPool.Get(projectilePrefab, transform.position + dir * muzzleOffset, Quaternion.LookRotation(dir));
        obj.GetComponent<Projectile>().Launch(dir, projectileSpeed, damage, pierceCount, projectileLifetime);
    }
}
