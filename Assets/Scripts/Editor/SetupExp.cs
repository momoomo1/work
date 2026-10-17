using UnityEditor;
using UnityEngine;

public static class SetupExp
{
    [MenuItem("Tools/Setup/Create Exp Orb and Experience")]
    static void Create()
    {
        foreach (var f in new[] { "Assets/Prefabs", "Assets/Materials" })
            if (!AssetDatabase.IsValidFolder(f)) AssetDatabase.CreateFolder("Assets", f.Substring(7));

        var mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/ExpOrb.mat");
        if (mat == null)
        {
            mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            AssetDatabase.CreateAsset(mat, "Assets/Materials/ExpOrb.mat");
        }
        var color = new Color(0.3f, 1f, 0.4f);
        mat.SetColor("_BaseColor", color);
        mat.color = color;
        EditorUtility.SetDirty(mat);

        // 루트(로직) + Visual(Sphere)
        var root = new GameObject("ExpOrb");
        root.AddComponent<ExpOrb>();
        var visual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        visual.name = "Visual";
        Object.DestroyImmediate(visual.GetComponent<SphereCollider>());
        visual.transform.SetParent(root.transform, false);
        visual.transform.localScale = Vector3.one * 0.3f;
        visual.GetComponent<MeshRenderer>().sharedMaterial = mat;
        var prefab = PrefabUtility.SaveAsPrefabAsset(root, "Assets/Prefabs/ExpOrb.prefab");
        Object.DestroyImmediate(root);

        var spawner = Object.FindFirstObjectByType<ExpOrbSpawner>();
        if (spawner == null)
        {
            var go = new GameObject("ExpOrbSpawner");
            spawner = go.AddComponent<ExpOrbSpawner>();
            Undo.RegisterCreatedObjectUndo(go, "Create ExpOrbSpawner");
        }
        var so = new SerializedObject(spawner);
        so.FindProperty("orbPrefab").objectReferenceValue = prefab;
        so.ApplyModifiedProperties();

        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && player.GetComponent<PlayerExperience>() == null)
            Undo.AddComponent<PlayerExperience>(player);

        AssetDatabase.SaveAssets();
        Debug.Log("경험치 구슬/PlayerExperience 설정 완료. 씬을 저장(Ctrl+S)하세요.");
    }
}
