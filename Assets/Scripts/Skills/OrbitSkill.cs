using System.Collections.Generic;
using UnityEngine;

// 회전 칼날: 플레이어 주변을 도는 칼날 N개를 유지하는 지속형 스킬
// 수치: count=칼날 수, area=회전 반경, speed=각속도(도/초), tickInterval=같은 적 재타격 간격
public class OrbitSkill : SkillBase
{
    readonly List<Transform> blades = new List<Transform>();
    float angle;

    protected override void OnInit() => Rebuild();
    protected override void OnLevelUp() => Rebuild();

    // 지속형이라 쿨타임 동작 없음
    protected override void Activate() { }

    void Rebuild()
    {
        ReleaseAll();
        var stat = Stat;
        int count = Mathf.Max(1, stat.count);
        for (int i = 0; i < count; i++)
        {
            var obj = ObjectPool.Get(Data.prefab, Owner.position, Quaternion.identity);
            obj.GetComponent<OrbitBlade>().Setup(stat.damage, stat.tickInterval);
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

        Vector3 center = Owner.position;
        for (int i = 0; i < blades.Count; i++)
        {
            Vector3 radial = Quaternion.AngleAxis(angle + step * i, Vector3.up) * Vector3.forward;
            blades[i].SetPositionAndRotation(center + radial * stat.area,
                Quaternion.LookRotation(Vector3.Cross(Vector3.up, radial), Vector3.up));
        }
    }

    void ReleaseAll()
    {
        for (int i = 0; i < blades.Count; i++) ObjectPool.Release(blades[i].gameObject);
        blades.Clear();
    }

    void OnDestroy() => ReleaseAll(); // 진화로 교체될 때 칼날 정리
}
