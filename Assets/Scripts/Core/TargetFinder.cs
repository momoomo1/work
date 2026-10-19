using System.Collections.Generic;
using UnityEngine;

// "Enemy" 레이어 기준 탐색 유틸. 내부 버퍼를 재사용하므로 호출 사이에 결과를 보관하지 말 것.
public static class TargetFinder
{
    const int MaxHits = 512;
    const int MaxCandidates = 48; // 밀집 지점 탐색 시 비교할 후보 수 상한 (적이 수백 마리여도 n^2이 되지 않게)
    static readonly Collider[] buffer = new Collider[MaxHits];
    static int enemyMask = -1;

    public static int EnemyMask
    {
        get
        {
            if (enemyMask < 0)
            {
                enemyMask = LayerMask.GetMask("Enemy");
                if (enemyMask == 0) Debug.LogWarning("\"Enemy\" 레이어가 없습니다. Tools/Setup 메뉴를 실행하세요.");
            }
            return enemyMask;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() => enemyMask = -1;

    // 반경 안에서 가장 가까운 적 (없으면 null)
    public static Transform FindNearest(Vector3 center, float radius)
    {
        int n = Physics.OverlapSphereNonAlloc(center, radius, buffer, EnemyMask, QueryTriggerInteraction.Collide);
        Transform best = null;
        float bestSqr = float.MaxValue;
        for (int i = 0; i < n; i++)
        {
            float sqr = Flat(buffer[i].transform.position - center).sqrMagnitude;
            if (sqr < bestSqr) { bestSqr = sqr; best = buffer[i].transform; }
        }
        return best;
    }

    // 반경 안의 모든 적을 results에 채움 (results는 먼저 비워짐). 반환값은 개수.
    public static int FindAll(Vector3 center, float radius, List<Transform> results)
    {
        results.Clear();
        int n = Physics.OverlapSphereNonAlloc(center, radius, buffer, EnemyMask, QueryTriggerInteraction.Collide);
        for (int i = 0; i < n; i++) results.Add(buffer[i].transform);
        return results.Count;
    }

    // 반경 안에서 clusterRadius 이내 이웃이 가장 많은 지점(이웃들의 평균 위치). 적이 없으면 false.
    public static bool FindDensestPoint(Vector3 center, float radius, float clusterRadius, out Vector3 point)
        => FindDensestPoint(center, radius, clusterRadius, null, 0f, out point);

    // exclude의 각 지점에서 excludeRadius 이내의 적은 후보에서 제외 (여러 폭발을 서로 다른 곳에 두기 위함)
    public static bool FindDensestPoint(Vector3 center, float radius, float clusterRadius,
        List<Vector3> exclude, float excludeRadius, out Vector3 point)
    {
        int n = Physics.OverlapSphereNonAlloc(center, radius, buffer, EnemyMask, QueryTriggerInteraction.Collide);
        point = center;

        if (exclude != null && exclude.Count > 0)
        {
            float exSqr = excludeRadius * excludeRadius;
            int w = 0;
            for (int i = 0; i < n; i++)
            {
                bool skip = false;
                for (int e = 0; e < exclude.Count && !skip; e++)
                    skip = Flat(buffer[i].transform.position - exclude[e]).sqrMagnitude <= exSqr;
                if (!skip) buffer[w++] = buffer[i];
            }
            n = w;
        }
        if (n == 0) return false;

        float clusterSqr = clusterRadius * clusterRadius;
        int bestCount = 0;
        int stride = Mathf.Max(1, n / MaxCandidates); // 후보만 건너뛰며 고르고, 이웃 수는 전체를 대상으로 셈
        int start = n > MaxCandidates ? Random.Range(0, stride) : 0;
        for (int i = start; i < n; i += stride)
        {
            Vector3 pi = buffer[i].transform.position;
            Vector3 sum = Vector3.zero;
            int count = 0;
            for (int j = 0; j < n; j++)
            {
                Vector3 pj = buffer[j].transform.position;
                if (Flat(pj - pi).sqrMagnitude <= clusterSqr) { sum += pj; count++; }
            }
            if (count > bestCount) { bestCount = count; point = sum / count; }
        }
        return true;
    }

    static Vector3 Flat(Vector3 v) { v.y = 0f; return v; }
}
