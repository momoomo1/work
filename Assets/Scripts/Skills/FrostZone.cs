using System.Collections.Generic;
using UnityEngine;

// 서리 구름 장판. 지속시간 동안 tickInterval마다 안의 적에게 데미지 + 슬로우, 끝나면 풀로 반환.
public class FrostZone : MonoBehaviour, IPoolable
{
    float baseY;
    float radius;
    float damage;
    float tickInterval;
    float slowMultiplier; // 이동 속도 배율 (0.7 = 30% 감속)
    float timeLeft;
    float tickTimer;
    bool active;

    readonly List<Transform> targets = new List<Transform>();

    void Awake() => baseY = transform.localScale.y;

    public void Begin(float radius, float damage, float tickInterval, float duration, float slowRatio)
    {
        this.radius = radius;
        this.damage = damage;
        this.tickInterval = Mathf.Max(0.05f, tickInterval);
        slowMultiplier = 1f - Mathf.Clamp01(slowRatio);
        timeLeft = duration;
        tickTimer = 0f; // 생성 즉시 첫 틱
        transform.localScale = new Vector3(radius * 2f, baseY, radius * 2f);
        active = true;
    }

    public void OnSpawn() { active = false; }
    public void OnDespawn() { active = false; }

    void Update()
    {
        if (!active) return;

        timeLeft -= Time.deltaTime;
        if (timeLeft <= 0f) { ObjectPool.Release(gameObject); return; }

        tickTimer -= Time.deltaTime;
        if (tickTimer > 0f) return;
        tickTimer = tickInterval;

        TargetFinder.FindAll(transform.position, radius, targets);
        for (int i = 0; i < targets.Count; i++)
        {
            var t = targets[i];
            // 슬로우는 다음 틱 전까지 유지 (벗어나면 곧 풀림)
            if (t.TryGetComponent<Enemy>(out var enemy)) enemy.SetSpeedMultiplier(slowMultiplier, tickInterval + 0.2f);
            if (t.TryGetComponent<IDamageable>(out var target)) target.TakeDamage(damage);
        }
    }
}
