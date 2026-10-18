using UnityEngine;

// 수라참/진공참: 가장 가까운 적 방향으로 투사체를 부채꼴로 발사
public class ProjectileSkill : SkillBase
{
    [SerializeField] float spreadAngle = 12f; // 투사체 사이 각도
    [SerializeField] float muzzleOffset = 0.6f;

    protected override void Activate()
    {
        var stat = Stat;
        Vector3 origin = Owner.position;

        // 탐색 반경 = 투사체 사거리(속도 × 수명)
        var target = TargetFinder.FindNearest(origin, stat.speed * stat.duration);
        if (target == null) return;

        Vector3 dir = target.position - origin;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return;
        dir.Normalize();

        PlayCastSound();
        int count = Mathf.Max(1, stat.count);
        for (int i = 0; i < count; i++)
        {
            float angle = (i - (count - 1) * 0.5f) * spreadAngle;
            Vector3 d = Quaternion.AngleAxis(angle, Vector3.up) * dir;

            var obj = ObjectPool.Get(Data.prefab, origin + d * muzzleOffset, Quaternion.LookRotation(d));
            var proj = obj.GetComponent<Projectile>();
            proj.Launch(d, stat.speed, stat.damage, stat.pierceCount, stat.duration, stat.area);
            OnLaunched(proj, stat);
        }
    }

    // 파생 스킬(진공참 등)이 투사체에 추가 효과를 줄 때 사용
    protected virtual void OnLaunched(Projectile projectile, SkillLevelStat stat) { }
}
