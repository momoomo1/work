using UnityEditor;
using UnityEngine;

// 이펙트 프리팹(Cartoon FX 등 어떤 파티클 프리팹이든)을 슬롯에 끌어넣고 Apply하면
// 풀 호환 래퍼 프리팹(Assets/Prefabs/Fx_*.prefab)을 만들어 스킬/피격/사망에 연결한다.
public class SetupVfxWindow : EditorWindow
{
    [SerializeField] GameObject hellfire;
    [SerializeField] float hellfireRefDiameter = 6f;
    [SerializeField] GameObject frost;
    [SerializeField] float frostRefDiameter = 6f;
    [SerializeField] GameObject bladeModel;                 // 검 모델 (있으면 큐브 대신 사용)
    [SerializeField] float bladeModelScale = 1f;
    [SerializeField] Vector3 bladeModelEuler = new Vector3(90f, 0f, 0f); // 모델의 칼날 방향을 진행(접선) 방향(+Z)으로 눕힘
    [SerializeField] GameObject bladeFx;
    [SerializeField] float bladeFxScale = 0.6f;
    [SerializeField] GameObject basicShot;
    [SerializeField] float basicShotScale = 0.5f;
    [SerializeField] GameObject sura;
    [SerializeField] float suraScale = 1f;
    [SerializeField] GameObject vacuum;
    [SerializeField] float vacuumScale = 2f;
    [SerializeField] GameObject bossShot;
    [SerializeField] float bossShotScale = 0.6f;
    [SerializeField] GameObject enemyHit;
    [SerializeField] GameObject enemyDeath;
    [SerializeField] GameObject bossDeath;
    [SerializeField] GameObject playerHit;
    [SerializeField] float hitRefDiameter = 2f; // 히트/사망 이펙트 원본 크기(지름) 추정치

    const string Cfx = "Assets/JMO Assets/Cartoon FX Remaster/CFXR Prefabs/";

    [MenuItem("Tools/Setup/VFX Wiring")]
    static void Open() => GetWindow<SetupVfxWindow>("VFX Wiring");

    // 창을 열지 않고 Cartoon FX 추천 조합을 바로 적용
    [MenuItem("Tools/Setup/Apply Cartoon FX Preset")]
    static void ApplyPreset()
    {
        var w = CreateInstance<SetupVfxWindow>();
        w.LoadPreset();
        w.Apply();
        DestroyImmediate(w);
    }

    void LoadPreset()
    {
        hellfire = Load("Explosions/CFXR3 Fire Explosion B.prefab");
        frost = Load("Ice/CFXR3 Hit Ice B (Air).prefab");
        frostRefDiameter = 3f;
        basicShot = Load("Light/CFXR3 LightGlow A (Loop).prefab");
        basicShotScale = 0.5f;
        bladeFx = basicShot;
        bladeFxScale = 0.6f;
        bladeModel = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/Kendall H/Easy Weapons/Prefabs/Regular_Enchanted/sword.RegColor.WithEnch.0.6.prefab"); // 없으면 큐브 사용
        sura = Load("Sword Trails/Plain/CFXR4 Sword Trail PLAIN (360 Thin Spiral).prefab");
        suraScale = 1f;
        vacuum = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/UnityTechnologies/ParticlePack/EffectExamples/Fire & Explosion Effects/Prefabs/FireBall.prefab");
        if (vacuum == null) Debug.LogWarning("Particle Pack의 FireBall을 찾지 못했습니다.");
        vacuumScale = 2f;
        bossShot = Load("Fire/CFXR3 Hit Fire B (Air).prefab");
        enemyHit = Load("Impacts/CFXR Hit A (Red).prefab");
        enemyDeath = Load("Eerie/CFXR2 WW Enemy Explosion.prefab");
        bossDeath = Load("Explosions/CFXR2 WW Explosion.prefab");
        playerHit = Load("Impacts/CFXR Hit D 3D (Yellow).prefab");
    }

    static GameObject Load(string relative)
    {
        var go = AssetDatabase.LoadAssetAtPath<GameObject>(Cfx + relative);
        if (go == null) Debug.LogWarning($"프리셋 이펙트를 찾지 못했습니다: {Cfx + relative}");
        return go;
    }

    void OnGUI()
    {
        EditorGUILayout.HelpBox("이펙트 프리팹을 슬롯에 넣고 Apply. 비운 슬롯은 건드리지 않습니다. 크기가 어색하면 Ref Diameter/Scale을 조절하고 다시 Apply하세요.", MessageType.Info);
        hellfire = Field("헬파이어 폭발 (불 폭발)", hellfire);
        hellfireRefDiameter = EditorGUILayout.FloatField("  원본 지름", hellfireRefDiameter);
        frost = Field("서리 구름 (얼음 장판)", frost);
        frostRefDiameter = EditorGUILayout.FloatField("  원본 지름", frostRefDiameter);
        bladeModel = Field("회전 칼날 모델 (검 프리팹, 비우면 큐브)", bladeModel);
        bladeModelScale = EditorGUILayout.FloatField("  모델 스케일", bladeModelScale);
        bladeModelEuler = EditorGUILayout.Vector3Field("  모델 회전", bladeModelEuler);
        bladeFx = Field("회전 칼날 (빛 이펙트, 트레일에 추가)", bladeFx);
        bladeFxScale = EditorGUILayout.FloatField("  스케일", bladeFxScale);
        basicShot = Field("기본 공격 투사체", basicShot);
        basicShotScale = EditorGUILayout.FloatField("  스케일", basicShotScale);
        sura = Field("수라참 투사체 (트레일/슬래시)", sura);
        suraScale = EditorGUILayout.FloatField("  스케일", suraScale);
        vacuum = Field("진공참 투사체", vacuum);
        vacuumScale = EditorGUILayout.FloatField("  스케일", vacuumScale);
        bossShot = Field("보스 투사체", bossShot);
        bossShotScale = EditorGUILayout.FloatField("  스케일", bossShotScale);
        EditorGUILayout.Space();
        enemyHit = Field("적 피격", enemyHit);
        enemyDeath = Field("적 사망", enemyDeath);
        bossDeath = Field("보스 사망", bossDeath);
        playerHit = Field("플레이어 피격", playerHit);
        hitRefDiameter = EditorGUILayout.FloatField("히트/사망 원본 지름", hitRefDiameter);

        if (GUILayout.Button("추천 프리셋 불러오기 (Cartoon FX)")) LoadPreset();
        if (GUILayout.Button("Apply", GUILayout.Height(32))) Apply();
    }

    static GameObject Field(string label, GameObject v) => (GameObject)EditorGUILayout.ObjectField(label, v, typeof(GameObject), false);

    void Apply()
    {
        // 스킬 비주얼
        if (hellfire != null) SetSkill("Hellfire", MakePooled("Fx_Hellfire", hellfire, hellfireRefDiameter, 2f));
        if (frost != null) SetSkill("Frost", MakeFrost());
        if (bladeFx != null || bladeModel != null) SetSkill("Blade", MakeBlade());
        if (basicShot != null)
        {
            var basic = MakeProjectile("Fx_Basic", basicShot, basicShotScale);
            var p = GameObject.FindGameObjectWithTag("Player");
            var atk = p != null ? p.GetComponent<PlayerAttack>() : null;
            if (atk != null)
            {
                var aso = new SerializedObject(atk);
                aso.FindProperty("projectilePrefab").objectReferenceValue = basic;
                aso.ApplyModifiedProperties();
            }
            else Debug.LogWarning("Player의 PlayerAttack을 찾지 못해 기본 공격 투사체는 연결하지 못했습니다.");
        }
        if (sura != null) SetSkill("Sura", MakeProjectile("Fx_Sura", sura, suraScale, true));
        if (vacuum != null) SetSkill("Vacuum", MakeProjectile("Fx_Vacuum", vacuum, vacuumScale));

        if (bossShot != null)
        {
            var shot = MakeEnemyShot("Fx_BossShot", bossShot, bossShotScale);
            var data = AssetDatabase.LoadAssetAtPath<BossData>("Assets/Data/Boss_Basic.asset");
            if (data != null) { data.projectilePrefab = shot; EditorUtility.SetDirty(data); }
        }

        // 피격 / 사망 (풀 래퍼)
        var eHit = enemyHit != null ? MakePooled("Fx_EnemyHit", enemyHit, hitRefDiameter, 1.5f) : null;
        var eDeath = enemyDeath != null ? MakePooled("Fx_EnemyDeath", enemyDeath, hitRefDiameter, 2f) : null;
        var bDeath = bossDeath != null ? MakePooled("Fx_BossDeath", bossDeath, hitRefDiameter, 3f) : eDeath;
        var pHit = playerHit != null ? MakePooled("Fx_PlayerHit", playerHit, hitRefDiameter, 1.5f) : eHit;

        WirePrefab("Assets/Prefabs/Enemy_Basic.prefab", eHit, eDeath, 2f, 1f, Sfx.EnemyHit, Sfx.EnemyDeath);
        WirePrefab("Assets/Prefabs/Boss.prefab", eHit, bDeath, 5f, 2.5f, Sfx.EnemyHit, Sfx.EnemyDeath);

        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            var hf = player.GetComponent<HitFeedback>();
            if (hf == null) hf = Undo.AddComponent<HitFeedback>(player);
            Configure(hf, pHit, null, 2f, 1f, Sfx.PlayerHit, Sfx.None);
            EditorUtility.SetDirty(player);
        }

        AssetDatabase.SaveAssets();
        Debug.Log("VFX 연결 완료. 플레이어에 HitFeedback이 추가됐으니 씬을 저장(Ctrl+S)하세요.");
    }

    static void SetSkill(string key, GameObject prefab)
    {
        var data = AssetDatabase.LoadAssetAtPath<SkillData>($"Assets/Data/Skills/Skill_{key}.asset");
        if (data == null || prefab == null) { Debug.LogWarning($"스킬 데이터 Skill_{key} 를 찾지 못해 건너뜀"); return; }
        data.prefab = prefab;
        EditorUtility.SetDirty(data);
    }

    // 폭발/히트 등 1회성 이펙트: 루트에 PooledVfx
    static GameObject MakePooled(string name, GameObject fx, float refDiameter, float lifetime)
    {
        var root = new GameObject(name);
        var vfx = root.AddComponent<PooledVfx>();
        var so = new SerializedObject(vfx);
        so.FindProperty("referenceDiameter").floatValue = refDiameter;
        so.FindProperty("lifetime").floatValue = lifetime;
        so.ApplyModifiedPropertiesWithoutUndo();
        Attach(root, fx, 1f);
        return Save(root, name);
    }

    // 회전 칼날: 은색 납작 큐브 + 밝은 트레일 + 빛 이펙트 (루트에 OrbitBlade)
    GameObject MakeBlade()
    {
        var bladeMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Blade.mat");
        var trailMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Blade_Trail.mat");
        if (trailMat == null)
        {
            trailMat = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
            AssetDatabase.CreateAsset(trailMat, "Assets/Materials/Blade_Trail.mat");
        }
        var trailColor = new Color(0.75f, 0.9f, 1f, 1f);
        trailMat.SetColor("_BaseColor", trailColor);
        trailMat.color = trailColor;
        EditorUtility.SetDirty(trailMat);

        var root = new GameObject("Fx_Blade");
        root.AddComponent<OrbitBlade>();

        if (bladeModel != null)
        {
            var model = (GameObject)PrefabUtility.InstantiatePrefab(bladeModel, root.transform);
            model.name = "Model";
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.Euler(bladeModelEuler);
            model.transform.localScale = Vector3.one * bladeModelScale;
            foreach (var c in model.GetComponentsInChildren<Collider>(true)) Object.DestroyImmediate(c);

            // 에셋의 무기 컨트롤러는 인챈트(빛) 메시를 시작할 때 숨기므로 항상 보이게 설정
            var ctrl = model.GetComponentInChildren<EasyWeapon.weaponControllerAA>(true);
            if (ctrl != null)
            {
                var cso = new SerializedObject(ctrl);
                cso.FindProperty("enchantAlwaysVisable").boolValue = true;
                cso.ApplyModifiedPropertiesWithoutUndo();
            }
        }
        else
        {
            var core = GameObject.CreatePrimitive(PrimitiveType.Cube);
            core.name = "Core";
            Object.DestroyImmediate(core.GetComponent<BoxCollider>());
            core.transform.SetParent(root.transform, false);
            core.transform.localScale = new Vector3(0.4f, 0.1f, 1.4f);
            if (bladeMat != null) core.GetComponent<MeshRenderer>().sharedMaterial = bladeMat;
        }

        var trail = root.AddComponent<TrailRenderer>();
        trail.time = 0.35f;
        trail.startWidth = 0.7f;
        trail.endWidth = 0f;
        trail.startColor = trailColor;
        trail.endColor = new Color(trailColor.r, trailColor.g, trailColor.b, 0f);
        trail.sharedMaterial = trailMat;

        if (bladeFx != null) Attach(root, bladeFx, bladeFxScale);
        return Save(root, "Fx_Blade");
    }

    // addCore: 이펙트가 잘 안 보일 때를 대비해 밝은 큐브 + 트레일(기존 임시 비주얼)을 함께 둔다
    static GameObject MakeProjectile(string name, GameObject fx, float scale, bool addCore = false)
    {
        var root = new GameObject(name);
        root.AddComponent<Projectile>();
        Attach(root, fx, scale);

        if (addCore)
        {
            var cubeMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Sura_Cube.mat");
            var trailMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Sura_Trail.mat");

            var core = GameObject.CreatePrimitive(PrimitiveType.Cube);
            core.name = "Core";
            Object.DestroyImmediate(core.GetComponent<BoxCollider>());
            core.transform.SetParent(root.transform, false);
            core.transform.localScale = new Vector3(0.3f, 0.3f, 1.6f);
            if (cubeMat != null) core.GetComponent<MeshRenderer>().sharedMaterial = cubeMat;

            var trail = root.AddComponent<TrailRenderer>();
            trail.time = 0.3f;
            trail.startWidth = 0.6f;
            trail.endWidth = 0f;
            trail.startColor = new Color(0.4f, 0.9f, 1f, 1f);
            trail.endColor = new Color(0.4f, 0.9f, 1f, 0f);
            if (trailMat != null) trail.sharedMaterial = trailMat;
        }
        return Save(root, name);
    }

    static GameObject MakeEnemyShot(string name, GameObject fx, float scale)
    {
        var root = new GameObject(name);
        root.AddComponent<EnemyProjectile>();
        Attach(root, fx, scale);
        return Save(root, name);
    }

    // 서리 구름: 납작한 바닥 원판(Disc) + 파티클(Fx). Disc는 납작하게, Fx는 균일하게 스케일됨.
    GameObject MakeFrost()
    {
        var mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Frost_Zone.mat");

        var root = new GameObject("Fx_Frost");
        var zone = root.AddComponent<FrostZone>();

        var disc = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        disc.name = "Disc";
        Object.DestroyImmediate(disc.GetComponent<CapsuleCollider>());
        disc.transform.SetParent(root.transform, false);
        disc.transform.localScale = new Vector3(1f, 0.03f, 1f);
        var mr = disc.GetComponent<MeshRenderer>();
        if (mat != null) mr.sharedMaterial = mat;
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        var fxGo = new GameObject("FxRoot");
        fxGo.transform.SetParent(root.transform, false);
        Attach(fxGo, frost, 1f);

        var so = new SerializedObject(zone);
        so.FindProperty("disc").objectReferenceValue = disc.transform;
        so.FindProperty("fxRoot").objectReferenceValue = fxGo.transform;
        so.FindProperty("fxReferenceDiameter").floatValue = frostRefDiameter;
        so.ApplyModifiedPropertiesWithoutUndo();
        return Save(root, "Fx_Frost");
    }

    static void Attach(GameObject root, GameObject fxPrefab, float scale)
    {
        var child = (GameObject)PrefabUtility.InstantiatePrefab(fxPrefab, root.transform);
        child.name = "Fx";
        child.transform.localPosition = Vector3.zero;
        child.transform.localRotation = Quaternion.identity;
        child.transform.localScale = Vector3.one * scale;
    }

    static GameObject Save(GameObject root, string name)
    {
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs")) AssetDatabase.CreateFolder("Assets", "Prefabs");
        var prefab = PrefabUtility.SaveAsPrefabAsset(root, $"Assets/Prefabs/{name}.prefab");
        Object.DestroyImmediate(root);
        return prefab;
    }

    static void WirePrefab(string path, GameObject hit, GameObject death, float diameter, float height, Sfx hitSfx, Sfx deathSfx)
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) == null) return;
        var root = PrefabUtility.LoadPrefabContents(path);
        try
        {
            var hf = root.GetComponent<HitFeedback>();
            if (hf == null) hf = root.AddComponent<HitFeedback>();
            Configure(hf, hit, death, diameter, height, hitSfx, deathSfx);
            PrefabUtility.SaveAsPrefabAsset(root, path);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
    }

    static void Configure(HitFeedback hf, GameObject hit, GameObject death, float diameter, float height, Sfx hitSfx, Sfx deathSfx)
    {
        var so = new SerializedObject(hf);
        so.FindProperty("hitVfx").objectReferenceValue = hit;
        so.FindProperty("deathVfx").objectReferenceValue = death;
        so.FindProperty("vfxDiameter").floatValue = diameter;
        so.FindProperty("vfxHeight").floatValue = height;
        so.FindProperty("hitSfx").enumValueIndex = (int)hitSfx;
        so.FindProperty("deathSfx").enumValueIndex = (int)deathSfx;
        so.ApplyModifiedPropertiesWithoutUndo();
    }
}
