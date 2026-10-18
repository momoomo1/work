using UnityEngine;

// 피격/사망 시 히트 이펙트(풀)와 효과음. 플레이어, 적, 보스에 붙임.
[RequireComponent(typeof(Health))]
public class HitFeedback : MonoBehaviour
{
    [SerializeField] GameObject hitVfx;   // 루트에 PooledVfx(IVfx)가 있는 프리팹
    [SerializeField] GameObject deathVfx;
    [SerializeField] float vfxDiameter = 2f;
    [SerializeField] float vfxHeight = 1f;
    [SerializeField] Sfx hitSfx = Sfx.EnemyHit;
    [SerializeField] Sfx deathSfx = Sfx.EnemyDeath;

    Health health;
    float lastCurrent;
    float lastMax;

    void Awake()
    {
        health = GetComponent<Health>();
        health.OnChanged += HandleChanged;
        health.OnDeath += HandleDeath;
    }

    void OnDestroy()
    {
        if (health == null) return;
        health.OnChanged -= HandleChanged;
        health.OnDeath -= HandleDeath;
    }

    void HandleChanged(float current, float max)
    {
        // 최대 체력이 바뀐 경우(풀 재사용 시 초기화)는 피격이 아님
        bool resync = !Mathf.Approximately(max, lastMax);
        bool damaged = !resync && current < lastCurrent;
        lastMax = max;
        lastCurrent = current;

        if (damaged && current > 0f)
        {
            Spawn(hitVfx);
            AudioManager.Play(hitSfx);
        }
    }

    void HandleDeath()
    {
        Spawn(deathVfx);
        AudioManager.Play(deathSfx);
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
