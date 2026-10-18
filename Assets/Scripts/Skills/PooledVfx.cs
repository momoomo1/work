using UnityEngine;

// 파티클 이펙트용: 크기를 맞추고 재생한 뒤 일정 시간 후 풀로 반환
public class PooledVfx : MonoBehaviour, IPoolable, IVfx
{
    [SerializeField] float referenceDiameter = 6f; // 프리팹 원본 이펙트의 대략적인 지름(유닛). 요청 지름에 맞춰 스케일됨
    [SerializeField] float lifetime = 2f;          // 0이면 Play의 duration 사용
    [SerializeField] bool forceOneShot = true;     // 루핑 파티클을 1회 재생으로 변경

    ParticleSystem[] systems;
    Component[] graphEffects; // VFX Graph 이펙트
    Vector3 baseScale;
    float timeLeft;
    bool playing;

    // 이펙트 에셋이 자체적으로 오브젝트를 파괴/이동시키는 스크립트는 풀링·투사체 로직과 충돌 → 제거
    //  - Cartoon FX의 CFXR_Effect (재생 후 Destroy/Disable)
    //  - Free Slash VFX 데모의 Projectile 등 (MaykerStudio.Demo 네임스페이스, 스스로 이동)
    public static void StripAutoClear(GameObject root)
    {
        foreach (var mb in root.GetComponentsInChildren<MonoBehaviour>(true))
        {
            if (mb == null) continue;
            var type = mb.GetType();
            if (type.Name == "CFXR_Effect" || (type.Namespace != null && type.Namespace.StartsWith("MaykerStudio.Demo")))
                Destroy(mb);
        }
    }

    void Awake()
    {
        StripAutoClear(gameObject);
        systems = GetComponentsInChildren<ParticleSystem>(true);
        graphEffects = VisualEffectUtil.GetAll(gameObject);
        baseScale = transform.localScale;
        // 부모 스케일이 파티클 크기에도 적용되도록
        foreach (var ps in systems)
        {
            var main = ps.main;
            main.scalingMode = ParticleSystemScalingMode.Hierarchy;
            main.stopAction = ParticleSystemStopAction.None; // 에셋의 자동 Destroy/Disable 방지 (풀이 관리)
        }
    }

    public void Play(float diameter, float duration)
    {
        float k = referenceDiameter > 0f ? diameter / referenceDiameter : 1f;
        transform.localScale = baseScale * k;
        timeLeft = lifetime > 0f ? lifetime : duration;
        playing = true;

        foreach (var ps in systems)
        {
            if (ps == null) continue;
            var main = ps.main;
            main.stopAction = ParticleSystemStopAction.None;
            if (forceOneShot) main.loop = false;
            ps.Clear(true);
            ps.Play(true);
        }
        VisualEffectUtil.Restart(graphEffects);
    }

    public void OnSpawn() { playing = false; }
    public void OnDespawn() { playing = false; }

    void Update()
    {
        if (!playing) return;
        timeLeft -= Time.deltaTime;
        if (timeLeft <= 0f) ObjectPool.Release(gameObject);
    }
}
