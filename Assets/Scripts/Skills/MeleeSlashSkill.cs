using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 기본 공격(근접 베기): 조준 방향(마우스 클릭 방향, 없으면 가장 가까운 적)으로 부채꼴 범위를 벤다.
// 수치: count=베기 횟수, area=사거리(반경), speed=부채꼴 각도(도), damage=1회 피해, cooldown=공격 간격
// 최대 레벨에서는 SkillData.secondaryPrefab(Multiple Slashes)을 쓰고, 한 이펙트에서 count번 연속 타격한다.
public class MeleeSlashSkill : SkillBase
{
    [SerializeField] float fanAngle = 30f;       // 베기가 여러 번일 때 서로 벌어지는 각도
    [SerializeField] float hitDelay = 0.1f;      // 연속 베기 사이 간격
    [SerializeField] float closeRange = 0.8f;    // 이 거리 안의 적은 각도와 상관없이 맞음
    [SerializeField] float vfxGroundY = 0.05f;   // 이펙트가 놓이는 높이 (발 위치)
    [SerializeField] float vfxRefDiameter = 5f;  // 슬래시 이펙트의 원본 지름 추정치 (클수록 이펙트가 작게 나옴)
    [SerializeField] float vfxLifetime = 1.2f;
    [SerializeField] float vfxYawOffset = 0f;    // 이펙트가 옆으로 틀어져 보이면 90, -90 등으로 보정

    static readonly Dictionary<GameObject, GameObject> wrappers = new Dictionary<GameObject, GameObject>();
    readonly List<Transform> hits = new List<Transform>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() => wrappers.Clear();

    protected override void Activate()
    {
        var stat = Stat;
        Vector3 origin = Owner.position;

        // 마우스 클릭 중이면 클릭 방향(+ 무리 보정), 아니면 사거리 안의 가장 가까운 적 방향
        Vector3 dir;
        if (!PlayerAim.TryGetAimDirection(origin, stat.area, out dir))
        {
            var target = TargetFinder.FindNearest(origin, stat.area);
            if (target == null) return;

            dir = target.position - origin;
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.0001f) return;
            dir.Normalize();
        }

        PlayCastSound();
        StartCoroutine(Swing(dir, stat));
    }

    IEnumerator Swing(Vector3 dir, SkillLevelStat stat)
    {
        int count = Mathf.Max(1, stat.count);
        bool multi = IsMaxLevel && Data.secondaryPrefab != null;

        if (multi) SpawnVfx(Data.secondaryPrefab, dir, stat.area); // 이펙트 하나로 여러 번 타격

        for (int i = 0; i < count; i++)
        {
            Vector3 d = multi ? dir : Quaternion.AngleAxis((i - (count - 1) * 0.5f) * fanAngle, Vector3.up) * dir;
            if (!multi) SpawnVfx(Data.prefab, d, stat.area);

            DealDamage(d, stat);
            if (i < count - 1) yield return new WaitForSeconds(hitDelay);
        }
    }

    void DealDamage(Vector3 dir, SkillLevelStat stat)
    {
        Vector3 origin = Owner.position; // 연속 베기 중 이동했어도 현재 위치 기준
        float half = stat.speed * 0.5f;
        float closeSqr = closeRange * closeRange;

        TargetFinder.FindAll(origin, stat.area, hits);
        for (int i = 0; i < hits.Count; i++)
        {
            Vector3 to = hits[i].position - origin;
            to.y = 0f;
            bool inSector = to.sqrMagnitude <= closeSqr || Vector3.Angle(dir, to) <= half;
            if (inSector && hits[i].TryGetComponent<IDamageable>(out var target)) target.TakeDamage(stat.damage);
        }
    }

    void SpawnVfx(GameObject fx, Vector3 dir, float reach)
    {
        if (fx == null) return;
        Vector3 pos = Owner.position;
        pos.y = vfxGroundY;
        var obj = ObjectPool.Get(GetWrapper(fx), pos, Quaternion.LookRotation(dir) * Quaternion.Euler(0f, vfxYawOffset, 0f));
        obj.GetComponent<PooledVfx>().Play(reach * 2f, 0f);
    }

    // 원본 이펙트 프리팹을 풀 호환 래퍼(PooledVfx)로 감싼 템플릿 (프리팹 에셋을 따로 만들 필요 없음)
    GameObject GetWrapper(GameObject fx)
    {
        if (wrappers.TryGetValue(fx, out var wrapper) && wrapper != null) return wrapper;

        wrapper = new GameObject($"VfxWrapper_{fx.name}");
        wrapper.hideFlags = HideFlags.HideInHierarchy;
        wrapper.SetActive(false); // 풀의 원본은 비활성 상태로 둠
        wrapper.AddComponent<PooledVfx>().Configure(vfxRefDiameter, vfxLifetime);

        var child = Instantiate(fx, wrapper.transform);
        child.transform.localPosition = Vector3.zero;
        child.transform.localRotation = Quaternion.identity;

        wrappers[fx] = wrapper;
        return wrapper;
    }
}
