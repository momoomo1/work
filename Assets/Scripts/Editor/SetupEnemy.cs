using UnityEditor;
using UnityEngine;

public static class SetupEnemy
{
    const string PrefabPath = "Assets/Prefabs/Enemy_Basic.prefab";
    const string DataPath = "Assets/Data/Enemy_Basic.asset";
    const string MatPath = "Assets/Materials/Enemy_Red.mat";

    [MenuItem("Tools/Setup/Create Enemy Assets and Spawner")]
    static void Create()
    {
        EnsureFolder("Assets/Prefabs");
        EnsureFolder("Assets/Data");
        EnsureFolder("Assets/Materials");

        // 빨간 머티리얼
        var mat = AssetDatabase.LoadAssetAtPath<Material>(MatPath);
        if (mat == null)
        {
            mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            AssetDatabase.CreateAsset(mat, MatPath);
        }
        mat.SetColor("_BaseColor", Color.red);
        mat.color = Color.red;
        EditorUtility.SetDirty(mat);

        // 프리팹: 루트(로직) + Visual 자식(캡슐, 나중에 교체)
        var root = new GameObject("Enemy_Basic");
        var col = root.AddComponent<CapsuleCollider>();
        col.isTrigger = true; // 플레이어 CharacterController를 막지 않음, 스킬 오버랩 판정용
        col.height = 2f;
        col.radius = 0.5f;
        root.AddComponent<Health>();
        root.AddComponent<Enemy>();

        var visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        visual.name = "Visual";
        Object.DestroyImmediate(visual.GetComponent<CapsuleCollider>());
        visual.transform.SetParent(root.transform, false);
        visual.GetComponent<MeshRenderer>().sharedMaterial = mat;

        int enemyLayer = EnsureLayer("Enemy");
        foreach (var t in root.GetComponentsInChildren<Transform>(true)) t.gameObject.layer = enemyLayer;

        var prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        Object.DestroyImmediate(root);

        // EnemyData 에셋
        var data = AssetDatabase.LoadAssetAtPath<EnemyData>(DataPath);
        if (data == null)
        {
            data = ScriptableObject.CreateInstance<EnemyData>();
            AssetDatabase.CreateAsset(data, DataPath);
        }
        data.displayName = "Basic Enemy";
        data.maxHealth = 30f;
        data.moveSpeed = 3f;
        data.contactDamage = 5f;
        data.attackInterval = 1f;
        data.expReward = 1;
        data.prefab = prefab;
        EditorUtility.SetDirty(data);

        // 씬: 플레이어에 Health, 스포너 배치
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && player.GetComponent<Health>() == null)
            Undo.AddComponent<Health>(player);

        var spawner = Object.FindFirstObjectByType<EnemySpawner>();
        if (spawner == null)
        {
            var go = new GameObject("EnemySpawner");
            spawner = go.AddComponent<EnemySpawner>();
            Undo.RegisterCreatedObjectUndo(go, "Create EnemySpawner");
        }
        var so = new SerializedObject(spawner);
        var types = so.FindProperty("enemyTypes");
        types.arraySize = 1;
        types.GetArrayElementAtIndex(0).objectReferenceValue = data;
        if (player != null) so.FindProperty("player").objectReferenceValue = player.transform;
        so.ApplyModifiedProperties();

        // 기본 공격용 투사체 프리팹 + 플레이어 공격 컴포넌트
        var projPrefab = CreateProjectilePrefab();
        if (player != null)
        {
            var atk = player.GetComponent<PlayerAttack>();
            if (atk == null) atk = Undo.AddComponent<PlayerAttack>(player);
            var aso = new SerializedObject(atk);
            aso.FindProperty("projectilePrefab").objectReferenceValue = projPrefab;
            aso.ApplyModifiedProperties();
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Selection.activeObject = data;
        Debug.Log("적 프리팹/EnemyData/스포너 생성 완료. 씬을 저장(Ctrl+S)하세요.");
    }

    static GameObject CreateProjectilePrefab()
    {
        var mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Projectile_Yellow.mat");
        if (mat == null)
        {
            mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            AssetDatabase.CreateAsset(mat, "Assets/Materials/Projectile_Yellow.mat");
        }
        mat.SetColor("_BaseColor", Color.yellow);
        mat.color = Color.yellow;
        EditorUtility.SetDirty(mat);

        var root = new GameObject("Projectile_Basic");
        root.AddComponent<Projectile>();
        var visual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        visual.name = "Visual";
        Object.DestroyImmediate(visual.GetComponent<SphereCollider>());
        visual.transform.SetParent(root.transform, false);
        visual.transform.localScale = Vector3.one * 0.4f;
        visual.GetComponent<MeshRenderer>().sharedMaterial = mat;

        var prefab = PrefabUtility.SaveAsPrefabAsset(root, "Assets/Prefabs/Projectile_Basic.prefab");
        Object.DestroyImmediate(root);
        return prefab;
    }

    // 레이어가 없으면 비어있는 첫 슬롯(8~31)에 추가
    static int EnsureLayer(string name)
    {
        int existing = LayerMask.NameToLayer(name);
        if (existing >= 0) return existing;

        var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        var layers = tagManager.FindProperty("layers");
        for (int i = 8; i < layers.arraySize; i++)
        {
            var sp = layers.GetArrayElementAtIndex(i);
            if (string.IsNullOrEmpty(sp.stringValue))
            {
                sp.stringValue = name;
                tagManager.ApplyModifiedProperties();
                return i;
            }
        }
        Debug.LogError("빈 레이어 슬롯이 없습니다.");
        return 0;
    }

    static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        int i = path.LastIndexOf('/');
        AssetDatabase.CreateFolder(path.Substring(0, i), path.Substring(i + 1));
    }
}
