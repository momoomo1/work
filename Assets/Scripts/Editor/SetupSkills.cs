using UnityEditor;
using UnityEngine;

public static class SetupSkills
{
    [MenuItem("Tools/Setup/Create Sample Skills")]
    static void Create()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Data")) AssetDatabase.CreateFolder("Assets", "Data");
        if (!AssetDatabase.IsValidFolder("Assets/Data/Skills")) AssetDatabase.CreateFolder("Assets/Data", "Skills");

        var sura = LoadOrCreate("Assets/Data/Skills/Skill_Sura.asset");
        sura.id = "sura";
        sura.displayName = "수라참";
        sura.description = "가장 가까운 적에게 관통 투사체를 발사한다.";
        sura.type = SkillType.Projectile;
        sura.evolvesFrom = null;
        float[] dmg = { 12f, 16f, 20f, 25f, 32f };
        float[] cd = { 1.0f, 0.95f, 0.9f, 0.8f, 0.7f };
        int[] cnt = { 1, 1, 2, 2, 3 };
        int[] prc = { 2, 3, 3, 4, 5 };
        sura.levels = new SkillLevelStat[5];
        for (int i = 0; i < 5; i++)
            sura.levels[i] = new SkillLevelStat
            {
                damage = dmg[i], cooldown = cd[i], count = cnt[i],
                area = 0.4f, speed = 16f, pierceCount = prc[i], duration = 1.2f, tickInterval = 0f
            };
        var suraPrefab = CreateSuraPrefab();
        sura.prefab = suraPrefab;
        EditorUtility.SetDirty(sura);

        var vacuum = LoadOrCreate("Assets/Data/Skills/Skill_Vacuum.asset");
        vacuum.id = "vacuum";
        vacuum.displayName = "진공참";
        vacuum.description = "수라참의 강화형. 더 크고 많은 적을 관통한다.";
        vacuum.type = SkillType.Projectile;
        vacuum.evolvesFrom = sura;
        vacuum.levels = new SkillLevelStat[3];
        for (int i = 0; i < 3; i++)
            vacuum.levels[i] = new SkillLevelStat
            {
                damage = 40f + 10f * i, cooldown = 0.8f - 0.1f * i, count = 3 + i,
                area = 1f, speed = 18f, pierceCount = 99, duration = 2f, tickInterval = 0f
            };
        vacuum.prefab = suraPrefab; // 임시: 진공참 전용 비주얼 전까지 공용
        EditorUtility.SetDirty(vacuum);

        // 플레이어에 SkillManager + 디버그 입력 부착
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            var mgr = player.GetComponent<SkillManager>();
            if (mgr == null) mgr = Undo.AddComponent<SkillManager>(player);
            if (player.GetComponent<SkillDebugInput>() == null) Undo.AddComponent<SkillDebugInput>(player);

            var so = new SerializedObject(mgr);
            var list = so.FindProperty("allSkills");
            list.arraySize = 2;
            list.GetArrayElementAtIndex(0).objectReferenceValue = sura;
            list.GetArrayElementAtIndex(1).objectReferenceValue = vacuum;
            so.ApplyModifiedProperties();
        }

        AssetDatabase.SaveAssets();
        Debug.Log("샘플 스킬 생성 완료 (수라참, 진공참). 씬을 저장(Ctrl+S)하세요.");
    }

    // 길쭉한 Cube + Trail Renderer (임시 비주얼)
    static GameObject CreateSuraPrefab()
    {
        foreach (var f in new[] { "Assets/Prefabs", "Assets/Materials" })
            if (!AssetDatabase.IsValidFolder(f)) AssetDatabase.CreateFolder("Assets", f.Substring(7));

        var cubeMat = LoadOrCreateMat("Assets/Materials/Sura_Cube.mat", "Universal Render Pipeline/Lit", new Color(0.4f, 0.9f, 1f));
        var trailMat = LoadOrCreateMat("Assets/Materials/Sura_Trail.mat", "Universal Render Pipeline/Particles/Unlit", new Color(0.4f, 0.9f, 1f));

        var root = new GameObject("Projectile_Sura");
        root.AddComponent<Projectile>();

        var visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
        visual.name = "Visual";
        Object.DestroyImmediate(visual.GetComponent<BoxCollider>());
        visual.transform.SetParent(root.transform, false);
        visual.transform.localScale = new Vector3(0.2f, 0.2f, 1.2f);
        visual.GetComponent<MeshRenderer>().sharedMaterial = cubeMat;

        var trail = root.AddComponent<TrailRenderer>();
        trail.time = 0.25f;
        trail.startWidth = 0.3f;
        trail.endWidth = 0f;
        trail.startColor = new Color(0.4f, 0.9f, 1f, 1f);
        trail.endColor = new Color(0.4f, 0.9f, 1f, 0f);
        trail.sharedMaterial = trailMat;

        var prefab = PrefabUtility.SaveAsPrefabAsset(root, "Assets/Prefabs/Projectile_Sura.prefab");
        Object.DestroyImmediate(root);
        return prefab;
    }

    static Material LoadOrCreateMat(string path, string shader, Color color)
    {
        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            mat = new Material(Shader.Find(shader));
            AssetDatabase.CreateAsset(mat, path);
        }
        mat.SetColor("_BaseColor", color);
        mat.color = color;
        EditorUtility.SetDirty(mat);
        return mat;
    }

    static SkillData LoadOrCreate(string path)
    {
        var data = AssetDatabase.LoadAssetAtPath<SkillData>(path);
        if (data == null)
        {
            data = ScriptableObject.CreateInstance<SkillData>();
            AssetDatabase.CreateAsset(data, path);
        }
        return data;
    }
}
