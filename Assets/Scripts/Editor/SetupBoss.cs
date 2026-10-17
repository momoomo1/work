using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class SetupBoss
{
    [MenuItem("Tools/Setup/Create Boss")]
    static void Create()
    {
        foreach (var f in new[] { "Assets/Prefabs", "Assets/Materials", "Assets/Data" })
            if (!AssetDatabase.IsValidFolder(f)) AssetDatabase.CreateFolder("Assets", f.Substring(7));

        var purple = LoadOrCreateMat("Assets/Materials/Boss_Purple.mat", new Color(0.55f, 0.15f, 0.8f));
        var enemyProjMat = LoadOrCreateMat("Assets/Materials/BossProjectile.mat", new Color(0.8f, 0.2f, 1f));
        var warnMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Hellfire_Warn.mat")
                      ?? LoadOrCreateMat("Assets/Materials/Boss_Telegraph.mat", new Color(1f, 0.1f, 0.1f));

        int enemyLayer = SetupEnemy.EnsureLayer("Enemy");

        // 보스 프리팹: 루트(로직+콜라이더) + Visual(큰 보라 캡슐) + Telegraph(돌진 예고)
        var root = new GameObject("Boss");
        root.layer = enemyLayer;
        var col = root.AddComponent<CapsuleCollider>();
        col.isTrigger = true;
        col.height = 5f;
        col.radius = 1.25f;
        root.AddComponent<Health>();
        var boss = root.AddComponent<Boss>();

        var visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        visual.name = "Visual";
        Object.DestroyImmediate(visual.GetComponent<CapsuleCollider>());
        visual.transform.SetParent(root.transform, false);
        visual.transform.localScale = Vector3.one * 2.5f;
        visual.layer = enemyLayer;
        visual.GetComponent<MeshRenderer>().sharedMaterial = purple;

        var tele = GameObject.CreatePrimitive(PrimitiveType.Cube);
        tele.name = "Telegraph";
        Object.DestroyImmediate(tele.GetComponent<BoxCollider>());
        tele.transform.SetParent(root.transform, false);
        tele.GetComponent<MeshRenderer>().sharedMaterial = warnMat;
        tele.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        tele.SetActive(false);

        var bso = new SerializedObject(boss);
        bso.FindProperty("telegraph").objectReferenceValue = tele.transform;
        bso.ApplyModifiedPropertiesWithoutUndo();

        var bossPrefab = PrefabUtility.SaveAsPrefabAsset(root, "Assets/Prefabs/Boss.prefab");
        Object.DestroyImmediate(root);

        // 보스 투사체 프리팹
        var proj = new GameObject("BossProjectile");
        proj.AddComponent<EnemyProjectile>();
        var pv = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        pv.name = "Visual";
        Object.DestroyImmediate(pv.GetComponent<SphereCollider>());
        pv.transform.SetParent(proj.transform, false);
        pv.transform.localScale = Vector3.one * 0.6f;
        pv.GetComponent<MeshRenderer>().sharedMaterial = enemyProjMat;
        var projPrefab = PrefabUtility.SaveAsPrefabAsset(proj, "Assets/Prefabs/BossProjectile.prefab");
        Object.DestroyImmediate(proj);

        // BossData
        const string dataPath = "Assets/Data/Boss_Basic.asset";
        var data = AssetDatabase.LoadAssetAtPath<BossData>(dataPath);
        if (data == null)
        {
            data = ScriptableObject.CreateInstance<BossData>();
            AssetDatabase.CreateAsset(data, dataPath);
        }
        data.displayName = "Boss";
        data.prefab = bossPrefab;
        data.projectilePrefab = projPrefab;
        EditorUtility.SetDirty(data);

        // 씬의 GameManager에 BossData 연결 (없으면 생성)
        var gm = Object.FindFirstObjectByType<GameManager>();
        if (gm == null)
        {
            gm = new GameObject("GameManager").AddComponent<GameManager>();
            Undo.RegisterCreatedObjectUndo(gm.gameObject, "Create GameManager");
        }
        var gso = new SerializedObject(gm);
        gso.FindProperty("bossData").objectReferenceValue = data;
        gso.ApplyModifiedProperties();

        CreateStageClearUI();

        AssetDatabase.SaveAssets();
        Selection.activeObject = data;
        Debug.Log("보스 설정 완료. 씬을 저장(Ctrl+S)하세요.");
    }

    static void CreateStageClearUI()
    {
        var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/Malgun SDF.asset");
        if (TMP_Settings.instance == null || font == null)
        {
            Debug.LogWarning("STAGE CLEAR UI 생략: TMP Essentials 임포트 후 'Create LevelUp UI'를 먼저 실행하고 이 메뉴를 다시 실행하세요.");
            return;
        }

        var old = GameObject.Find("StageClearCanvas");
        if (old != null) Undo.DestroyObjectImmediate(old);

        var canvasGo = new GameObject("StageClearCanvas", typeof(RectTransform));
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 90;
        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        Undo.RegisterCreatedObjectUndo(canvasGo, "Create StageClearCanvas");

        var textGo = new GameObject("StageClearText", typeof(RectTransform));
        var rt = textGo.GetComponent<RectTransform>();
        rt.SetParent(canvasGo.transform, false);
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(1200f, 200f);
        rt.anchoredPosition = new Vector2(0f, 150f);
        var tmp = textGo.AddComponent<TextMeshProUGUI>();
        tmp.font = font;
        tmp.text = "STAGE CLEAR";
        tmp.fontSize = 120;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = new Color(1f, 0.85f, 0.2f);
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;

        var ui = canvasGo.AddComponent<StageClearUI>();
        var so = new SerializedObject(ui);
        so.FindProperty("textObject").objectReferenceValue = textGo;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static Material LoadOrCreateMat(string path, Color color)
    {
        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            AssetDatabase.CreateAsset(mat, path);
        }
        mat.SetColor("_BaseColor", color);
        mat.color = color;
        EditorUtility.SetDirty(mat);
        return mat;
    }
}
