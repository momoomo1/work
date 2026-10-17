using UnityEngine;

// 진공참: 수라참 강화형. 더 큰 투사체(area), 사실상 무제한 관통(pierceCount),
// 지나가는 적을 투사체 쪽으로 끌어당김.
public class VacuumSkill : ProjectileSkill
{
    [SerializeField] float pullRadiusScale = 2.5f; // 끌어당김 반경 = area × 이 값
    [SerializeField] float pullStrength = 6f;      // 초당 끌어당기는 거리

    protected override void OnLaunched(Projectile projectile, SkillLevelStat stat)
    {
        projectile.SetPull(stat.area * pullRadiusScale, pullStrength);
    }
}
