using System.Collections.Generic;
using UnityEngine;

// 서리 구름: 적이 밀집한 곳(없으면 플레이어 발밑)에 장판 생성
// 수치: count=장판 수, area=반경, speed=슬로우 비율(0~1), duration=장판 지속시간, tickInterval=틱 간격
public class ZoneSkill : SkillBase
{
    [SerializeField] float searchRadius = 12f;
    [SerializeField] float clusterRadius = 3f;
    [SerializeField] float groundY = 0.4f;   // 장판 높이 (볼록한 바닥에 가려지지 않게 위로 띄움)

    readonly List<Vector3> points = new List<Vector3>();

    protected override void Activate()
    {
        var stat = Stat;
        points.Clear();

        int count = Mathf.Max(1, stat.count);
        for (int i = 0; i < count; i++)
        {
            Vector3 point;
            if (!TargetFinder.FindDensestPoint(Owner.position, searchRadius, clusterRadius, points, stat.area, out point))
            {
                if (i > 0) break;
                point = Owner.position; // 적이 없으면 플레이어 주변
            }
            points.Add(point);

            var obj = ObjectPool.Get(Data.prefab, new Vector3(point.x, groundY, point.z), Quaternion.identity);
            PlayCastSound();
            obj.GetComponent<FrostZone>().Begin(stat.area, stat.damage, stat.tickInterval, stat.duration, stat.speed);
        }
    }
}
