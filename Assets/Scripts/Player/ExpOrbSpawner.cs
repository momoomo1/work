using UnityEngine;

// 적이 죽으면 그 자리에 경험치 구슬 드롭. 보스는 대량 드롭.
public class ExpOrbSpawner : MonoBehaviour
{
    [SerializeField] GameObject orbPrefab;
    [SerializeField] float orbY = 0.5f;
    [SerializeField] float bossScatterRadius = 3f;
    [SerializeField] int maxOrbs = 220;            // 동시에 떠 있을 수 있는 구슬 수

    void OnEnable()
    {
        Enemy.OnAnyDied += HandleEnemyDied;
        Boss.OnBossDied += HandleBossDied;
    }

    void OnDisable()
    {
        Enemy.OnAnyDied -= HandleEnemyDied;
        Boss.OnBossDied -= HandleBossDied;
    }

    void HandleEnemyDied(Enemy enemy)
    {
        if (orbPrefab == null || enemy.Data == null) return;
        Drop(enemy.transform.position, enemy.Data.expReward);
    }

    void HandleBossDied(Boss boss)
    {
        if (orbPrefab == null || boss.Data == null) return;
        int n = Mathf.Max(1, boss.Data.orbCount);
        int each = Mathf.Max(1, boss.Data.expReward / n);
        for (int i = 0; i < n; i++)
        {
            Vector2 offset = Random.insideUnitCircle * bossScatterRadius;
            Drop(boss.transform.position + new Vector3(offset.x, 0f, offset.y), each);
        }
    }

    void Drop(Vector3 pos, int value)
    {
        // 구슬이 너무 많이 쌓이면 줍지 않아도 바로 경험치로 지급 (성능)
        if (ExpOrb.ActiveCount >= maxOrbs)
        {
            PlayerExperience.Current?.AddExp(value);
            return;
        }
        pos.y = orbY;
        ObjectPool.Get(orbPrefab, pos, Quaternion.identity).GetComponent<ExpOrb>().Init(value);
    }
}
