using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] EnemyData[] enemyTypes;
    [SerializeField] Transform player;                 // 비어있으면 "Player" 태그로 검색
    [SerializeField] float spawnRadius = 18f;
    [SerializeField] float spawnY = 1.1f;
    [SerializeField] int maxAlive = 60;

    [Header("난이도 곡선")]
    [SerializeField] float startInterval = 1.5f;       // 시작 스폰 간격(초)
    [SerializeField] float minInterval = 0.2f;
    [SerializeField] float intervalDecayPerSecond = 0.01f; // 경과 1초당 줄어드는 간격

    float elapsed;
    float timer;

    public float CurrentInterval => Mathf.Max(minInterval, startInterval - elapsed * intervalDecayPerSecond);

    void Start()
    {
        _ = GameManager.Instance; // 처치 집계 구독 보장
        if (player == null)
        {
            var go = GameObject.FindGameObjectWithTag("Player");
            if (go != null) player = go.transform;
        }
    }

    void Update()
    {
        elapsed += Time.deltaTime;
        if (player == null || enemyTypes == null || enemyTypes.Length == 0) return;

        timer += Time.deltaTime;
        float interval = CurrentInterval;
        while (timer >= interval)
        {
            timer -= interval;
            if (Enemy.ActiveCount >= maxAlive) { timer = 0f; break; }
            Spawn();
        }
    }

    void Spawn()
    {
        var data = enemyTypes[Random.Range(0, enemyTypes.Length)];
        if (data == null || data.prefab == null) return;

        float angle = Random.value * Mathf.PI * 2f;
        Vector3 pos = player.position + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * spawnRadius;
        pos.y = spawnY;

        var obj = ObjectPool.Get(data.prefab, pos, Quaternion.identity);
        obj.GetComponent<Enemy>().Init(data);
    }
}
