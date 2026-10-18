using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] EnemyData[] enemyTypes;
    [SerializeField] Transform player;                 // 비어있으면 "Player" 태그로 검색
    [SerializeField] float spawnRadius = 18f;
    [SerializeField] float spawnY = 1.1f;
    [SerializeField] int maxAlive = 60;
    [SerializeField] int maxAlivePerWave = 8;   // 웨이브마다 늘어나는 동시 적 수
    [SerializeField] int maxAliveCap = 160;     // 상한 (성능)

    [Header("난이도 곡선")]
    [SerializeField] float startInterval = 1.5f;       // 시작 스폰 간격(초)
    [SerializeField] float minInterval = 0.2f;
    [SerializeField] float intervalDecayPerSecond = 0.01f; // 경과 1초당 줄어드는 간격

    [Header("보스전")]
    [SerializeField, Range(0f, 1f)] float spawnRateDuringBoss = 0f; // 0 = 보스 생존 중 스폰 중지

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

        // 보스 생존 중에는 스폰 속도를 줄이거나(0~1) 멈춤(0)
        float rate = GameManager.Instance.BossAlive ? spawnRateDuringBoss : 1f;
        if (rate <= 0f) return;

        timer += Time.deltaTime * rate;
        float interval = CurrentInterval;
        while (timer >= interval)
        {
            timer -= interval;
            int cap = Mathf.Min(maxAliveCap, maxAlive + maxAlivePerWave * (GameManager.Instance.Wave - 1)); // 웨이브가 오를수록 동시 적 수 증가
            if (Enemy.ActiveCount >= cap) { timer = 0f; break; }
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
        var gm = GameManager.Instance;
        // 웨이브가 오를수록 일반 적도 강해짐
        obj.GetComponent<Enemy>().Init(data, gm.EnemyHealthMultiplier, gm.EnemyDamageMultiplier, gm.EnemySpeedMultiplier);
    }
}
