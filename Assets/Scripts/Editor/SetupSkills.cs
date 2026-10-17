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
                area = 0.9f + 0.1f * i, speed = 18f, pierceCount = 999, duration = 2f, tickInterval = 0f
            };
        vacuum.prefab = CreateTrailProjectilePrefab("Vacuum", new Color(0.7f, 0.4f, 1f), new Vector3(0.6f, 0.6f, 2.2f), 1f);
        EditorUtility.SetDirty(vacuum);

        var hell = LoadOrCreate("Assets/Data/Skills/Skill_Hellfire.asset");
        hell.id = "hellfire";
        hell.displayName = "헬파이어";
        hell.description = "적이 가장 밀집한 곳에 경고 후 폭발을 일으킨다.";
        hell.type = SkillType.Explosion;
        hell.evolvesFrom = null;
        float[] hd = { 30f, 40f, 50f, 65f, 80f };
        float[] hc = { 4f, 3.8f, 3.5f, 3.2f, 3f };
        int[] hn = { 1, 1, 2, 2, 3 };
        float[] ha = { 3f, 3.5f, 3.5f, 4f, 4.5f };
        hell.levels = new SkillLevelStat[5];
        for (int i = 0; i < 5; i++)
            hell.levels[i] = new SkillLevelStat
            {
                damage = hd[i], cooldown = hc[i], count = hn[i], area = ha[i],
                speed = 0f, pierceCount = 0, duration = 0.3f, tickInterval = 0f
            };
        CreateHellfirePrefabs(out var boomPrefab, out var warnPrefab);
        hell.prefab = boomPrefab;
        hell.secondaryPrefab = warnPrefab;
        EditorUtility.SetDirty(hell);

        var blade = LoadOrCreate("Assets/Data/Skills/Skill_Blade.asset");
        blade.id = "blade";
        blade.displayName = "회전 칼날";
        blade.description = "플레이어 주변을 도는 칼날이 닿는 적을 벤다.";
        blade.type = SkillType.Orbit;
        blade.evolvesFrom = null;
        int[] bn = { 2, 3, 3, 4, 5 };
        float[] bd = { 8f, 10f, 12f, 15f, 18f };
        float[] br = { 2.5f, 2.7f, 3f, 3.2f, 3.5f };
        float[] bs = { 180f, 200f, 220f, 250f, 280f };
        blade.levels = new SkillLevelStat[5];
        for (int i = 0; i < 5; i++)
            blade.levels[i] = new SkillLevelStat
            {
                damage = bd[i], cooldown = 1f, count = bn[i], area = br[i],
                speed = bs[i], pierceCount = 0, duration = 0f, tickInterval = 0.3f
            };
        blade.prefab = CreateBladePrefab();
        EditorUtility.SetDirty(blade);

        // 서리 구름: speed = 슬로우 비율, count = 장판 수, duration = 장판 지속시간
        var frost = LoadOrCreate("Assets/Data/Skills/Skill_Frost.asset");
        frost.id = "frost";
        frost.displayName = "서리 구름";
        frost.description = "적이 모인 곳에 장판을 깔아 피해를 주고 느리게 만든다.";
        frost.type = SkillType.Zone;
        frost.evolvesFrom = null;
        float[] fd = { 6f, 8f, 10f, 12f, 15f };
        float[] fc = { 5f, 4.8f, 4.5f, 4.2f, 4f };
        int[] fn = { 1, 1, 1, 2, 2 };
        float[] fa = { 3f, 3.5f, 4f, 4.5f, 5f };
        float[] fs = { 0.3f, 0.35f, 0.4f, 0.45f, 0.5f };
        float[] fu = { 3f, 3.5f, 4f, 4.5f, 5f };
        frost.levels = new SkillLevelStat[5];
        for (int i = 0; i < 5; i++)
            frost.levels[i] = new SkillLevelStat
            {
                damage = fd[i], cooldown = fc[i], count = fn[i], area = fa[i],
                speed = fs[i], pierceCount = 0, duration = fu[i], tickInterval = 0.5f
            };
        frost.prefab = CreateFrostPrefab();
        EditorUtility.SetDirty(frost);

        // 플레이어에 SkillManager + 디버그 입력 부착
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            var mgr = player.GetComponent<SkillManager>();
            if (mgr == null) mgr = Undo.AddComponent<SkillManager>(player);
            if (player.GetComponent<SkillDebugInput>() == null) Undo.AddComponent<SkillDebugInput>(player);

            var so = new SerializedObject(mgr);
            var list = so.FindProperty("allSkills");
            list.arraySize = 5;
            list.GetArrayElementAtIndex(4).objectReferenceValue = frost;
            list.GetArrayElementAtIndex(3).objectReferenceValue = blade;
            list.GetArrayElementAtIndex(0).objectReferenceValue = sura;
            list.GetArrayElementAtIndex(1).objectReferenceValue = vacuum;
            list.GetArrayElementAtIndex(2).objectReferenceValue = hell;
            so.ApplyModifiedProperties();
        }

        AssetDatabase.SaveAssets();
        Debug.Log("샘플 스킬 생성 완료 (수라참, 진공참). 씬을 저장(Ctrl+S)하세요.");
    }

    // 길쭉한 Cube + Trail Renderer (임시 비주얼)
    static GameObject CreateSuraPrefab()
        => CreateTrailProjectilePrefab("Sura", new Color(0.4f, 0.9f, 1f), new Vector3(0.2f, 0.2f, 1.2f), 0.3f);

    static GameObject CreateTrailProjectilePrefab(string key, Color color, Vector3 cubeScale, float trailWidth)
    {
        foreach (var f in new[] { "Assets/Prefabs", "Assets/Materials" })
            if (!AssetDatabase.IsValidFolder(f)) AssetDatabase.CreateFolder("Assets", f.Substring(7));

        var cubeMat = LoadOrCreateMat($"Assets/Materials/{key}_Cube.mat", "Universal Render Pipeline/Lit", color);
        var trailMat = LoadOrCreateMat($"Assets/Materials/{key}_Trail.mat", "Universal Render Pipeline/Particles/Unlit", color);

        var root = new GameObject($"Projectile_{key}");
        root.AddComponent<Projectile>();

        var visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
        visual.name = "Visual";
        Object.DestroyImmediate(visual.GetComponent<BoxCollider>());
        visual.transform.SetParent(root.transform, false);
        visual.transform.localScale = cubeScale;
        visual.GetComponent<MeshRenderer>().sharedMaterial = cubeMat;

        var trail = root.AddComponent<TrailRenderer>();
        trail.time = 0.25f;
        trail.startWidth = trailWidth;
        trail.endWidth = 0f;
        trail.startColor = color;
        trail.endColor = new Color(color.r, color.g, color.b, 0f);
        trail.sharedMaterial = trailMat;

        var prefab = PrefabUtility.SaveAsPrefabAsset(root, $"Assets/Prefabs/Projectile_{key}.prefab");
        Object.DestroyImmediate(root);
        return prefab;
    }

    // 하늘색 반투명 납작 원기둥 장판 (임시 비주얼)
    static GameObject CreateFrostPrefab()
    {
        var mat = LoadOrCreateMat("Assets/Materials/Frost_Zone.mat", "Universal Render Pipeline/Lit", new Color(0.5f, 0.85f, 1f, 0.45f));
        MakeTransparent(mat);

        var z = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        z.name = "Zone_Frost";
        Object.DestroyImmediate(z.GetComponent<CapsuleCollider>());
        z.transform.localScale = new Vector3(1f, 0.03f, 1f);
        z.GetComponent<MeshRenderer>().sharedMaterial = mat;
        z.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        z.AddComponent<FrostZone>();

        var prefab = PrefabUtility.SaveAsPrefabAsset(z, "Assets/Prefabs/Zone_Frost.prefab");
        Object.DestroyImmediate(z);
        return prefab;
    }

    // 납작한 Cube 칼날 (임시 비주얼)
    static GameObject CreateBladePrefab()
    {
        var mat = LoadOrCreateMat("Assets/Materials/Blade.mat", "Universal Render Pipeline/Lit", new Color(0.8f, 0.85f, 0.9f));

        var root = new GameObject("Blade_Orbit");
        root.AddComponent<OrbitBlade>();
        var visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
        visual.name = "Visual";
        Object.DestroyImmediate(visual.GetComponent<BoxCollider>());
        visual.transform.SetParent(root.transform, false);
        visual.transform.localScale = new Vector3(0.35f, 0.08f, 1.2f);
        visual.GetComponent<MeshRenderer>().sharedMaterial = mat;

        var prefab = PrefabUtility.SaveAsPrefabAsset(root, "Assets/Prefabs/Blade_Orbit.prefab");
        Object.DestroyImmediate(root);
        return prefab;
    }

    // 폭발(오렌지 Sphere, 커짐) + 경고 원(붉은 납작 원기둥, 고정). 둘 다 TimedScaleVfx.
    static void CreateHellfirePrefabs(out GameObject boom, out GameObject warn)
    {
        var boomMat = LoadOrCreateMat("Assets/Materials/Hellfire_Boom.mat", "Universal Render Pipeline/Lit", new Color(1f, 0.5f, 0.1f, 0.7f));
        var warnMat = LoadOrCreateMat("Assets/Materials/Hellfire_Warn.mat", "Universal Render Pipeline/Lit", new Color(1f, 0.1f, 0.1f, 0.4f));
        MakeTransparent(boomMat);
        MakeTransparent(warnMat);

        var b = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        b.name = "Explosion_Hellfire";
        Object.DestroyImmediate(b.GetComponent<SphereCollider>());
        b.GetComponent<MeshRenderer>().sharedMaterial = boomMat;
        b.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        SetVfx(b.AddComponent<TimedScaleVfx>(), true, false);
        boom = PrefabUtility.SaveAsPrefabAsset(b, "Assets/Prefabs/Explosion_Hellfire.prefab");
        Object.DestroyImmediate(b);

        var w = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        w.name = "Warning_Hellfire";
        Object.DestroyImmediate(w.GetComponent<CapsuleCollider>());
        w.transform.localScale = new Vector3(1f, 0.02f, 1f); // 납작하게
        w.GetComponent<MeshRenderer>().sharedMaterial = warnMat;
        w.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        SetVfx(w.AddComponent<TimedScaleVfx>(), false, true);
        warn = PrefabUtility.SaveAsPrefabAsset(w, "Assets/Prefabs/Warning_Hellfire.prefab");
        Object.DestroyImmediate(w);
    }

    static void SetVfx(TimedScaleVfx vfx, bool grow, bool flat)
    {
        var so = new SerializedObject(vfx);
        so.FindProperty("grow").boolValue = grow;
        so.FindProperty("flat").boolValue = flat;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    // URP Lit을 투명 모드로 전환
    static void MakeTransparent(Material mat)
    {
        mat.SetFloat("_Surface", 1f);
        mat.SetFloat("_Blend", 0f);
        mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetFloat("_ZWrite", 0f);
        mat.SetOverrideTag("RenderType", "Transparent");
        mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        EditorUtility.SetDirty(mat);
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
