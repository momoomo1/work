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
    [SerializeField] int bossKillThreshold = 5; // 이 처치 수에 도달하면 보스를 한 번 소환
    [SerializeField] BossData bossData;

    bool bossSpawned;

    public int KillCount => killCount;
    public bool BossAlive { get; private set; }

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
        if (instance == this) instance = null;
    }

    void HandleEnemyDied(Enemy enemy)
    {
        killCount++;
        OnEnemyKilled?.Invoke(enemy.Data);

        if (!bossSpawned && bossData != null && killCount >= bossKillThreshold) SpawnBoss();
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
        boss.Init(bossData);
        BossAlive = true;
        Debug.Log("[Boss] 보스 등장!");
        OnBossSpawned?.Invoke(boss);
    }

    void HandleBossDied(Boss boss)
    {
        BossAlive = false;
        Debug.Log("[Boss] 보스 처치! STAGE CLEAR");
        OnBossDefeated?.Invoke();
    }
}
