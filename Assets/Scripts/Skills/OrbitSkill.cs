using System.Collections.Generic;
using UnityEngine;

// 회전 칼날: 고리가 레벨마다 하나씩 늘어나며 플레이어 주변 360도를 돈다 (1레벨 1개 → 5레벨 5개).
// 수치: count=고리 수, area=회전 반경, speed=각속도(도/초), damage=1회 피해, tickInterval=같은 적 재타격 간격
// 고리 하나 = 타격 판정(OrbitBlade) + 이펙트(Cartoon FX Sword Trail 등). 이펙트는 레벨별로
// SkillLevelStat.prefab(없으면 SkillData.prefab)에서 가져오고, 코드가 풀 호환 래퍼로 감싸서 쓴다.
public class OrbitSkill : SkillBase
{
    [SerializeField] float ringScale = 0.6f;      // 고리 이펙트 크기 (클수록 큼)
    [SerializeField] float hitRadius = 0.9f;      // 타격 판정 반경
    [SerializeField] float heightOffset = -0.1f;  // 플레이어 중심 대비 고리 높이
    [SerializeField] float phaseStagger = 0.3f;   // 고리끼리 이펙트 시작 시간차(초)

    static readonly Dictionary<GameObject, GameObject> templates = new Dictionary<GameObject, GameObject>();
    static GameObject hitOnlyTemplate;

    readonly List<Transform> rings = new List<Transform>();
    float angle;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() { templates.Clear(); hitOnlyTemplate = null; }

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

        for (int i = 0; i < count; i++)
        {
            var obj = ObjectPool.Get(fx != null ? GetTemplate(fx) : GetHitOnlyTemplate(), Owner.position, Quaternion.identity);
            if (obj.TryGetComponent<OrbitBlade>(out var blade)) blade.Setup(stat.damage, stat.tickInterval, hitRadius);
            if (obj.TryGetComponent<OrbitRingVfx>(out var vfx)) vfx.Begin(ringScale, i * phaseStagger);
            rings.Add(obj.transform);
        }
    }

    protected override void Update()
    {
        base.Update();
        if (Data == null || rings.Count == 0) return;

        var stat = Stat;
        angle = (angle + stat.speed * Time.deltaTime) % 360f;
        float step = 360f / rings.Count; // 균등 배치

        Vector3 center = Owner.position + Vector3.up * heightOffset;
        for (int i = 0; i < rings.Count; i++)
        {
            Vector3 radial = Quaternion.AngleAxis(angle + step * i, Vector3.up) * Vector3.forward;
            rings[i].position = center + radial * stat.area;
        }
    }

    void ReleaseAll()
    {
        for (int i = 0; i < rings.Count; i++) ObjectPool.Release(rings[i].gameObject);
        rings.Clear();
    }

    void OnDestroy() => ReleaseAll(); // 진화로 교체될 때 정리

    // 풀의 원본: 타격 판정(OrbitBlade) + 이펙트 제어(OrbitRingVfx) + 이펙트 프리팹 (프리팹 에셋을 따로 만들 필요 없음)
    static GameObject GetTemplate(GameObject fx)
    {
        if (templates.TryGetValue(fx, out var t) && t != null) return t;

        t = new GameObject($"OrbitRing_{fx.name}");
        t.hideFlags = HideFlags.HideInHierarchy;
        t.SetActive(false);
        t.AddComponent<OrbitBlade>();
        t.AddComponent<OrbitRingVfx>();
        var child = Instantiate(fx, t.transform);
        child.transform.localPosition = Vector3.zero;
        child.transform.localRotation = Quaternion.identity;

        templates[fx] = t;
        return t;
    }

    static GameObject GetHitOnlyTemplate()
    {
        if (hitOnlyTemplate != null) return hitOnlyTemplate;
        hitOnlyTemplate = new GameObject("OrbitHitTemplate");
        hitOnlyTemplate.hideFlags = HideFlags.HideInHierarchy;
        hitOnlyTemplate.SetActive(false);
        hitOnlyTemplate.AddComponent<OrbitBlade>();
        return hitOnlyTemplate;
    }
}
