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

    // ---- 공간 격자 (프레임당 한 번만 다시 만듦) ----
    const float CellSize = 1.5f; // separationRadius 이상이어야 함
    static int gridFrame = -1;
    static readonly Dictionary<long, List<Enemy>> grid = new Dictionary<long, List<Enemy>>();
    static readonly Stack<List<Enemy>> listPool = new Stack<List<Enemy>>();

    static long Key(int x, int z) => ((long)x << 32) ^ (uint)z;

    static void BuildGrid()
    {
        if (gridFrame == Time.frameCount) return;
        gridFrame = Time.frameCount;

        foreach (var l in grid.Values) { l.Clear(); listPool.Push(l); }
        grid.Clear();
        for (int i = 0; i < active.Count; i++)
        {
            var e = active[i];
            Vector3 p = e.transform.position;
            long key = Key(Mathf.FloorToInt(p.x / CellSize), Mathf.FloorToInt(p.z / CellSize));
            if (!grid.TryGetValue(key, out var list))
            {
                list = listPool.Count > 0 ? listPool.Pop() : new List<Enemy>(8);
                grid[key] = list;
            }
            list.Add(e);
        }
    }
    [NonSerialized] EnemyData data;
    Health health;
    float nextAttackTime;
    float fixedY;
    [SerializeField] float knockDecay = 25f; // 넉백 속도가 줄어드는 정도 (클수록 짧게 밀림)
    Vector3 knockVelocity;
    float damageMult = 1f;     // 웨이브 강화 배율
    float baseSpeedMult = 1f;
    float speedMultiplier = 1f;
    float slowUntil;

    public EnemyData Data => data;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        active.Clear();
        grid.Clear();
        gridFrame = -1;
        player = null;
        playerDamageable = null;
        OnAnyDied = null;
    }

    void Awake()
    {
        health = GetComponent<Health>();
        health.OnDeath += HandleDeath;

        // 움직이는 콜라이더는 리지드바디(키네마틱)가 있어야 수백 마리여도 물리 비용이 작다
        if (!TryGetComponent<Rigidbody>(out var rb)) rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    void OnDestroy()
    {
        active.Remove(this); // 씬 재로드로 파괴될 때 정적 목록에 남지 않게
        if (health != null) health.OnDeath -= HandleDeath;
    }

    // 스포너가 풀에서 꺼낸 직후 호출
    public void Init(EnemyData enemyData, float healthMultiplier = 1f, float damageMultiplier = 1f, float speedMultiplier = 1f)
    {
        data = enemyData;
        damageMult = damageMultiplier;
        baseSpeedMult = speedMultiplier;
        health.SetMaxHealth(data.maxHealth * healthMultiplier);
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

    // 피격 시 짧게 밀려남 (속도가 점점 줄어듦)
    public void Knockback(Vector3 direction, float speed)
    {
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.0001f) return;
        knockVelocity = direction.normalized * speed;
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
        knockVelocity = Vector3.zero;
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
        float speed = data.moveSpeed * baseSpeedMult * (Time.time < slowUntil ? speedMultiplier : 1f);
        Vector3 velocity = dist > contactRange * 0.8f ? dir * speed : Vector3.zero;

        // 겹침 방지: 가까운 적에게서 멀어지는 방향으로 밀기 (격자로 주변 칸만 검사 → 적이 수백 마리여도 가볍다)
        Vector3 push = Vector3.zero;
        float sqrRadius = separationRadius * separationRadius;
        BuildGrid();
        int cx = Mathf.FloorToInt(pos.x / CellSize), cz = Mathf.FloorToInt(pos.z / CellSize);
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dz = -1; dz <= 1; dz++)
            {
                if (!grid.TryGetValue(Key(cx + dx, cz + dz), out var cell)) continue;
                for (int i = 0; i < cell.Count; i++)
                {
                    var other = cell[i];
                    if (other == this) continue;
                    Vector3 away = pos - other.transform.position;
                    away.y = 0f;
                    float sqr = away.sqrMagnitude;
                    if (sqr >= sqrRadius) continue;
                    if (sqr < 0.0001f) { away = UnityEngine.Random.insideUnitSphere; away.y = 0f; sqr = 0.01f; }
                    float d = Mathf.Sqrt(sqr);
                    push += away / d * (1f - d / separationRadius);
                }
            }
        }
        velocity += push * (separationStrength * speed);

        pos += (velocity + knockVelocity) * Time.deltaTime;
        knockVelocity = Vector3.MoveTowards(knockVelocity, Vector3.zero, knockDecay * Time.deltaTime);
        pos.y = fixedY;
        transform.position = pos;

        if (dir.sqrMagnitude > 0.001f) transform.rotation = Quaternion.LookRotation(dir, Vector3.up);

        // 접촉 데미지
        if (dist <= contactRange && Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + data.attackInterval;
            playerDamageable?.TakeDamage(data.contactDamage * damageMult);
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
