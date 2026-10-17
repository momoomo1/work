using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour, IPoolable
{
    [SerializeField] float speed = 15f;
    [SerializeField] float damage = 10f;
    [SerializeField] int pierceCount = 0;     // 0이면 첫 적중 후 반환, N이면 N마리 더 관통
    [SerializeField] float lifetime = 2f;
    [SerializeField] float hitRadius = 0.3f;

    readonly HashSet<int> hitIds = new HashSet<int>();
    readonly Collider[] hits = new Collider[16];
    Vector3 direction = Vector3.forward;
    int pierceLeft;
    float timeLeft;

    // 풀에서 꺼낸 직후 호출. 이후 스킬에서도 재사용.
    public void Launch(Vector3 dir, float speed, float damage, int pierceCount, float lifetime)
    {
        direction = dir.sqrMagnitude > 0.0001f ? dir.normalized : transform.forward;
        this.speed = speed;
        this.damage = damage;
        this.pierceCount = pierceCount;
        this.lifetime = lifetime;
        pierceLeft = pierceCount;
        timeLeft = lifetime;
        transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
    }

    public void OnSpawn()
    {
        hitIds.Clear(); // 풀 재사용 시 이전 적중 기록 제거
        direction = transform.forward;
        pierceLeft = pierceCount;
        timeLeft = lifetime;
    }

    public void OnDespawn() { }

    void Update()
    {
        transform.position += direction * (speed * Time.deltaTime);

        timeLeft -= Time.deltaTime;
        if (timeLeft <= 0f) { ObjectPool.Release(gameObject); return; }

        int n = Physics.OverlapSphereNonAlloc(transform.position, hitRadius, hits,
            TargetFinder.EnemyMask, QueryTriggerInteraction.Collide);
        for (int i = 0; i < n; i++)
        {
            var col = hits[i];
            if (!hitIds.Add(col.gameObject.GetInstanceID())) continue; // 같은 적은 한 번만

            if (col.TryGetComponent<IDamageable>(out var target)) target.TakeDamage(damage);

            if (pierceLeft <= 0) { ObjectPool.Release(gameObject); return; }
            pierceLeft--;
        }
    }
}
