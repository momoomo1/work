using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    static GameManager instance;
    // 씬에 없으면 자동 생성
    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<GameManager>();
                if (instance == null) instance = new GameObject("GameManager").AddComponent<GameManager>();
            }
            return instance;
        }
    }

    [SerializeField] int killCount;

    [Header("보스")]
    [SerializeField] int bossKillThreshold = 5; // 첫 보스가 나오는 처치 수
    [SerializeField] BossData bossData;

    [Header("무한 웨이브")]
    [SerializeField] int killsPerBoss = 200;           // 보스를 잡은 뒤 다음 보스까지 필요한 처치 수
    [SerializeField] float bossHealthGrowth = 1.6f;    // 보스가 나올 때마다 체력 배율 (곱연산)
    [SerializeField] float bossDamageGrowth = 1.15f;   // 보스 피해량 배율 (곱연산)
    [SerializeField] float enemyHealthGrowth = 0.2f;   // 웨이브가 오를 때마다 일반 적 체력 +20%
    [SerializeField] float enemyDamageGrowth = 0.1f;   // 일반 적 접촉 피해 +10%
    [SerializeField] float enemySpeedGrowth = 0.03f;   // 일반 적 이동 속도 +3% (최대 +60%)

    [Header("점수")]
    [SerializeField] int scorePerKill = 10;
    [SerializeField] int scorePerSecond = 2;     // 버틴 시간 1초당
    [SerializeField] int scorePerBoss = 1000;    // 보스 처치당

    [Header("시간이 갈수록 어려워짐 (가만히 서 있으면 못 버티게)")]
    [SerializeField] float timeDamagePerMinute = 0.3f;   // 1분마다 적 접촉 피해 +30%
    [SerializeField] float timeSpeedPerMinute = 0.025f;  // 1분마다 적 이동 속도 +2.5%
    [SerializeField] float maxSpeedMultiplier = 1.7f;    // 속도 배율 상한 (플레이어가 달리면 도망칠 수는 있게)
    [SerializeField] float timeHealthPerMinute = 0.08f;  // 1분마다 적 체력 +8%

    const string BestScoreKey = "BestScore";

    bool bossSpawned;
    int nextBossKill;
    int bossesDefeated;
    Health playerHealth;

    public int KillCount => killCount;
    public int Wave => bossesDefeated + 1;            // 보스를 잡을 때마다 웨이브가 오름
    public int BossesDefeated => bossesDefeated;
    float Minutes => SurvivalTime / 60f;
    public float EnemyHealthMultiplier => (1f + enemyHealthGrowth * bossesDefeated) * (1f + timeHealthPerMinute * Minutes);
    public float EnemyDamageMultiplier => (1f + enemyDamageGrowth * bossesDefeated) * (1f + timeDamagePerMinute * Minutes);
    public float EnemySpeedMultiplier => Mathf.Min(maxSpeedMultiplier, (1f + Mathf.Min(0.6f, enemySpeedGrowth * bossesDefeated)) * (1f + timeSpeedPerMinute * Minutes));

    // ---- 점수 ----
    public int Score => killCount * scorePerKill + Mathf.FloorToInt(SurvivalTime) * scorePerSecond + bossesDefeated * scorePerBoss;
    public int BestScore { get; private set; }                       // 저장된 최고 기록
    public int DisplayBestScore => Mathf.Max(BestScore, Score);       // 지금 점수가 넘으면 실시간으로 최고 기록이 됨
    public bool IsNewRecord { get; private set; }                    // 이번 판에서 최고 기록을 갱신했는지
    public bool BossAlive { get; private set; }
    public bool IsGameOver { get; private set; }
    public float SurvivalTime { get; private set; } // 게임이 멈춰 있는 동안은 증가하지 않음

    public event Action OnPlayerDied;

    public static string FormatTime(float seconds)
    {
        int s = Mathf.FloorToInt(seconds);
        return $"{s / 60:00}:{s % 60:00}";
    }

    // 적 처치 시 발생 (처치된 적의 데이터 전달 → 경험치 등에 사용)
    public event Action<EnemyData> OnEnemyKilled;
    public event Action<Boss> OnBossSpawned;
    public event Action OnBossDefeated; // 게임(스테이지) 클리어

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot() { _ = Instance; }

    void Awake()
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;
        nextBossKill = bossKillThreshold;
        BestScore = PlayerPrefs.GetInt(BestScoreKey, 0);
        Time.timeScale = 1f; // 씬 재로드 시 정지 상태가 남지 않게
    }

    void Start()
    {
        var player = PlayerLocator.Player;
        if (player != null && player.TryGetComponent(out playerHealth)) playerHealth.OnDeath += HandlePlayerDied;
    }

    void Update()
    {
        if (!IsGameOver) SurvivalTime += Time.deltaTime;
    }

    // 최고 기록을 넘었으면 저장 (게임오버 또는 게임 종료 시)
    void CommitBestScore()
    {
        int score = Score;
        if (score > BestScore)
        {
            BestScore = score;
            IsNewRecord = true;
            PlayerPrefs.SetInt(BestScoreKey, BestScore);
            PlayerPrefs.Save();
        }
    }

    void OnApplicationQuit()
    {
        if (!IsGameOver) CommitBestScore(); // 도중에 종료해도 기록은 남김
    }

    void HandlePlayerDied()
    {
        if (IsGameOver) return;
        IsGameOver = true;
        CommitBestScore();
        Time.timeScale = 0f;
        Debug.Log($"[GameOver] 생존 {FormatTime(SurvivalTime)}, 처치 {killCount}, 점수 {Score} (최고 {BestScore})");
        OnPlayerDied?.Invoke();
    }

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

    void OnDestroy()
    {
        if (playerHealth != null) playerHealth.OnDeath -= HandlePlayerDied;
        if (instance == this) instance = null;
    }

    void HandleEnemyDied(Enemy enemy)
    {
        killCount++;
        OnEnemyKilled?.Invoke(enemy.Data);

        if (!bossSpawned && bossData != null && killCount >= nextBossKill) SpawnBoss();
    }

    void SpawnBoss()
    {
        var player = PlayerLocator.Player;
        if (player == null || bossData.prefab == null) return;

        bossSpawned = true;
        float angle = UnityEngine.Random.value * Mathf.PI * 2f;
        Vector3 pos = player.position + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * bossData.spawnRadius;
        pos.y = bossData.spawnY;

        var obj = ObjectPool.Get(bossData.prefab, pos, Quaternion.identity);
        var boss = obj.GetComponent<Boss>();
        // 보스가 나올 때마다 체력/피해량 증가 (모양은 동일)
        boss.Init(bossData, Mathf.Pow(bossHealthGrowth, bossesDefeated), Mathf.Pow(bossDamageGrowth, bossesDefeated));
        BossAlive = true;
        Debug.Log($"[Boss] 웨이브 {Wave} 보스 등장! (체력 x{Mathf.Pow(bossHealthGrowth, bossesDefeated):0.0})");
        OnBossSpawned?.Invoke(boss);
    }

    void HandleBossDied(Boss boss)
    {
        BossAlive = false;
        bossesDefeated++;
        bossSpawned = false;                       // 다음 보스를 다시 소환할 수 있게
        nextBossKill = killCount + killsPerBoss;
        Debug.Log($"[Boss] 보스 처치! 웨이브 {bossesDefeated} 클리어 → 웨이브 {Wave} 시작 (다음 보스: 처치 {nextBossKill})");
        OnBossDefeated?.Invoke();
    }
}
