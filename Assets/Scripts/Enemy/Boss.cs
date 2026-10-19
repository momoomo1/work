using System;
using UnityEngine;

// 보스: 추적 → 돌진 예고 → 돌진을 반복. Health/IDamageable은 일반 적과 동일하게 사용.
[RequireComponent(typeof(Health))]
public class Boss : MonoBehaviour, IPoolable
{
    [SerializeField] Transform telegraph; // 돌진 예고 표시 (납작한 큐브)
    [SerializeField] float stopFactor = 0.3f;  // 접촉 거리 × 이 값보다 가까워지면 멈춤 (작을수록 끝까지 파고듦)
    [SerializeField] float pushFactor = 0.55f; // 접촉 거리 × 이 값보다 가까우면 플레이어를 밀어냄

    public static event Action<Boss> OnBossDied;

    enum State { Chase, ChargeWarn, Charging }

    BossData data;
    Health health;
    State state;
    float stateTimer;
    float nextPatternTime;
    float nextContactTime;
    float fixedY;
    Vector3 chargeDir;
    bool chargeHit;
    float damageMult = 1f;

    public BossData Data => data;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() => OnBossDied = null;

    void Awake()
    {
        health = GetComponent<Health>();
        health.OnDeath += HandleDeath;
    }

    void OnDestroy()
    {
        if (health != null) health.OnDeath -= HandleDeath;
    }

    public void Init(BossData bossData, float healthMultiplier = 1f, float damageMultiplier = 1f)
    {
        data = bossData;
        damageMult = damageMultiplier;
        health.SetMaxHealth(data.maxHealth * healthMultiplier);
        fixedY = transform.position.y;
        EnterChase();
    }

    public void OnSpawn()
    {
        data = null;
        SetTelegraph(false);
    }

    public void OnDespawn() => SetTelegraph(false);

    void Update()
    {
        if (data == null) return;
        var player = PlayerLocator.Player;
        if (player == null) return;

        switch (state)
        {
            case State.Chase: UpdateChase(player); break;
            case State.ChargeWarn: UpdateChargeWarn(); break;
            case State.Charging: UpdateCharging(player); break;
        }
    }

    void UpdateChase(Transform player)
    {
        Vector3 to = Flat(player.position - transform.position);
        float dist = to.magnitude;
        Vector3 dir = dist > 0.001f ? to / dist : transform.forward;

        // 플레이어 바로 앞에서 멈추지 않고 계속 밀고 들어옴 (붙어 있어도 가만히 서 있지 않게)
        if (dist > data.contactRange * stopFactor) Move(dir * data.moveSpeed);
        if (dist > 0.001f) transform.rotation = Quaternion.LookRotation(dir, Vector3.up);

        // 너무 가까우면 플레이어를 앞으로 밀어냄 → 비비고 있으면 계속 밀려서 움직여야 함
        if (dist < data.contactRange * pushFactor && player.TryGetComponent<CharacterController>(out var cc))
            cc.Move(dir * (data.moveSpeed * 1.3f) * Time.deltaTime);

        if (dist <= data.contactRange && Time.time >= nextContactTime)
        {
            nextContactTime = Time.time + data.contactInterval;
            PlayerLocator.Damageable?.TakeDamage(data.contactDamage * damageMult);
        }

        if (Time.time >= nextPatternTime) StartCharge(player);
    }

    // 방향 고정 후 예고 → 돌진
    void StartCharge(Transform player)
    {
        Vector3 to = Flat(player.position - transform.position);
        chargeDir = to.sqrMagnitude > 0.001f ? to.normalized : transform.forward;
        transform.rotation = Quaternion.LookRotation(chargeDir, Vector3.up);

        float length = data.chargeSpeed * data.chargeDuration;
        telegraph.localPosition = new Vector3(0f, 0.05f - fixedY, length * 0.5f);
        telegraph.localRotation = Quaternion.identity;
        telegraph.localScale = new Vector3(data.telegraphWidth, 0.02f, length);
        SetTelegraph(true);

        state = State.ChargeWarn;
        stateTimer = data.chargeWarnTime;
    }

    void UpdateChargeWarn()
    {
        stateTimer -= Time.deltaTime;
        if (stateTimer > 0f) return;
        SetTelegraph(false);
        chargeHit = false;
        state = State.Charging;
        stateTimer = data.chargeDuration;
    }

    void UpdateCharging(Transform player)
    {
        Move(chargeDir * data.chargeSpeed);

        if (!chargeHit && Flat(player.position - transform.position).sqrMagnitude <= data.contactRange * data.contactRange)
        {
            chargeHit = true;
            PlayerLocator.Damageable?.TakeDamage(data.chargeDamage * damageMult);
        }

        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0f) EnterChase();
    }

    void EnterChase()
    {
        state = State.Chase;
        nextPatternTime = Time.time + data.patternInterval;
    }

    void Move(Vector3 velocity)
    {
        Vector3 pos = transform.position + velocity * Time.deltaTime;
        pos.y = fixedY;
        transform.position = pos;
    }

    void SetTelegraph(bool on)
    {
        if (telegraph != null) telegraph.gameObject.SetActive(on);
    }

    static Vector3 Flat(Vector3 v) { v.y = 0f; return v; }

    void HandleDeath()
    {
        SetTelegraph(false);
        OnBossDied?.Invoke(this);
        ObjectPool.Release(gameObject);
    }
}
