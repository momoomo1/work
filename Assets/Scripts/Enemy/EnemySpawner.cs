using UnityEngine;

// 뱀서식 적 스폰: 한 번에 여러 마리씩 무리 지어 나오고, 시간이 갈수록 무리가 커지며,
// 주기적으로 플레이어를 빙 둘러싸는 "대규모 습격(Swarm)"이 온다.
public class EnemySpawner : MonoBehaviour
{
    [SerializeField] EnemyData[] enemyTypes;
    [SerializeField] Transform player;                 // 비어있으면 "Player" 태그로 검색
    [SerializeField] float spawnRadius = 18f;          // 화면 밖에서 나타나도록 카메라 시야보다 먼 거리
    [SerializeField] float radiusJitter = 4f;          // 반경에 더해지는 무작위 거리
    [SerializeField] float spawnY = 1.1f;
    [SerializeField] int maxAlive = 150;
    [SerializeField] int maxAlivePerWave = 20;         // 웨이브마다 늘어나는 동시 적 수
    [SerializeField] int maxAliveCap = 250;            // 상한 (성능). 컴퓨터가 버티면 올리세요
    [SerializeField] int prewarmCount = 150;            // 시작할 때 미리 만들어 둘 적 수 (처음 몰려올 때 끊김 방지)

    [Header("난이도 곡선")]
    [SerializeField] float startInterval = 1f;         // 시작 스폰 간격(초)
    [SerializeField] float minInterval = 0.35f;
    [SerializeField] float intervalDecayPerSecond = 0.004f; // 경과 1초당 줄어드는 간격

    [Header("무리 (한 번에 나오는 마리 수)")]
    [SerializeField] int groupStart = 8;
    [SerializeField] int groupMax = 48;
    [SerializeField] float groupGrowSeconds = 15f;     // 이 시간마다 무리가 1마리씩 커짐
    [SerializeField] int groupPerWave = 4;             // 웨이브마다 추가되는 무리 크기
    [SerializeField] float groupArc = 40f;             // 한 무리가 퍼지는 각도(도)

    [Header("대규모 습격 (플레이어를 빙 둘러싸고 몰려옴)")]
    [SerializeField] float swarmInterval = 30f;        // 0이면 사용 안 함
    [SerializeField] int swarmBaseCount = 100;
    [SerializeField] int swarmPerWave = 30;
    [SerializeField] float swarmMinInterval = 18f;     // 시간이 갈수록 습격 주기가 이 값까지 짧아짐
    [SerializeField] float swarmSpeedupPerSecond = 0.015f;

    [Header("보스전")]
    [SerializeField, Range(0f, 1f)] float spawnRateDuringBoss = 0.4f; // 0 = 보스 생존 중 스폰 중지

    float elapsed;
    float timer;
    float swarmTimer;

    public float CurrentInterval => Mathf.Max(minInterval, startInterval - elapsed * intervalDecayPerSecond);

    void Start()
    {
        _ = GameManager.Instance; // 처치 집계 구독 보장
        if (player == null)
        {
            var go = GameObject.FindGameObjectWithTag("Player");
            if (go != null) player = go.transform;
        }

        // 미리 만들어 두기 (풀)
        if (enemyTypes != null && prewarmCount > 0)
        {
            foreach (var d in enemyTypes)
                if (d != null && d.prefab != null) ObjectPool.Instance.Prewarm(d.prefab, prewarmCount / Mathf.Max(1, enemyTypes.Length));
        }
    }

    void Update()
    {
        float dt = Time.deltaTime;
        elapsed += dt;
        if (player == null || enemyTypes == null || enemyTypes.Length == 0) return;

        var gm = GameManager.Instance;
        int wave = gm.Wave;
        int cap = Mathf.Min(maxAliveCap, maxAlive + maxAlivePerWave * (wave - 1)); // 웨이브가 오를수록 동시 적 수 증가

        // 보스 생존 중에는 스폰 속도를 줄이거나(0~1) 멈춤(0)
        float rate = gm.BossAlive ? spawnRateDuringBoss : 1f;
        if (rate <= 0f) return;

        // 일반 스폰: 무리 단위로
        timer += dt * rate;
        float interval = CurrentInterval;
        while (timer >= interval)
        {
            timer -= interval;
            int room = cap - Enemy.ActiveCount;
            if (room <= 0) { timer = 0f; break; }
            SpawnGroup(Mathf.Min(GroupSize(wave), room));
        }

        // 대규모 습격: 주기마다 플레이어 주위를 둘러싸고 나옴
        if (swarmInterval > 0f)
        {
            swarmTimer += dt * rate;
            float effectiveSwarm = Mathf.Max(swarmMinInterval, swarmInterval - elapsed * swarmSpeedupPerSecond);
            if (swarmTimer >= effectiveSwarm)
            {
                swarmTimer = 0f;
                int room = cap - Enemy.ActiveCount;
                int count = Mathf.Min(swarmBaseCount + swarmPerWave * (wave - 1), room);
                if (count > 0) SpawnRing(count);
            }
        }
    }

    int GroupSize(int wave)
    {
        int size = groupStart + Mathf.FloorToInt(elapsed / Mathf.Max(1f, groupGrowSeconds)) + groupPerWave * (wave - 1);
        return Mathf.Clamp(size, 1, groupMax);
    }

    // 한쪽 방향에서 부채꼴로 몰려오는 무리
    void SpawnGroup(int count)
    {
        float center = Random.value * 360f;
        for (int i = 0; i < count; i++)
            SpawnAt(center + Random.Range(-groupArc * 0.5f, groupArc * 0.5f), spawnRadius + Random.value * radiusJitter);
    }

    // 사방에서 동시에 몰려오는 고리
    void SpawnRing(int count)
    {
        float start = Random.value * 360f;
        for (int i = 0; i < count; i++)
            SpawnAt(start + 360f * i / count, spawnRadius + 2f + Random.value * radiusJitter);
    }

    void SpawnAt(float angleDeg, float radius)
    {
        var data = enemyTypes[Random.Range(0, enemyTypes.Length)];
        if (data == null || data.prefab == null) return;

        float a = angleDeg * Mathf.Deg2Rad;
        Vector3 pos = player.position + new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * radius;
        pos.y = spawnY * Mathf.Max(0.05f, data.scale); // 작은 적은 발이 바닥에 닿도록 높이도 같이 줄임

        var obj = ObjectPool.Get(data.prefab, pos, Quaternion.identity);
        var gm = GameManager.Instance;
        // 웨이브가 오를수록 일반 적도 강해짐
        obj.GetComponent<Enemy>().Init(data, gm.EnemyHealthMultiplier, gm.EnemyDamageMultiplier, gm.EnemySpeedMultiplier);
    }
}
