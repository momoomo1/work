using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class Enemy : MonoBehaviour, IPoolable
{
    [SerializeField] float contactRange = 1.3f;       // 이 거리 안이면 멈추고 접촉 데미지
    [SerializeField] float separationRadius = 1f;     // 이 거리 안의 다른 적에게서 밀려남
    [SerializeField] float separationStrength = 1.5f; // 이동 속도 대비 분리 비율

    // 어떤 적이든 죽으면 발생 (GameManager가 구독)
    public static event Action<Enemy> OnAnyDied;

    static readonly List<Enemy> active = new List<Enemy>();
    public static int ActiveCount => active.Count;

    static Transform player;
    static IDamageable playerDamageable;

    [NonSerialized] EnemyData data;
    Health health;
    float nextAttackTime;
    float fixedY;
    float speedMultiplier = 1f;
    float slowUntil;

    public EnemyData Data => data;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        active.Clear();
        player = null;
        playerDamageable = null;
        OnAnyDied = null;
    }

    void Awake()
    {
        health = GetComponent<Health>();
        health.OnDeath += HandleDeath;
    }

    void OnDestroy()
    {
        active.Remove(this); // 씬 재로드로 파괴될 때 정적 목록에 남지 않게
        if (health != null) health.OnDeath -= HandleDeath;
    }

    // 스포너가 풀에서 꺼낸 직후 호출
    public void Init(EnemyData enemyData)
    {
        data = enemyData;
        health.SetMaxHealth(data.maxHealth);
        nextAttackTime = Time.time;
        fixedY = transform.position.y;
    }

    // 이동 속도 배율 적용 (예: 0.6 = 40% 감속). duration 동안 유지되며 더 강한 감속이 우선.
    public void SetSpeedMultiplier(float multiplier, float duration)
    {
        if (Time.time >= slowUntil || multiplier <= speedMultiplier)
        {
            speedMultiplier = multiplier;
            slowUntil = Time.time + duration;
        }
    }

    // 외부 힘으로 위치를 밀거나 당김 (수평만)
    public void ApplyDisplacement(Vector3 delta)
    {
        delta.y = 0f;
        transform.position += delta;
    }

    public void OnSpawn()
    {
        data = null; // Init 전까지 동작 안 함
        slowUntil = 0f;
        speedMultiplier = 1f;
        active.Add(this);
    }

    public void OnDespawn() => active.Remove(this);

    void Update()
    {
        if (data == null) return;
        if (!FindPlayer()) return;

        Vector3 pos = transform.position;
        Vector3 toPlayer = player.position - pos;
        toPlayer.y = 0f;
        float dist = toPlayer.magnitude;
        Vector3 dir = dist > 0.001f ? toPlayer / dist : Vector3.zero;

        // 직선 추적 (접촉 거리 안이면 멈춤)
        float speed = data.moveSpeed * (Time.time < slowUntil ? speedMultiplier : 1f);
        Vector3 velocity = dist > contactRange * 0.8f ? dir * speed : Vector3.zero;

        // 겹침 방지: 가까운 적에게서 멀어지는 방향으로 밀기
        Vector3 push = Vector3.zero;
        float sqrRadius = separationRadius * separationRadius;
        for (int i = 0; i < active.Count; i++)
        {
            var other = active[i];
            if (other == this) continue;
            Vector3 away = pos - other.transform.position;
            away.y = 0f;
            float sqr = away.sqrMagnitude;
            if (sqr >= sqrRadius) continue;
            if (sqr < 0.0001f) { away = UnityEngine.Random.insideUnitSphere; away.y = 0f; sqr = 0.01f; }
            float d = Mathf.Sqrt(sqr);
            push += away / d * (1f - d / separationRadius);
        }
        velocity += push * (separationStrength * speed);

        pos += velocity * Time.deltaTime;
        pos.y = fixedY;
        transform.position = pos;

        if (dir.sqrMagnitude > 0.001f) transform.rotation = Quaternion.LookRotation(dir, Vector3.up);

        // 접촉 데미지
        if (dist <= contactRange && Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + data.attackInterval;
            playerDamageable?.TakeDamage(data.contactDamage);
        }
    }

    bool FindPlayer()
    {
        if (player == null)
        {
            var go = GameObject.FindGameObjectWithTag("Player");
            if (go == null) return false;
            player = go.transform;
            playerDamageable = go.GetComponent<IDamageable>();
        }
        return true;
    }

    void HandleDeath()
    {
        OnAnyDied?.Invoke(this);
        ObjectPool.Release(gameObject);
    }
}
