using UnityEngine;

// 적이 죽으면 그 자리에 경험치 구슬 드롭
public class ExpOrbSpawner : MonoBehaviour
{
    [SerializeField] GameObject orbPrefab;
    [SerializeField] float orbY = 0.5f;

    void OnEnable() => Enemy.OnAnyDied += HandleEnemyDied;
    void OnDisable() => Enemy.OnAnyDied -= HandleEnemyDied;

    void HandleEnemyDied(Enemy enemy)
    {
        if (orbPrefab == null || enemy.Data == null) return;

        Vector3 pos = enemy.transform.position;
        pos.y = orbY;
        var obj = ObjectPool.Get(orbPrefab, pos, Quaternion.identity);
        obj.GetComponent<ExpOrb>().Init(enemy.Data.expReward);
    }
}
