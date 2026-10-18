using UnityEditor;
using UnityEngine;

// 이펙트 프리팹(Cartoon FX 등 어떤 파티클 프리팹이든)을 슬롯에 끌어넣고 Apply하면
// 풀 호환 래퍼 프리팹(Assets/Prefabs/Fx_*.prefab)을 만들어 스킬/피격/사망에 연결한다.
public class SetupVfxWindow : EditorWindow
{
    // 메인 이펙트 위에 겹쳐 얹는 추가 이펙트 (예: Cartoon FX 슬래시 + UNI VFX 미사일)
    [SerializeField] GameObject hellfireExtra;
    [SerializeField] GameObject frostExtra;
    [SerializeField] GameObject suraExtra;
    [SerializeField] GameObject vacuumExtra;
    [SerializeField] GameObject bladeExtra;
    [SerializeField] float extraScale = 1f;
    [SerializeField] float bladeExtraScale = 0.4f;

    [SerializeField] GameObject hellfire;
    [SerializeField] float hellfireRefDiameter = 6f;
    [SerializeField] GameObject frost;
    [SerializeField] float frostRefDiameter = 6f;
    [SerializeField] GameObject bladeModel;                 // 검 모델 (있으면 큐브 대신 사용)
    [SerializeField] float bladeModelScale = 1.8f;            // 칼날 모델의 목표 길이(유닛). 모델 크기에 상관없이 이 길이로 자동 조정
    [SerializeField] Vector3 bladeModelEuler = Vector3.zero;  // 자동 정렬 후 추가로 돌릴 각도 (칼끝 방향이 반대일 때 Y 180 등)
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
    [SerializeField] bool suraCore = false; // 수라참에 밝은 큐브+트레일을 추가 (이펙트가 안 보일 때)
    [SerializeField] GameObject enemyHit;
    [SerializeField] GameObject enemyDeath;
    [SerializeField] GameObject bossDeath;
    [SerializeField] GameObject playerHit;
    [SerializeField] float hitRefDiameter = 2f; // 히트 이펙트 원본 크기(지름) 추정치
    [SerializeField] float deathRefDiameter = 2.5f; // 사망 이펙트 원본 크기 추정치 (값이 클수록 작게 나옴)

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

    // 전체 조합: Cartoon FX + Free Slash VFX 기본 위에 UNI VFX(VFX Graph)를 겹쳐 얹는다.
    // com.unity.visualeffectgraph 패키지가 필요하다.
    [MenuItem("Tools/Setup/Apply Full VFX Preset (all skills + UNI VFX)")]
    internal static void ApplyUniPreset()
    {
        const string uni = "Assets/UNI VFX/Missiles & Explosions/Prefabs/";
        var w = CreateInstance<SetupVfxWindow>();
        w.LoadPreset();

        // 수라참: 물 슬래시(메인) + 청록 레이저 미사일(겹침)
        w.suraExtra = LoadAny(uni + "Razor Missile Teal.prefab");
        // 헬파이어(진화): 불 슬래시(메인) + 불꽃 돌진(겹침)
        w.vacuumExtra = LoadAny(uni + "Onslaught Fiery.prefab");
        // 메테오: 폭발은 너무 커 보여서 UNI VFX를 겹치지 않고 작은 폭발 하나만 사용
        // 서리 구름: 얼음 마법진(메인) + 시안 폭발 파동(생성 시 한 번)
        w.frostExtra = LoadAny(uni + "Outburst Cyan.prefab");
        // 회전 칼날: 카타나 + 트레일만 사용 (겹치는 이펙트 없음)
        // 보스 투사체: 보라 오브
        var orb = LoadAny(uni + "Orb Violet.prefab");
        if (orb != null) { w.bossShot = orb; w.bossShotScale = 0.6f; }

        w.Apply();
        DestroyImmediate(w);
    }

    void LoadPreset()
    {
        // 메테오(폭발): 자연스러운 작은 폭발 (Particle Pack). 크기가 크면 원본 지름을 더 키우세요(값이 클수록 작아짐)
        hellfire = LoadAny("Assets/UnityTechnologies/ParticlePack/EffectExamples/Fire & Explosion Effects/Prefabs/SmallExplosion.prefab");
        if (hellfire == null) hellfire = Load("Explosions/CFXR3 Fire Explosion B.prefab");
        hellfireRefDiameter = 8f;
        // 서리 구름: 얼음 마법진 (Game VFX - MagicCircle)
        frost = LoadAny("Assets/Game VFX - MagicCircle/Prefabs/FX_MagicCircle_Icearrow.prefab");
        frostRefDiameter = 4f;
        // 기본 공격: 작은 빛 덩어리
        basicShot = Load("Light/CFXR3 LightGlow A (Loop).prefab");
        basicShotScale = 0.5f;
        // 회전 칼날: 카타나 모델 + 가는 트레일만 (별도 빛 이펙트는 위치가 어긋나 보여서 제거)
        // (카타나가 없으면 Easy Weapons 검, 그것도 없으면 큐브)
        bladeFx = null;
        bladeExtra = null;
        bladeModelScale = 2.4f;
        bladeModel = LoadAny("Assets/Free Slash VFX/Model/Katana/Low-Poly Katana.prefab");
        if (bladeModel == null)
            bladeModel = LoadAny("Assets/Kendall H/Easy Weapons/Prefabs/Regular_Enchanted/sword.RegColor.WithEnch.0.6.prefab");
        // 수라참: 물(청색) 슬래시 투사체 / 헬파이어(진화): 불 슬래시 투사체 (Free Slash VFX)
        sura = LoadAny("Assets/Free Slash VFX/Prefabs/Projectiles/Slash Projectile VFX Water.prefab");
        suraScale = 1f;
        suraCore = false;
        vacuum = LoadAny("Assets/Free Slash VFX/Prefabs/Projectiles/Slash Projectile VFX Fire.prefab");
        vacuumScale = 1.6f;
        bossShot = Load("Fire/CFXR3 Hit Fire B (Air).prefab");
        enemyHit = LoadAny("Assets/Free Slash VFX/Prefabs/Impact.prefab");
        if (enemyHit == null) enemyHit = Load("Impacts/CFXR Hit A (Red).prefab");
        // 사망: 흙먼지가 퍼지는 자연스러운 효과 (보라색 퍼프는 어색해서 교체)
        enemyDeath = LoadAny("Assets/UnityTechnologies/ParticlePack/EffectExamples/Fire & Explosion Effects/Prefabs/DustExplosion.prefab");
        if (enemyDeath == null) enemyDeath = Load("Misc/CFXR3 Hit Misc F Smoke.prefab");
        bossDeath = enemyDeath;
        playerHit = Load("Impacts/CFXR Hit D 3D (Yellow).prefab");
    }

    static GameObject LoadAny(string path)
    {
        var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (go == null) Debug.LogWarning($"프리셋 에셋을 찾지 못했습니다: {path}");
        return go;
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
        if (hellfire != null) SetSkill("Hellfire", MakePooled("Fx_Hellfire", hellfire, hellfireRefDiameter, 2f, hellfireExtra, extraScale));
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
        if (sura != null) SetSkill("Sura", MakeProjectile("Fx_Sura", sura, suraScale, suraCore, suraExtra, extraScale));
        if (vacuum != null) SetSkill("Vacuum", MakeProjectile("Fx_Vacuum", vacuum, vacuumScale, false, vacuumExtra, extraScale * vacuumScale));

        if (bossShot != null)
        {
            var shot = MakeEnemyShot("Fx_BossShot", bossShot, bossShotScale);
            var data = AssetDatabase.LoadAssetAtPath<BossData>("Assets/Data/Boss_Basic.asset");
            if (data != null) { data.projectilePrefab = shot; EditorUtility.SetDirty(data); }
        }

        // 피격 / 사망 (풀 래퍼)
        var eHit = enemyHit != null ? MakePooled("Fx_EnemyHit", enemyHit, hitRefDiameter, 1.5f) : null;
        var eDeath = enemyDeath != null ? MakePooled("Fx_EnemyDeath", enemyDeath, deathRefDiameter, 1.5f) : null;
        var bDeath = bossDeath != null ? MakePooled("Fx_BossDeath", bossDeath, deathRefDiameter, 2f) : eDeath;
        var pHit = playerHit != null ? MakePooled("Fx_PlayerHit", playerHit, hitRefDiameter, 1.5f) : eHit;

        WirePrefab("Assets/Prefabs/Enemy_Basic.prefab", eHit, eDeath, 1.5f, 1f, Sfx.EnemyHit, Sfx.EnemyDeath);
        WirePrefab("Assets/Prefabs/Boss.prefab", eHit, bDeath, 4f, 2.5f, Sfx.EnemyHit, Sfx.EnemyDeath);

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
    static GameObject MakePooled(string name, GameObject fx, float refDiameter, float lifetime, GameObject extra = null, float extraScale = 1f)
    {
        var root = new GameObject(name);
        var vfx = root.AddComponent<PooledVfx>();
        var so = new SerializedObject(vfx);
        so.FindProperty("referenceDiameter").floatValue = refDiameter;
        so.FindProperty("lifetime").floatValue = lifetime;
        so.ApplyModifiedPropertiesWithoutUndo();
        Attach(root, fx, 1f);
        if (extra != null) Attach(root, extra, extraScale);
        return Save(root, name);
    }

    // 모델을 루트(원점) 기준으로 정렬: 가장 긴 축 → +Z(진행 방향), 길이를 targetLength로, 중심을 원점으로
    static void AlignModel(Transform model, float targetLength, Vector3 extraEuler)
    {
        var rs = model.GetComponentsInChildren<Renderer>();
        if (rs.Length == 0) return;

        Bounds Measure()
        {
            var b = rs[0].bounds;
            foreach (var r in rs) b.Encapsulate(r.bounds);
            return b;
        }

        var size = Measure().size;
        Quaternion align = Quaternion.identity;
        if (size.y >= size.x && size.y >= size.z) align = Quaternion.Euler(90f, 0f, 0f);   // 세로로 긴 검 → 눕힘
        else if (size.x >= size.z) align = Quaternion.Euler(0f, 90f, 0f);                    // 가로로 긴 검 → 앞쪽으로
        model.localRotation = Quaternion.Euler(extraEuler) * align;

        var b2 = Measure();
        float longest = Mathf.Max(b2.size.x, b2.size.y, b2.size.z);
        if (longest > 0.0001f) model.localScale = Vector3.one * (targetLength / longest);

        var b3 = Measure();
        model.position -= b3.center; // 루트가 원점이라 월드 중심 = 루트 기준 중심
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
            model.transform.localRotation = Quaternion.identity;
            model.transform.localScale = Vector3.one;
            foreach (var c in model.GetComponentsInChildren<Collider>(true)) Object.DestroyImmediate(c);

            // 모델의 가장 긴 축을 진행 방향(+Z)에 맞추고, 목표 길이로 스케일하고, 중심을 루트(이펙트 위치)에 맞춘다
            // → 이펙트와 무기가 같은 위치에서 같이 움직인다
            AlignModel(model.transform, bladeModelScale, bladeModelEuler);

            // 에셋의 무기 컨트롤러는 인챈트(빛) 메시를 시작할 때 숨기므로 항상 보이게 설정(Easy Weapons 모델용)
            var ctrl0 = model.GetComponentInChildren<EasyWeapon.weaponControllerAA>(true);
            if (ctrl0 != null)
            {
                var cso0 = new SerializedObject(ctrl0);
                cso0.FindProperty("enchantAlwaysVisable").boolValue = true;
                cso0.ApplyModifiedPropertiesWithoutUndo();
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
        trail.time = 0.2f;
        trail.startWidth = 0.25f;
        trail.endWidth = 0f;
        trail.startColor = trailColor;
        trail.endColor = new Color(trailColor.r, trailColor.g, trailColor.b, 0f);
        trail.sharedMaterial = trailMat;

        if (bladeFx != null) Attach(root, bladeFx, bladeFxScale);
        if (bladeExtra != null) Attach(root, bladeExtra, bladeExtraScale);
        return Save(root, "Fx_Blade");
    }

    // addCore: 이펙트가 잘 안 보일 때를 대비해 밝은 큐브 + 트레일(기존 임시 비주얼)을 함께 둔다
    static GameObject MakeProjectile(string name, GameObject fx, float scale, bool addCore = false, GameObject extra = null, float extraScale = 1f)
    {
        var root = new GameObject(name);
        root.AddComponent<Projectile>();
        Attach(root, fx, scale);
        if (extra != null) Attach(root, extra, extraScale);

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
        if (frostExtra != null) Attach(fxGo, frostExtra, extraScale);

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
