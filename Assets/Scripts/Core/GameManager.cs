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
    [SerializeField] int killsPerBoss = 40;            // 보스를 잡은 뒤 다음 보스까지 필요한 처치 수
    [SerializeField] float bossHealthGrowth = 1.6f;    // 보스가 나올 때마다 체력 배율 (곱연산)
    [SerializeField] float bossDamageGrowth = 1.15f;   // 보스 피해량 배율 (곱연산)
    [SerializeField] float enemyHealthGrowth = 0.2f;   // 웨이브가 오를 때마다 일반 적 체력 +20%
    [SerializeField] float enemyDamageGrowth = 0.1f;   // 일반 적 접촉 피해 +10%
    [SerializeField] float enemySpeedGrowth = 0.03f;   // 일반 적 이동 속도 +3% (최대 +60%)

    bool bossSpawned;
    int nextBossKill;
    int bossesDefeated;
    Health playerHealth;

    public int KillCount => killCount;
    public int Wave => bossesDefeated + 1;            // 보스를 잡을 때마다 웨이브가 오름
    public int BossesDefeated => bossesDefeated;
    public float EnemyHealthMultiplier => 1f + enemyHealthGrowth * bossesDefeated;
    public float EnemyDamageMultiplier => 1f + enemyDamageGrowth * bossesDefeated;
    public float EnemySpeedMultiplier => 1f + Mathf.Min(0.6f, enemySpeedGrowth * bossesDefeated);
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

    void HandlePlayerDied()
    {
        if (IsGameOver) return;
        IsGameOver = true;
        Time.timeScale = 0f;
        Debug.Log($"[GameOver] 생존 {FormatTime(SurvivalTime)}, 처치 {killCount}");
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
