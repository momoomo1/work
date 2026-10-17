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

    public int KillCount => killCount;

    // 적 처치 시 발생 (처치된 적의 데이터 전달 → 경험치 등에 사용)
    public event Action<EnemyData> OnEnemyKilled;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot() { _ = Instance; }

    void Awake()
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;
    }

    void OnEnable() => Enemy.OnAnyDied += HandleEnemyDied;
    void OnDisable() => Enemy.OnAnyDied -= HandleEnemyDied;

    void OnDestroy()
    {
        if (instance == this) instance = null;
    }

    void HandleEnemyDied(Enemy enemy)
    {
        killCount++;
        OnEnemyKilled?.Invoke(enemy.Data);
    }
}
