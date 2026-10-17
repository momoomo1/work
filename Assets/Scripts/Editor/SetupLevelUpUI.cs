using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public static class SetupLevelUpUI
{
    const string FontSrc = @"C:\Windows\Fonts\malgun.ttf";
    const string FontPath = "Assets/Fonts/malgun.ttf";
    const string FontAssetPath = "Assets/Fonts/Malgun SDF.asset";
    const string CardPath = "Assets/Prefabs/SkillCard.prefab";

    [MenuItem("Tools/Setup/Create LevelUp UI")]
    static void Create()
    {
        if (TMP_Settings.instance == null)
        {
            EditorUtility.DisplayDialog("TextMeshPro 필요",
                "먼저 Window > TextMeshPro > Import TMP Essential Resources 를 실행한 뒤 다시 시도하세요.", "확인");
            return;
        }

        var font = GetKoreanFont();
        if (font == null) return;

        if (!AssetDatabase.IsValidFolder("Assets/Prefabs")) AssetDatabase.CreateFolder("Assets", "Prefabs");

        var cardPrefab = CreateCardPrefab(font);

        // 기존 캔버스 제거 후 재생성
        var old = GameObject.Find("LevelUpCanvas");
        if (old != null) Undo.DestroyObjectImmediate(old);

        var canvasGo = new GameObject("LevelUpCanvas", typeof(RectTransform));
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGo.AddComponent<GraphicRaycaster>();
        Undo.RegisterCreatedObjectUndo(canvasGo, "Create LevelUpCanvas");

        // 패널: 전체 화면 반투명 배경
        var panel = MakeRect("LevelUpPanel", canvasGo.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        var dim = panel.gameObject.AddComponent<Image>();
        dim.color = new Color(0f, 0f, 0f, 0.75f);

        MakeText("Title", panel, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 300f),
            new Vector2(800f, 100f), "레벨 업!", 64, Color.white, font);

        var container = MakeRect("Cards", panel, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1100f, 460f));
        var layout = container.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 40f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        var ui = canvasGo.AddComponent<LevelUpUI>();
        var so = new SerializedObject(ui);
        so.FindProperty("panel").objectReferenceValue = panel.gameObject;
        so.FindProperty("cardContainer").objectReferenceValue = container;
        so.FindProperty("cardPrefab").objectReferenceValue = cardPrefab.GetComponent<SkillCardUI>();
        so.ApplyModifiedPropertiesWithoutUndo();

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

        AssetDatabase.SaveAssets();
        Debug.Log("레벨업 UI 생성 완료. 씬을 저장(Ctrl+S)하세요.");
    }

    static GameObject CreateCardPrefab(TMP_FontAsset font)
    {
        var root = MakeRect("SkillCard", null, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(320f, 440f));
        var bg = root.gameObject.AddComponent<Image>();
        bg.color = new Color(0.15f, 0.16f, 0.2f, 1f);
        var button = root.gameObject.AddComponent<Button>();
        button.targetGraphic = bg;
        var colors = button.colors;
        colors.highlightedColor = new Color(1.3f, 1.3f, 1.5f, 1f);
        colors.pressedColor = new Color(0.8f, 0.8f, 0.9f, 1f);
        button.colors = colors;

        var iconRt = MakeRect("Icon", root, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -110f), new Vector2(160f, 160f));
        var icon = iconRt.gameObject.AddComponent<Image>();
        icon.raycastTarget = false;

        var nameText = MakeText("Name", root, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -240f), new Vector2(-20f, 50f), "스킬 이름", 36, Color.white, font);
        var levelText = MakeText("Level", root, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -285f), new Vector2(-20f, 40f), "Lv 1 → Lv 2", 26, new Color(1f, 0.85f, 0.3f), font);
        var descText = MakeText("Desc", root, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -370f), new Vector2(-30f, 110f), "설명", 22, new Color(0.85f, 0.85f, 0.9f), font);
        var keyText = MakeText("Key", root, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(35f, -25f), new Vector2(60f, 40f), "[1]", 24, new Color(0.7f, 0.7f, 0.75f), font);

        var card = root.gameObject.AddComponent<SkillCardUI>();
        var so = new SerializedObject(card);
        so.FindProperty("button").objectReferenceValue = button;
        so.FindProperty("icon").objectReferenceValue = icon;
        so.FindProperty("nameText").objectReferenceValue = nameText;
        so.FindProperty("levelText").objectReferenceValue = levelText;
        so.FindProperty("descText").objectReferenceValue = descText;
        so.FindProperty("keyText").objectReferenceValue = keyText;
        so.ApplyModifiedPropertiesWithoutUndo();

        var prefab = PrefabUtility.SaveAsPrefabAsset(root.gameObject, CardPath);
        Object.DestroyImmediate(root.gameObject);
        return prefab;
    }

    // 한글 표시를 위해 맑은 고딕으로 동적 TMP 폰트 에셋 생성
    internal static TMP_FontAsset GetKoreanFont()
    {
        var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);
        if (existing != null) return existing;

        if (!AssetDatabase.IsValidFolder("Assets/Fonts")) AssetDatabase.CreateFolder("Assets", "Fonts");
        if (!File.Exists(FontPath))
        {
            if (!File.Exists(FontSrc))
            {
                Debug.LogError($"{FontSrc} 가 없습니다. 한글 TTF를 Assets/Fonts/malgun.ttf 로 넣고 다시 실행하세요.");
                return null;
            }
            File.Copy(FontSrc, FontPath);
            AssetDatabase.ImportAsset(FontPath);
        }

        var src = AssetDatabase.LoadAssetAtPath<Font>(FontPath);
        var fa = TMP_FontAsset.CreateFontAsset(src, 90, 9, UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA,
            1024, 1024, AtlasPopulationMode.Dynamic, true);
        AssetDatabase.CreateAsset(fa, FontAssetPath);
        fa.material.name = "Malgun SDF Material";
        AssetDatabase.AddObjectToAsset(fa.material, fa);
        foreach (var tex in fa.atlasTextures) AssetDatabase.AddObjectToAsset(tex, fa);
        EditorUtility.SetDirty(fa);
        AssetDatabase.SaveAssets();
        return fa;
    }

    internal static RectTransform MakeRect(string name, Transform parent, Vector2 aMin, Vector2 aMax, Vector2 pos, Vector2 size)
    {
        var rt = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        if (parent != null) rt.SetParent(parent, false);
        rt.anchorMin = aMin;
        rt.anchorMax = aMax;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        return rt;
    }

    internal static TMP_Text MakeText(string name, Transform parent, Vector2 aMin, Vector2 aMax, Vector2 pos, Vector2 size,
        string text, float fontSize, Color color, TMP_FontAsset font)
    {
        var rt = MakeRect(name, parent, aMin, aMax, pos, size);
        var tmp = rt.gameObject.AddComponent<TextMeshProUGUI>();
        tmp.font = font;
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;
        return tmp;
    }
}
