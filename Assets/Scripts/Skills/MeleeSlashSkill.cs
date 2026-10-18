using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 기본 공격(근접 베기): 조준 방향(마우스 클릭 방향, 없으면 가장 가까운 적)으로 부채꼴 범위를 벤다.
// 수치: count=베기 횟수, area=사거리(반경), speed=부채꼴 각도(도), damage=1회 피해, cooldown=공격 간격
// 최대 레벨에서는 SkillData.secondaryPrefab(Multiple Slashes)을 쓰고, 한 이펙트에서 count번 연속 타격한다.
// 이펙트 높이/방향은 SkillData의 vfxHeight / vfxYawOffset으로 조절하고, 방향은 처음 몇 번 자동으로 보정한다.
public class MeleeSlashSkill : SkillBase
{
    [SerializeField] float fanAngle = 30f;       // 베기가 여러 번일 때 서로 벌어지는 각도
    [SerializeField] float hitDelay = 0.1f;      // 연속 베기 사이 간격
    [SerializeField] float closeRange = 0.8f;    // 이 거리 안의 적은 각도와 상관없이 맞음
    [SerializeField] float autoEngageRange = 8f; // 자동 공격 때 이 거리 안의 가장 가까운 적을 향해 휘두름
    [SerializeField] bool swingWhenIdle = true;  // 적이 없어도 바라보는 방향으로 계속 휘두름 (끄면 적이 있을 때만)
    [SerializeField] float vfxRefDiameter = 5f;  // 슬래시 이펙트의 원본 지름 추정치 (클수록 이펙트가 작게 나옴)
    [SerializeField] float vfxLifetime = 1.2f;

    static readonly Dictionary<GameObject, GameObject> wrappers = new Dictionary<GameObject, GameObject>();
    static readonly Dictionary<GameObject, float> autoYaw = new Dictionary<GameObject, float>();   // 자동 보정된 방향
    static readonly HashSet<GameObject> calibrated = new HashSet<GameObject>();
    static readonly Dictionary<GameObject, int> attempts = new Dictionary<GameObject, int>();
    readonly List<Transform> hits = new List<Transform>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        wrappers.Clear();
        autoYaw.Clear();
        calibrated.Clear();
        attempts.Clear();
    }

    protected override void Activate()
    {
        var stat = Stat;
        Vector3 origin = Owner.position;

        // 마우스 클릭 중이면 클릭 방향(+ 무리 보정), 아니면 자동 공격:
        // 가까운 적(autoEngageRange 안)이 있으면 그쪽, 없으면 바라보는 방향으로 계속 휘두름
        Vector3 dir;
        if (!PlayerAim.TryGetAimDirection(origin, stat.area, out dir))
        {
            var target = TargetFinder.FindNearest(origin, Mathf.Max(stat.area, autoEngageRange));
            if (target != null)
            {
                dir = target.position - origin;
                dir.y = 0f;
            }
            else if (swingWhenIdle) dir = Owner.forward;
            else return;

            dir.y = 0f;
            if (dir.sqrMagnitude < 0.0001f) dir = Vector3.forward;
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
        Vector3 pos = Owner.position + dir.normalized * Data.vfxForwardOffset; // 플레이어에서 떨어진 곳에 띄움
        pos.y = Data.vfxHeight;

        autoYaw.TryGetValue(fx, out float auto);
        var obj = ObjectPool.Get(GetWrapper(fx), pos, Quaternion.LookRotation(dir) * Quaternion.Euler(0f, auto + Data.vfxYawOffset, 0f));
        obj.GetComponent<PooledVfx>().Play(reach * 2f, 0f);

        if (!calibrated.Contains(fx)) StartCoroutine(Calibrate(fx, obj, pos, dir));
    }

    // 이펙트가 실제로 뻗어 나가는 쪽을 재서, 조준 방향과 어긋나 있으면 90도 단위로 보정 (한 번 맞추면 이후 계속 적용)
    IEnumerator Calibrate(GameObject fx, GameObject obj, Vector3 origin, Vector3 dir)
    {
        yield return new WaitForSeconds(0.08f);
        if (obj == null || !obj.activeInHierarchy) yield break;

        attempts.TryGetValue(fx, out int tried);
        if (tried >= 8) { calibrated.Add(fx); yield break; }
        attempts[fx] = tried + 1;

        // 살아 있는 메시 파티클(칼날 모양)의 월드 영역 중심
        bool any = false;
        Bounds b = default;
        foreach (var r in obj.GetComponentsInChildren<ParticleSystemRenderer>())
        {
            if (r.renderMode != ParticleSystemRenderMode.Mesh) continue;
            var ps = r.GetComponent<ParticleSystem>();
            if (ps == null || ps.particleCount == 0) continue;
            if (!any) { b = r.bounds; any = true; } else b.Encapsulate(r.bounds);
        }
        if (!any) yield break;

        Vector3 v = b.center - origin;
        v.y = 0f;
        if (v.magnitude < 0.4f) yield break; // 중심이 거의 같으면 판단할 수 없음 → 다음 번에 다시 시도

        float angle = Vector3.SignedAngle(v, dir, Vector3.up);
        float snapped = Mathf.Round(angle / 90f) * 90f;

        autoYaw.TryGetValue(fx, out float auto);
        autoYaw[fx] = auto + snapped;
        calibrated.Add(fx);

        if (!Mathf.Approximately(snapped, 0f))
            obj.transform.rotation = Quaternion.AngleAxis(snapped, Vector3.up) * obj.transform.rotation; // 이번 이펙트도 바로 보정
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
