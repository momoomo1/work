using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 게임을 처음 실행했을 때 뜨는 시작 화면 (게임 시작 / 게임 종료).
// 씬 편집 없이 실행 시 자동으로 만들어지고, 게임 시작을 누를 때까지 게임이 멈춰 있다.
// 게임오버 후 "다시 시작"으로 씬을 다시 불러올 때는 뜨지 않는다 (실행당 한 번만).
public class StartMenuUI : MonoBehaviour
{
    [SerializeField] string title = "ASURA SURVIVOR"; // 게임 제목

    TMP_FontAsset font;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        new GameObject("StartMenuUI").AddComponent<StartMenuUI>();
    }

    void Awake()
    {
        Time.timeScale = 0f; // 시작을 누르기 전까지 정지
    }

    void Start()
    {
        font = FindKoreanFont();
        Build();
    }

    // 씬의 다른 TMP 텍스트(HUD 등)가 쓰는 한글 폰트를 가져옴
    static TMP_FontAsset FindKoreanFont()
    {
        TMP_FontAsset fallback = null;
        foreach (var t in FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (t.font == null) continue;
            fallback = t.font;
            if (t.font.name.ToLowerInvariant().Contains("malgun")) return t.font;
        }
        return fallback;
    }

    void Build()
    {
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200; // 모든 UI 위
        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        gameObject.AddComponent<GraphicRaycaster>();

        var panel = NewRect("Panel", transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        panel.gameObject.AddComponent<Image>().color = new Color(0.04f, 0.05f, 0.09f, 0.94f);

        var center = new Vector2(0.5f, 0.5f);
        var titleText = NewText("Title", panel, center, new Vector2(0f, 190f), new Vector2(1400f, 160f), title, 110, Color.white);
        titleText.fontStyle = FontStyles.Bold;

        NewButton("StartButton", panel, center, new Vector2(0f, -40f), "게임 시작", new Color(0.25f, 0.55f, 0.95f), StartGame);
        NewButton("QuitButton", panel, center, new Vector2(0f, -170f), "게임 종료", new Color(0.35f, 0.37f, 0.42f), QuitGame);
    }

    void StartGame()
    {
        Time.timeScale = 1f;
        Destroy(gameObject);
    }

    // 에디터에서는 플레이 모드를 끄고, 빌드에서는 프로그램을 종료
    public static void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    RectTransform NewRect(string name, Transform parent, Vector2 aMin, Vector2 aMax, Vector2 pos, Vector2 size)
    {
        var rt = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchorMin = aMin;
        rt.anchorMax = aMax;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        return rt;
    }

    TMP_Text NewText(string name, Transform parent, Vector2 anchor, Vector2 pos, Vector2 size, string text, float fontSize, Color color)
    {
        var rt = NewRect(name, parent, anchor, anchor, pos, size);
        var tmp = rt.gameObject.AddComponent<TextMeshProUGUI>();
        if (font != null) tmp.font = font;
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;
        return tmp;
    }

    void NewButton(string name, Transform parent, Vector2 anchor, Vector2 pos, string label, Color color, UnityEngine.Events.UnityAction onClick)
    {
        var rt = NewRect(name, parent, anchor, anchor, pos, new Vector2(380f, 100f));
        var img = rt.gameObject.AddComponent<Image>();
        img.color = color;
        var button = rt.gameObject.AddComponent<Button>();
        button.targetGraphic = img;
        var colors = button.colors;
        colors.highlightedColor = new Color(1.25f, 1.25f, 1.25f, 1f);
        colors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
        colors.fadeDuration = 0.05f;
        button.colors = colors;
        button.onClick.AddListener(onClick);
        NewText("Label", rt, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(360f, 90f), label, 44, Color.white);
    }
}
