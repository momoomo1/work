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
    readonly Collider[] hits = new Collider[48];
    TrailRenderer trail;
    float pullRadius;
    float pullStrength;
    Vector3 direction = Vector3.forward;
    int pierceLeft;
    float timeLeft;

    // 풀에서 꺼낸 직후 호출. 이후 스킬에서도 재사용.
    public void Launch(Vector3 dir, float speed, float damage, int pierceCount, float lifetime, float hitRadius = -1f)
    {
        if (hitRadius > 0f) this.hitRadius = hitRadius;
        direction = dir.sqrMagnitude > 0.0001f ? dir.normalized : transform.forward;
        this.speed = speed;
        this.damage = damage;
        this.pierceCount = pierceCount;
        this.lifetime = lifetime;
        pierceLeft = pierceCount;
        timeLeft = lifetime;
        transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
    }

    // 지나가는 적을 투사체 쪽으로 끌어당김 (진공참). Launch 이후 호출.
    public void SetPull(float radius, float strength)
    {
        pullRadius = radius;
        pullStrength = strength;
    }

    public void OnSpawn()
    {
        pullRadius = 0f;
        pullStrength = 0f;
        if (trail == null) trail = GetComponentInChildren<TrailRenderer>(true);
        if (trail != null) trail.Clear(); // 풀 재사용 시 이전 위치에서 이어지는 꼬리 방지
        hitIds.Clear(); // 풀 재사용 시 이전 적중 기록 제거
        direction = transform.forward;
        pierceLeft = pierceCount;
        timeLeft = lifetime;
    }

    public void OnDespawn() { }

    void PullEnemies()
    {
        int n = Physics.OverlapSphereNonAlloc(transform.position, pullRadius, hits,
            TargetFinder.EnemyMask, QueryTriggerInteraction.Collide);
        for (int i = 0; i < n; i++)
        {
            if (!hits[i].TryGetComponent<Enemy>(out var enemy)) continue; // 보스는 끌어당기지 않음
            Vector3 to = transform.position - enemy.transform.position;
            to.y = 0f;
            float dist = to.magnitude;
            if (dist < 0.05f) continue;
            enemy.ApplyDisplacement(to / dist * Mathf.Min(dist, pullStrength * Time.deltaTime));
        }
    }

    void Update()
    {
        transform.position += direction * (speed * Time.deltaTime);

        timeLeft -= Time.deltaTime;
        if (timeLeft <= 0f) { ObjectPool.Release(gameObject); return; }

        if (pullRadius > 0f) PullEnemies();

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
