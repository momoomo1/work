using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

// 마우스 조준: 왼쪽 버튼을 누르고 있는 동안만 조준 방향을 제공한다.
// 커서 방향 부채꼴(ConeAngle) 안에 적이 있으면 그 중 가장 밀집한 곳을 향하게 보정하고,
// 부채꼴 안에 적이 없어도 사거리 안에 적이 있으면 가장 밀집한 곳을 향한다.
// 비슷한 무리끼리는 커서 방향에 가까운 쪽을 고른다.
// 사거리 안에 적이 전혀 없을 때만 커서 방향 그대로 쏜다.
// 누르지 않으면 false를 반환하므로 호출하는 쪽에서 자동 타겟팅으로 대체하면 된다.
public static class PlayerAim
{
    public static float ConeAngle = 60f;     // 커서 방향 기준 좌우 각도 (이 안의 적을 우선)
    public static float ClusterRadius = 3f;  // 이 반경 안의 적끼리를 한 무리로 봄
    public static float AngleWeight = 60f;   // 이 각도(도)만큼 벗어나면 이웃 1마리만큼 손해로 계산 (작을수록 커서 방향을 중시)

    static Camera cam;
    static readonly List<Transform> found = new List<Transform>();
    static readonly List<Vector3> positions = new List<Vector3>();
    static readonly List<Vector3> subset = new List<Vector3>();

    // 클릭 여부와 상관없이 커서가 가리키는 지면 방향 (대쉬 방향 등에 사용)
    public static bool TryGetCursorDirection(Vector3 origin, out Vector3 direction)
    {
        direction = Vector3.zero;
        var mouse = Mouse.current;
        if (mouse == null) return false;
        if (cam == null) cam = Camera.main;
        if (cam == null) return false;

        Ray ray = cam.ScreenPointToRay(mouse.position.ReadValue());
        var plane = new Plane(Vector3.up, new Vector3(0f, origin.y, 0f));
        if (!plane.Raycast(ray, out float dist)) return false;

        Vector3 d = ray.GetPoint(dist) - origin;
        d.y = 0f;
        if (d.sqrMagnitude < 0.01f) return false;
        direction = d.normalized;
        return true;
    }
    public static bool TryGetAimDirection(Vector3 origin, float range, out Vector3 direction)
    {
        direction = Vector3.zero;

        var mouse = Mouse.current;
        if (mouse == null || !mouse.leftButton.isPressed) return false;
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return false; // UI 위 클릭 무시

        if (cam == null) cam = Camera.main;
        if (cam == null) return false;

        Ray ray = cam.ScreenPointToRay(mouse.position.ReadValue());
        var plane = new Plane(Vector3.up, new Vector3(0f, origin.y, 0f)); // 플레이어 높이의 수평면
        if (!plane.Raycast(ray, out float dist)) return false;

        Vector3 aim = ray.GetPoint(dist) - origin;
        aim.y = 0f;
        bool hasAim = aim.sqrMagnitude >= 0.01f;
        aim = hasAim ? aim.normalized : Vector3.zero;

        // 사거리 안의 적 위치 수집
        positions.Clear();
        if (TargetFinder.FindAll(origin, range, found) > 0)
        {
            foreach (var t in found)
            {
                Vector3 p = t.position;
                p.y = origin.y;
                positions.Add(p);
            }
        }

        if (positions.Count > 0)
        {
            // 커서 방향 부채꼴 안의 적 우선, 없으면 사거리 안 전체
            subset.Clear();
            if (hasAim)
            {
                float cos = Mathf.Cos(ConeAngle * Mathf.Deg2Rad);
                foreach (var p in positions)
                {
                    Vector3 to = p - origin;
                    if (to.sqrMagnitude > 0.0001f && Vector3.Dot(to.normalized, aim) >= cos) subset.Add(p);
                }
            }
            var pool = subset.Count > 0 ? subset : positions;

            if (TryFindCluster(pool, origin, aim, hasAim, out Vector3 center))
            {
                Vector3 to = center - origin;
                to.y = 0f;
                if (to.sqrMagnitude > 0.09f)
                {
                    direction = to.normalized;
                    return true;
                }
            }
        }

        if (!hasAim) return false;
        direction = aim; // 사거리 안에 적이 없으면 커서 방향 그대로
        return true;
    }

    // 이웃이 많고 커서 방향에 가까운 적을 골라 그 무리의 평균 위치를 반환
    // 점수 = 이웃 수 - (커서 방향과 벌어진 각도 / AngleWeight)
    static bool TryFindCluster(List<Vector3> pool, Vector3 origin, Vector3 aim, bool hasAim, out Vector3 center)
    {
        center = Vector3.zero;
        if (pool.Count == 0) return false;

        float sqr = ClusterRadius * ClusterRadius;
        float bestScore = float.MinValue;
        bool found = false;
        for (int i = 0; i < pool.Count; i++)
        {
            Vector3 sum = Vector3.zero;
            int n = 0;
            for (int j = 0; j < pool.Count; j++)
            {
                if ((pool[j] - pool[i]).sqrMagnitude <= sqr) { sum += pool[j]; n++; }
            }

            Vector3 c = sum / n;
            float angle = hasAim ? Vector3.Angle(aim, c - origin) : 0f;
            float score = n - angle / Mathf.Max(1f, AngleWeight);
            if (score > bestScore) { bestScore = score; center = c; found = true; }
        }
        return found;
    }
}
