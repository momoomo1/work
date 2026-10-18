using System.Collections.Generic;
using UnityEngine;

// 회전 칼날 1개. 닿은 적에게 적별 쿨다운으로 데미지.
public class OrbitBlade : MonoBehaviour, IPoolable
{
    [SerializeField] float hitRadius = 0.6f;

    readonly Dictionary<int, float> nextHitTime = new Dictionary<int, float>();
    readonly Collider[] hits = new Collider[16];
    TrailRenderer trail;
    bool initialized;
    float damage;
    float hitInterval = 0.3f;

    public void Setup(float damage, float hitInterval, float hitRadius = -1f)
    {
        this.damage = damage;
        this.hitInterval = hitInterval;
        if (hitRadius > 0f) this.hitRadius = hitRadius;
    }

    public void OnSpawn()
    {
        nextHitTime.Clear();
        if (!initialized)
        {
            PooledVfx.StripAutoClear(gameObject); // 이펙트 에셋의 자동 파괴 방지
            trail = GetComponentInChildren<TrailRenderer>(true);
            initialized = true;
        }
        if (trail != null) trail.Clear(); // 풀 재사용 시 이전 위치에서 이어지는 꼬리 방지
    }
    public void OnDespawn() { }

    void Update()
    {
        int n = Physics.OverlapSphereNonAlloc(transform.position, hitRadius, hits,
            TargetFinder.EnemyMask, QueryTriggerInteraction.Collide);
        if (n == 0) return;

        if (nextHitTime.Count > 32) Prune();

        float now = Time.time;
        for (int i = 0; i < n; i++)
        {
            int id = hits[i].gameObject.GetInstanceID();
            if (nextHitTime.TryGetValue(id, out var t) && now < t) continue;
            nextHitTime[id] = now + hitInterval;
            if (hits[i].TryGetComponent<IDamageable>(out var target)) target.TakeDamage(damage);
        }
    }

    void Prune()
    {
        float now = Time.time;
        var expired = new List<int>();
        foreach (var kv in nextHitTime) if (kv.Value < now) expired.Add(kv.Key);
        foreach (var id in expired) nextHitTime.Remove(id);
    }
}
