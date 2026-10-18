using System.Collections.Generic;
using UnityEngine;

// 회전 칼날: 고리가 레벨마다 하나씩 늘어나며 플레이어 주변 360도를 계속 돈다 (1레벨 1개 → 5레벨 5개).
// 수치: count=고리 수, area=회전 반경, speed=각속도(도/초), damage=1회 피해, tickInterval=같은 적 재타격 간격
// 레벨별 고리 이펙트(색)는 SkillLevelStat.prefab, 없으면 SkillData.prefab을 쓴다.
// 고리 이펙트는 플레이어 중심에서 재생되고, 보이지 않는 타격 판정 구체가 각 고리를 따라 돈다.
public class OrbitSkill : SkillBase
{
    [SerializeField] float ringRefRadius = 2f;   // 고리 이펙트 원본 반지름 추정치 (클수록 고리가 작게 나옴)
    [SerializeField] float ringHeight = 1.0f;    // 고리 이펙트가 놓이는 월드 높이
    [SerializeField] float hitRadius = 1.0f;     // 타격 판정 구체 반경

    static GameObject bladeTemplate;
    static readonly Dictionary<GameObject, GameObject> ringTemplates = new Dictionary<GameObject, GameObject>();

    readonly List<Transform> blades = new List<Transform>();   // 보이지 않는 타격 판정
    readonly List<Transform> rings = new List<Transform>();    // 고리 이펙트
    float angle;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() { bladeTemplate = null; ringTemplates.Clear(); }

    protected override void OnInit() => Rebuild();
    protected override void OnLevelUp() => Rebuild();

    // 지속형이라 쿨타임 동작 없음
    protected override void Activate() { }

    void Rebuild()
    {
        ReleaseAll();
        var stat = Stat;
        int count = Mathf.Max(1, stat.count);
        var fx = stat.prefab != null ? stat.prefab : Data.prefab;
        float period = 360f / Mathf.Max(1f, stat.speed);
        float scale = stat.area / Mathf.Max(0.1f, ringRefRadius);

        for (int i = 0; i < count; i++)
        {
            var blade = ObjectPool.Get(GetBladeTemplate(), Owner.position, Quaternion.identity);
            blade.GetComponent<OrbitBlade>().Setup(stat.damage, stat.tickInterval, hitRadius);
            blades.Add(blade.transform);

            if (fx == null) continue;
            var ring = ObjectPool.Get(GetRingTemplate(fx), RingPosition(), Quaternion.identity);
            ring.GetComponent<OrbitRingVfx>().Begin(period, period * i / count, scale); // 고리끼리 고르게 시간차
            rings.Add(ring.transform);
        }
    }

    Vector3 RingPosition()
    {
        Vector3 p = Owner.position;
        p.y = ringHeight;
        return p;
    }

    protected override void Update()
    {
        base.Update();
        if (Data == null || blades.Count == 0) return;

        var stat = Stat;
        angle = (angle + stat.speed * Time.deltaTime) % 360f;
        float step = 360f / blades.Count; // 균등 배치

        Vector3 center = Owner.position;
        for (int i = 0; i < blades.Count; i++)
        {
            Vector3 radial = Quaternion.AngleAxis(angle + step * i, Vector3.up) * Vector3.forward;
            blades[i].position = center + radial * stat.area;
        }
        for (int i = 0; i < rings.Count; i++) rings[i].position = RingPosition();
    }

    void ReleaseAll()
    {
        for (int i = 0; i < blades.Count; i++) ObjectPool.Release(blades[i].gameObject);
        for (int i = 0; i < rings.Count; i++) ObjectPool.Release(rings[i].gameObject);
        blades.Clear();
        rings.Clear();
    }

    void OnDestroy() => ReleaseAll(); // 진화로 교체될 때 정리

    // 풀의 원본: 보이지 않는 타격 판정 (OrbitBlade만)
    static GameObject GetBladeTemplate()
    {
        if (bladeTemplate != null) return bladeTemplate;
        bladeTemplate = new GameObject("OrbitHitTemplate");
        bladeTemplate.hideFlags = HideFlags.HideInHierarchy;
        bladeTemplate.SetActive(false);
        bladeTemplate.AddComponent<OrbitBlade>();
        return bladeTemplate;
    }

    // 풀의 원본: 고리 이펙트 프리팹을 OrbitRingVfx로 감싼 것
    static GameObject GetRingTemplate(GameObject fx)
    {
        if (ringTemplates.TryGetValue(fx, out var t) && t != null) return t;

        t = new GameObject($"OrbitRing_{fx.name}");
        t.hideFlags = HideFlags.HideInHierarchy;
        t.SetActive(false);
        t.AddComponent<OrbitRingVfx>();
        var child = Instantiate(fx, t.transform);
        child.transform.localPosition = Vector3.zero;
        child.transform.localRotation = Quaternion.identity;

        ringTemplates[fx] = t;
        return t;
    }
}
