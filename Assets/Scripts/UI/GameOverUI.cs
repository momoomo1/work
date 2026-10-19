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
        CreateQuitButton();
        gm = GameManager.Instance;
        gm.OnPlayerDied += Show;
    }

    // 다시 시작 버튼을 복제해서 바로 아래에 '게임 종료' 버튼을 만듦
    void CreateQuitButton()
    {
        var quit = Instantiate(restartButton, restartButton.transform.parent);
        quit.name = "QuitButton";
        quit.onClick.RemoveAllListeners();
        quit.onClick.AddListener(StartMenuUI.QuitGame);

        var rt = (RectTransform)quit.transform;
        rt.anchoredPosition = ((RectTransform)restartButton.transform).anchoredPosition + new Vector2(0f, -110f);
        if (quit.TryGetComponent<Image>(out var img)) img.color = new Color(0.35f, 0.37f, 0.42f);
        var label = quit.GetComponentInChildren<TMP_Text>();
        if (label != null) label.text = "게임 종료";
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
