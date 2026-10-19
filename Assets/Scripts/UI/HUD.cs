using TMPro;
using UnityEngine;

// 체력/경험치/처치 수/생존 시간 + 보스 체력 바
public class HUD : MonoBehaviour
{
    [Header("플레이어")]
    [SerializeField] RectTransform hpFill;
    [SerializeField] TMP_Text hpText;
    [SerializeField] RectTransform expFill;
    [SerializeField] TMP_Text levelText;
    [Header("진행")]
    [SerializeField] TMP_Text killText;
    [SerializeField] TMP_Text timeText;
    [Header("보스")]
    [SerializeField] GameObject bossGroup;
    [SerializeField] RectTransform bossFill;
    [SerializeField] TMP_Text bossText;

    TMP_Text scoreText;
    int lastScore = -1;
    int lastBest = -1;
    GameManager gm;
    Health playerHealth;
    PlayerExperience exp;
    Health bossHealth;
    int lastSecond = -1;

    void Start()
    {
        bossGroup.SetActive(false);
        killText.textWrappingMode = TextWrappingModes.NoWrap; // "웨이브 n · 처치 m"이 길어져도 한 줄 유지

        CreateScoreText();

        gm = GameManager.Instance;
        gm.OnEnemyKilled += HandleKill;
        gm.OnBossSpawned += HandleBossSpawned;
        gm.OnBossDefeated += HideBoss;
        HandleKill(null);

        var player = PlayerLocator.Player;
        if (player != null)
        {
            playerHealth = player.GetComponent<Health>();
            exp = player.GetComponent<PlayerExperience>();
        }
        if (playerHealth != null)
        {
            playerHealth.OnChanged += UpdateHp;
            UpdateHp(playerHealth.CurrentHealth, playerHealth.MaxHealth);
        }
        if (exp != null)
        {
            exp.OnExpChanged += UpdateExp;
            UpdateExp(exp.CurrentExp, exp.ExpToNext);
        }
    }

    void OnDestroy()
    {
        if (gm != null)
        {
            gm.OnEnemyKilled -= HandleKill;
            gm.OnBossSpawned -= HandleBossSpawned;
            gm.OnBossDefeated -= HideBoss;
        }
        if (playerHealth != null) playerHealth.OnChanged -= UpdateHp;
        if (exp != null) exp.OnExpChanged -= UpdateExp;
        if (bossHealth != null) bossHealth.OnChanged -= UpdateBoss;
    }

    // 처치 수 글자를 복제해서 그 아래에 점수 줄을 만듦
    void CreateScoreText()
    {
        scoreText = Instantiate(killText, killText.transform.parent);
        scoreText.name = "ScoreText";
        var rt = (RectTransform)scoreText.transform;
        rt.pivot = new Vector2(1f, 0.5f);
        rt.sizeDelta = new Vector2(760f, rt.sizeDelta.y);
        rt.anchoredPosition = new Vector2(-20f, ((RectTransform)killText.transform).anchoredPosition.y - 46f);
        scoreText.fontSize = 26f;
        scoreText.alignment = TextAlignmentOptions.Right;
        scoreText.textWrappingMode = TextWrappingModes.NoWrap;
    }

    void UpdateScore()
    {
        int score = gm.Score, best = gm.DisplayBestScore;
        if (score == lastScore && best == lastBest) return;
        lastScore = score;
        lastBest = best;
        scoreText.text = $"최고 기록 {best:N0}점 : 현재 점수 {score:N0}";
        // 최고 기록을 넘은 동안은 금색
        scoreText.color = (score > 0 && score >= best) ? new Color(1f, 0.85f, 0.25f) : Color.white;
    }

    void Update()
    {
        if (gm == null) return;
        if (scoreText != null) UpdateScore();
        int s = Mathf.FloorToInt(gm.SurvivalTime);
        if (s == lastSecond) return;
        lastSecond = s;
        timeText.text = GameManager.FormatTime(gm.SurvivalTime);
    }

    static void SetBar(RectTransform fill, float ratio) => fill.anchorMax = new Vector2(Mathf.Clamp01(ratio), 1f);

    void UpdateHp(float cur, float max)
    {
        SetBar(hpFill, max > 0f ? cur / max : 0f);
        hpText.text = $"{Mathf.CeilToInt(cur)} / {Mathf.CeilToInt(max)}";
    }

    void UpdateExp(int cur, int next)
    {
        SetBar(expFill, next > 0 ? (float)cur / next : 0f);
        levelText.text = $"Lv {exp.Level}";
    }

    void HandleKill(EnemyData _) => killText.text = $"웨이브 {gm.Wave} · 처치 {gm.KillCount}";

    void HandleBossSpawned(Boss boss)
    {
        bossHealth = boss.GetComponent<Health>();
        bossHealth.OnChanged += UpdateBoss;
        bossText.text = boss.Data != null ? boss.Data.displayName : "Boss";
        bossGroup.SetActive(true);
        UpdateBoss(bossHealth.CurrentHealth, bossHealth.MaxHealth);
    }

    void UpdateBoss(float cur, float max) => SetBar(bossFill, max > 0f ? cur / max : 0f);

    void HideBoss()
    {
        if (bossHealth != null) bossHealth.OnChanged -= UpdateBoss;
        bossHealth = null;
        bossGroup.SetActive(false);
        HandleKill(null); // 웨이브 번호 갱신
    }
}
