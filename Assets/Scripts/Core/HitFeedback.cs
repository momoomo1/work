using System.Collections.Generic;
using UnityEngine;

// 피격/사망 시 타격감: 히트 이펙트(풀), 효과음, 데미지 숫자, 붉은 깜빡임, 넉백, 화면 흔들림.
// 플레이어, 적, 보스에 붙임. 종류(플레이어/보스/일반 적)는 컴포넌트로 자동 판별.
[RequireComponent(typeof(Health))]
public class HitFeedback : MonoBehaviour
{
    [SerializeField] GameObject hitVfx;   // 루트에 PooledVfx(IVfx)가 있는 프리팹
    [SerializeField] GameObject deathVfx;
    [SerializeField] float vfxDiameter = 2f;
    [SerializeField] float vfxHeight = 1f;
    [SerializeField] Sfx hitSfx = Sfx.EnemyHit;
    [SerializeField] Sfx deathSfx = Sfx.EnemyDeath;

    [Header("타격감")]
    [SerializeField] bool showDamageNumber = true;
    [SerializeField] float numberHeight = 1.8f;       // 숫자가 뜨는 월드 높이 (보스는 자동으로 더 높게)
    [SerializeField] float flashDuration = 0.09f;
    [SerializeField] Color flashColor = new Color(1f, 0.35f, 0.35f);
    [SerializeField] float knockbackSpeed = 4f;       // 일반 적만 해당
    [SerializeField] float playerHitShake = 0.2f;     // 플레이어 피격 시 화면 흔들림 세기
    [SerializeField] float bossDeathShake = 0.5f;

    static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    static readonly int ColorId = Shader.PropertyToID("_Color");

    Health health;
    Enemy enemy;
    bool isPlayer;
    bool isBoss;
    Renderer[] flashRenderers;
    MaterialPropertyBlock block;
    float flashTimer;
    float lastCurrent;
    float lastMax;

    void Awake()
    {
        health = GetComponent<Health>();
        enemy = GetComponent<Enemy>();
        isBoss = GetComponent<Boss>() != null;
        isPlayer = CompareTag("Player");

        // 메시 렌더러만 깜빡임 대상 (파티클/트레일 제외)
        var list = new List<Renderer>();
        foreach (var r in GetComponentsInChildren<Renderer>(true))
            if (r is MeshRenderer || r is SkinnedMeshRenderer) list.Add(r);
        flashRenderers = list.ToArray();
        block = new MaterialPropertyBlock();

        health.OnChanged += HandleChanged;
        health.OnDeath += HandleDeath;
    }

    void OnDestroy()
    {
        if (health == null) return;
        health.OnChanged -= HandleChanged;
        health.OnDeath -= HandleDeath;
    }

    void OnDisable() => ClearFlash();

    void HandleChanged(float current, float max)
    {
        // 최대 체력이 바뀐 경우(풀 재사용 시 초기화)는 피격이 아님
        bool resync = !Mathf.Approximately(max, lastMax);
        float damage = resync ? 0f : lastCurrent - current;
        lastMax = max;
        lastCurrent = current;
        if (damage <= 0f) return;

        // 데미지 숫자: 치명타(마지막 일격) 포함
        if (showDamageNumber)
        {
            Vector3 pos = transform.position;
            pos.y = isBoss ? numberHeight + 2.2f : numberHeight;
            Color color = isPlayer ? new Color(1f, 0.25f, 0.25f)
                        : isBoss ? new Color(1f, 0.65f, 0.2f)
                        : new Color(1f, 0.95f, 0.6f);
            float scale = 1f + Mathf.Min(1f, damage / 40f); // 큰 데미지일수록 크게
            DamageNumber.Spawn(pos, damage, color, scale);
        }

        if (current <= 0f) return; // 사망 연출은 HandleDeath에서

        Spawn(hitVfx);
        AudioManager.Play(hitSfx);
        StartFlash();

        if (enemy != null && knockbackSpeed > 0f)
        {
            var player = PlayerLocator.Player;
            if (player != null) enemy.Knockback(transform.position - player.position, knockbackSpeed); // 플레이어 반대 방향으로
        }
        if (isPlayer) CameraFollow.Shake(playerHitShake, 0.2f);
    }

    void HandleDeath()
    {
        Spawn(deathVfx);
        AudioManager.Play(deathSfx);
        ClearFlash();
        if (isBoss) CameraFollow.Shake(bossDeathShake, 0.5f);
    }

    void Update()
    {
        if (flashTimer <= 0f) return;
        flashTimer -= Time.deltaTime;
        if (flashTimer <= 0f) ClearFlash();
    }

    void StartFlash()
    {
        if (flashRenderers.Length == 0) return;
        block.SetColor(BaseColorId, flashColor);
        block.SetColor(ColorId, flashColor);
        foreach (var r in flashRenderers) if (r != null) r.SetPropertyBlock(block);
        flashTimer = flashDuration;
    }

    void ClearFlash()
    {
        flashTimer = 0f;
        if (flashRenderers == null) return;
        foreach (var r in flashRenderers) if (r != null) r.SetPropertyBlock(null);
    }

    void Spawn(GameObject prefab)
    {
        if (prefab == null) return;
        Vector3 pos = transform.position;
        pos.y = vfxHeight; // 월드 높이 고정
        var obj = ObjectPool.Get(prefab, pos, Quaternion.identity);
        if (obj.TryGetComponent<IVfx>(out var vfx)) vfx.Play(vfxDiameter, 1.5f);
    }
}
