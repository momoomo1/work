using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class SetupHUD
{
    [MenuItem("Tools/Setup/Create HUD and Game Over")]
    static void Create()
    {
        if (TMP_Settings.instance == null)
        {
            EditorUtility.DisplayDialog("TextMeshPro 필요",
                "먼저 Window > TextMeshPro > Import TMP Essential Resources 를 실행한 뒤 다시 시도하세요.", "확인");
            return;
        }
        var font = SetupLevelUpUI.GetKoreanFont();
        if (font == null) return;

        var old = GameObject.Find("HUDCanvas");
        if (old != null) Undo.DestroyObjectImmediate(old);

        var canvasGo = new GameObject("HUDCanvas", typeof(RectTransform));
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 80;
        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGo.AddComponent<GraphicRaycaster>();
        Undo.RegisterCreatedObjectUndo(canvasGo, "Create HUDCanvas");

        var t = canvasGo.transform;
        var topLeft = new Vector2(0f, 1f);
        var topRight = new Vector2(1f, 1f);
        var topCenter = new Vector2(0.5f, 1f);
        var white = Color.white;

        // 플레이어 체력 바 (좌상단)
        var hp = MakeBar("HpBar", t, topLeft, new Vector2(230f, -40f), new Vector2(400f, 32f), new Color(0.85f, 0.2f, 0.2f), out var hpFill);
        var hpText = SetupLevelUpUI.MakeText("HpText", hp, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, "100 / 100", 22, white, font);

        // 경험치 바 (하단 전체) + 레벨
        var exp = MakeBar("ExpBar", t, new Vector2(0.5f, 0f), new Vector2(0f, 14f), new Vector2(1920f, 24f), new Color(0.3f, 0.6f, 1f), out var expFill);
        exp.anchorMin = new Vector2(0f, 0f);
        exp.anchorMax = new Vector2(1f, 0f);
        exp.sizeDelta = new Vector2(0f, 24f);
        var levelText = SetupLevelUpUI.MakeText("LevelText", t, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(90f, 55f), new Vector2(160f, 40f), "Lv 1", 30, white, font);
        levelText.alignment = TextAlignmentOptions.Left;

        // 처치 수 / 생존 시간 (우상단)
        var timeText = SetupLevelUpUI.MakeText("TimeText", t, topRight, topRight, new Vector2(-130f, -40f), new Vector2(220f, 44f), "00:00", 36, white, font);
        timeText.alignment = TextAlignmentOptions.Right;
        var killText = SetupLevelUpUI.MakeText("KillText", t, topRight, topRight, new Vector2(-130f, -88f), new Vector2(220f, 40f), "처치 0", 30, white, font);
        killText.alignment = TextAlignmentOptions.Right;

        // 보스 체력 바 (상단 중앙)
        var boss = MakeBar("BossBar", t, topCenter, new Vector2(0f, -45f), new Vector2(800f, 36f), new Color(0.6f, 0.2f, 0.85f), out var bossFill);
        var bossText = SetupLevelUpUI.MakeText("BossText", boss, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, "Boss", 24, white, font);

        var hud = canvasGo.AddComponent<HUD>();
        var hso = new SerializedObject(hud);
        hso.FindProperty("hpFill").objectReferenceValue = hpFill;
        hso.FindProperty("hpText").objectReferenceValue = hpText;
        hso.FindProperty("expFill").objectReferenceValue = expFill;
        hso.FindProperty("levelText").objectReferenceValue = levelText;
        hso.FindProperty("killText").objectReferenceValue = killText;
        hso.FindProperty("timeText").objectReferenceValue = timeText;
        hso.FindProperty("bossGroup").objectReferenceValue = boss.gameObject;
        hso.FindProperty("bossFill").objectReferenceValue = bossFill;
        hso.FindProperty("bossText").objectReferenceValue = bossText;
        hso.ApplyModifiedPropertiesWithoutUndo();

        // 게임 오버 패널
        var panel = SetupLevelUpUI.MakeRect("GameOverPanel", t, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        panel.gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.85f);
        var center = new Vector2(0.5f, 0.5f);
        SetupLevelUpUI.MakeText("Title", panel, center, center, new Vector2(0f, 220f), new Vector2(1000f, 160f), "GAME OVER", 110, new Color(0.9f, 0.2f, 0.2f), font);
        var result = SetupLevelUpUI.MakeText("Result", panel, center, center, new Vector2(0f, 20f), new Vector2(800f, 180f), "", 44, white, font);

        var btnRt = SetupLevelUpUI.MakeRect("RestartButton", panel, center, center, new Vector2(0f, -170f), new Vector2(320f, 90f));
        var btnImg = btnRt.gameObject.AddComponent<Image>();
        btnImg.color = new Color(0.25f, 0.5f, 0.9f);
        var btn = btnRt.gameObject.AddComponent<Button>();
        btn.targetGraphic = btnImg;
        SetupLevelUpUI.MakeText("Label", btnRt, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, "다시 시작", 40, white, font);

        var go = canvasGo.AddComponent<GameOverUI>();
        var gso = new SerializedObject(go);
        gso.FindProperty("panel").objectReferenceValue = panel.gameObject;
        gso.FindProperty("resultText").objectReferenceValue = result;
        gso.FindProperty("restartButton").objectReferenceValue = btn;
        gso.ApplyModifiedPropertiesWithoutUndo();

        // 새 Input System용 EventSystem
        var es = Object.FindFirstObjectByType<EventSystem>();
        if (es == null)
        {
            es = new GameObject("EventSystem").AddComponent<EventSystem>();
            Undo.RegisterCreatedObjectUndo(es.gameObject, "Create EventSystem");
        }
        var legacy = es.GetComponent<StandaloneInputModule>();
        if (legacy != null) Object.DestroyImmediate(legacy);
        if (es.GetComponent<InputSystemUIInputModule>() == null)
            es.gameObject.AddComponent<InputSystemUIInputModule>().AssignDefaultActions();

        // 재시작(LoadScene)을 위해 현재 씬을 Build Settings에 등록
        var scene = SceneManager.GetActiveScene();
        if (!string.IsNullOrEmpty(scene.path))
        {
            var list = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            if (!list.Exists(s => s.path == scene.path))
            {
                list.Add(new EditorBuildSettingsScene(scene.path, true));
                EditorBuildSettings.scenes = list.ToArray();
            }
        }
        else Debug.LogWarning("씬이 저장되지 않았습니다. 저장 후 이 메뉴를 다시 실행해야 재시작이 동작합니다.");

        Debug.Log("HUD/게임오버 UI 생성 완료. 씬을 저장(Ctrl+S)하세요.");
    }

    // 배경(어두운색) + 채움(fill) 구조의 바. fill의 anchorMax.x로 비율 조절.
    static RectTransform MakeBar(string name, Transform parent, Vector2 anchor, Vector2 pos, Vector2 size, Color fillColor, out RectTransform fill)
    {
        var bg = SetupLevelUpUI.MakeRect(name, parent, anchor, anchor, pos, size);
        bg.gameObject.AddComponent<Image>().color = new Color(0.08f, 0.08f, 0.1f, 0.85f);
        fill = SetupLevelUpUI.MakeRect("Fill", bg, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        fill.gameObject.AddComponent<Image>().color = fillColor;
        fill.GetComponent<Image>().raycastTarget = false;
        return bg;
    }
}
