using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 헬파이어: 적이 가장 밀집한 곳에 경고 원 → 폭발. 레벨에 따라 동시에 여러 지점.
public class ExplosionSkill : SkillBase
{
    [SerializeField] float searchRadius = 15f;   // 플레이어 기준 탐색 반경
    [SerializeField] float clusterRadius = 3f;   // 밀집도 계산 반경
    [SerializeField] float warnTime = 0.5f;
    [SerializeField] float groundY = 0.05f;      // 경고 원 높이

    readonly List<Vector3> points = new List<Vector3>();
    readonly List<Transform> hitBuffer = new List<Transform>();

    protected override void Activate()
    {
        var stat = Stat;
        points.Clear();

        int count = Mathf.Max(1, stat.count);
        for (int i = 0; i < count; i++)
        {
            if (!TargetFinder.FindDensestPoint(Owner.position, searchRadius, clusterRadius,
                    points, stat.area, out var point)) break;
            points.Add(point);

            if (Data.secondaryPrefab != null)
            {
                var warn = ObjectPool.Get(Data.secondaryPrefab, new Vector3(point.x, groundY, point.z), Quaternion.identity);
                warn.GetComponent<IVfx>().Play(stat.area * 2f, warnTime);
            }
            StartCoroutine(Explode(point, stat.damage, stat.area, stat.duration));
        }
    }

    IEnumerator Explode(Vector3 point, float damage, float radius, float vfxDuration)
    {
        yield return new WaitForSeconds(warnTime);

        if (Data.prefab != null)
        {
            var vfx = ObjectPool.Get(Data.prefab, point, Quaternion.identity);
            vfx.GetComponent<IVfx>().Play(radius * 2f, vfxDuration);
        }

        TargetFinder.FindAll(point, radius, hitBuffer);
        for (int i = 0; i < hitBuffer.Count; i++)
            if (hitBuffer[i].TryGetComponent<IDamageable>(out var target)) target.TakeDamage(damage);
    }
}
