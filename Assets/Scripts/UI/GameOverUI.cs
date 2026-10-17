using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] GameObject panel;
    [SerializeField] TMP_Text resultText;
    [SerializeField] Button restartButton;

    GameManager gm;

    void Start()
    {
        panel.SetActive(false);
        restartButton.onClick.AddListener(Restart);
        gm = GameManager.Instance;
        gm.OnPlayerDied += Show;
    }

    void OnDestroy()
    {
        if (gm != null) gm.OnPlayerDied -= Show;
    }

    void Show()
    {
        resultText.text = $"생존 시간  {GameManager.FormatTime(gm.SurvivalTime)}\n처치 수  {gm.KillCount}";
        panel.SetActive(true);
    }

    void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
