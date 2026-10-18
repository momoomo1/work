using System.Collections.Generic;
using UnityEngine;

// 회전 칼날: 고리가 레벨마다 하나씩 늘어나며 플레이어 주변 360도를 돈다 (1레벨 1개 → 5레벨 5개).
// 수치: count=고리 수, area=회전 반경, speed=각속도(도/초), damage=1회 피해, tickInterval=같은 적 재타격 간격
// 레벨별 고리 프리팹(색)은 SkillLevelStat.prefab, 없으면 SkillData.prefab을 쓴다.
// 고리 프리팹에는 OrbitBlade(타격 판정)와 OrbitRingMesh(모양)가 붙어 있다.
public class OrbitSkill : SkillBase
{
    [SerializeField] float hitRadius = 0.9f;      // 타격 판정 반경
    [SerializeField] float heightOffset = -0.1f;  // 플레이어 중심 대비 고리 높이

    static GameObject hitOnlyTemplate;

    readonly List<Transform> blades = new List<Transform>();
    float angle;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() => hitOnlyTemplate = null;

    protected override void OnInit() => Rebuild();
    protected override void OnLevelUp() => Rebuild();

    // 지속형이라 쿨타임 동작 없음
    protected override void Activate() { }

    void Rebuild()
    {
        ReleaseAll();
        var stat = Stat;
        int count = Mathf.Max(1, stat.count);
        var prefab = stat.prefab != null ? stat.prefab : Data.prefab;
        if (prefab == null) prefab = GetHitOnlyTemplate(); // 모양 프리팹이 없으면 보이지 않는 판정만

        for (int i = 0; i < count; i++)
        {
            var obj = ObjectPool.Get(prefab, Owner.position, Quaternion.identity);
            if (obj.TryGetComponent<OrbitBlade>(out var blade)) blade.Setup(stat.damage, stat.tickInterval, hitRadius);
            blades.Add(obj.transform);
        }
    }

    protected override void Update()
    {
        base.Update();
        if (Data == null || blades.Count == 0) return;

        var stat = Stat;
        angle = (angle + stat.speed * Time.deltaTime) % 360f;
        float step = 360f / blades.Count; // 균등 배치

        Vector3 center = Owner.position + Vector3.up * heightOffset;
        for (int i = 0; i < blades.Count; i++)
        {
            Vector3 radial = Quaternion.AngleAxis(angle + step * i, Vector3.up) * Vector3.forward;
            blades[i].position = center + radial * stat.area;
        }
    }

    void ReleaseAll()
    {
        for (int i = 0; i < blades.Count; i++) ObjectPool.Release(blades[i].gameObject);
        blades.Clear();
    }

    void OnDestroy() => ReleaseAll(); // 진화로 교체될 때 정리

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
