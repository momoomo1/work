using UnityEditor;
using UnityEngine;

// Unity Particle Pack 이펙트를 스킬 프리팹에 연결. 아래 경로만 바꾸면 다른 이펙트로 교체 가능.
public static class SetupVfx
{
    const string Pack = "Assets/UnityTechnologies/ParticlePack/EffectExamples/";
    const string ExplosionFx = Pack + "Fire & Explosion Effects/Prefabs/BigExplosion.prefab";
    const string SuraFx = Pack + "Magic Effects/Prefabs/IceLance.prefab";
    const string VacuumFx = Pack + "Fire & Explosion Effects/Prefabs/FireBall.prefab";
    const string BossShotFx = Pack + "Fire & Explosion Effects/Prefabs/FireBall.prefab";

    [MenuItem("Tools/Setup/Apply Particle Pack VFX")]
    static void Apply()
    {
        // 헬파이어 폭발: referenceDiameter는 원본 이펙트 크기에 맞춰 조절
        var boom = MakeFx("Explosion_Hellfire_FX", ExplosionFx, 1f, false);
        SetSkillPrefab("Assets/Data/Skills/Skill_Hellfire.asset", boom, false);

        // 수라참 / 진공참 투사체
        var sura = MakeFx("Projectile_Sura_FX", SuraFx, 1f, true);
        SetSkillPrefab("Assets/Data/Skills/Skill_Sura.asset", sura, false);
        var vacuum = MakeFx("Projectile_Vacuum_FX", VacuumFx, 2f, true);
        SetSkillPrefab("Assets/Data/Skills/Skill_Vacuum.asset", vacuum, false);

        // 보스 투사체
        var shot = MakeEnemyShot("BossProjectile_FX", BossShotFx, 0.6f);
        var bossData = AssetDatabase.LoadAssetAtPath<BossData>("Assets/Data/Boss_Basic.asset");
        if (bossData != null && shot != null)
        {
            bossData.projectilePrefab = shot;
            EditorUtility.SetDirty(bossData);
        }

        AssetDatabase.SaveAssets();
        Debug.Log("Particle Pack 이펙트 적용 완료. 원본 임시 프리팹(큐브/구체)은 그대로 남아 있습니다.");
    }

    // projectile=true: Projectile 컴포넌트를 붙인 투사체, false: PooledVfx(폭발 등)
    static GameObject MakeFx(string name, string fxPath, float fxScale, bool projectile)
    {
        var fx = AssetDatabase.LoadAssetAtPath<GameObject>(fxPath);
        if (fx == null) { Debug.LogError($"이펙트를 찾을 수 없습니다: {fxPath}"); return null; }

        var root = new GameObject(name);
        if (projectile) root.AddComponent<Projectile>();
        else root.AddComponent<PooledVfx>();
        Attach(root, fx, fxScale);

        var prefab = PrefabUtility.SaveAsPrefabAsset(root, $"Assets/Prefabs/{name}.prefab");
        Object.DestroyImmediate(root);
        return prefab;
    }

    static GameObject MakeEnemyShot(string name, string fxPath, float fxScale)
    {
        var fx = AssetDatabase.LoadAssetAtPath<GameObject>(fxPath);
        if (fx == null) { Debug.LogError($"이펙트를 찾을 수 없습니다: {fxPath}"); return null; }

        var root = new GameObject(name);
        root.AddComponent<EnemyProjectile>();
        Attach(root, fx, fxScale);

        var prefab = PrefabUtility.SaveAsPrefabAsset(root, $"Assets/Prefabs/{name}.prefab");
        Object.DestroyImmediate(root);
        return prefab;
    }

    static void Attach(GameObject root, GameObject fxPrefab, float scale)
    {
        var child = (GameObject)PrefabUtility.InstantiatePrefab(fxPrefab, root.transform);
        child.name = "Fx";
        child.transform.localPosition = Vector3.zero;
        child.transform.localRotation = Quaternion.identity;
        child.transform.localScale = Vector3.one * scale;
    }

    static void SetSkillPrefab(string path, GameObject prefab, bool secondary)
    {
        var data = AssetDatabase.LoadAssetAtPath<SkillData>(path);
        if (data == null || prefab == null) { Debug.LogWarning($"건너뜀: {path}"); return; }
        if (secondary) data.secondaryPrefab = prefab; else data.prefab = prefab;
        EditorUtility.SetDirty(data);
    }
}
