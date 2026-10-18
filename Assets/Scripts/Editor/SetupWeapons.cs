using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// 플레이어/적/보스의 오른손 본에 무기 모델을 붙인다. (Humanoid 모델 기준)
// 무기마다 손에 쥐는 위치/방향이 달라서 아래 상수(로컬 회전/오프셋/스케일)를 조정하고 다시 실행하면 된다.
public static class SetupWeapons
{
    const string PlayerWeapon = "Assets/Free Slash VFX/Model/Katana/Low-Poly Katana.prefab";
    const string EnemyWeapon = "Assets/PurePoly/Free Fantasy RPG Weapons/Prefabs/PP_Theme_10_Dagger_002.prefab";
    const string BossWeapon = "Assets/PurePoly/Free Fantasy RPG Weapons/Prefabs/PP_Theme_11_Axe_One_Handed_002.prefab";

    [MenuItem("Tools/Setup/Attach Weapons")]
    internal static void Attach()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Undo.RegisterFullObjectHierarchyUndo(player, "Attach Player Weapon");
            AttachTo(player, PlayerWeapon, 1f, new Vector3(0f, 0f, 0f), Vector3.zero);
            EditorSceneManager.MarkSceneDirty(player.scene);
        }
        AttachToPrefab("Assets/Prefabs/Enemy_Basic.prefab", EnemyWeapon, 1f, new Vector3(0f, 0f, 0f), Vector3.zero);
        AttachToPrefab("Assets/Prefabs/Boss.prefab", BossWeapon, 1.8f, new Vector3(0f, 0f, 0f), Vector3.zero);
        AssetDatabase.SaveAssets();
        Debug.Log("무기 장착 완료. 위치/방향이 어색하면 SetupWeapons.cs 상단 상수와 아래 값을 조정해서 다시 실행하세요. 플레이어는 씬 저장(Ctrl+S) 필요.");
    }

    static void AttachToPrefab(string path, string weaponPath, float scale, Vector3 euler, Vector3 offset)
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) == null) return;
        var root = PrefabUtility.LoadPrefabContents(path);
        try
        {
            AttachTo(root, weaponPath, scale, euler, offset);
            PrefabUtility.SaveAsPrefabAsset(root, path);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
    }

    static void AttachTo(GameObject root, string weaponPath, float scale, Vector3 euler, Vector3 offset)
    {
        var weapon = AssetDatabase.LoadAssetAtPath<GameObject>(weaponPath);
        if (weapon == null) { Debug.LogWarning($"무기 프리팹을 찾지 못했습니다: {weaponPath}"); return; }

        var visual = root.transform.Find("Visual");
        var anim = visual != null ? visual.GetComponentInChildren<Animator>(true) : null;
        if (anim == null || anim.avatar == null || !anim.avatar.isHuman)
        {
            Debug.LogWarning($"{root.name}: Humanoid 아바타가 없어 무기를 붙이지 못했습니다. (Character Visuals를 먼저 적용하고 Rig를 Humanoid로 설정하세요)");
            return;
        }
        var hand = anim.GetBoneTransform(HumanBodyBones.RightHand);
        if (hand == null) { Debug.LogWarning($"{root.name}: 오른손 본을 찾지 못했습니다."); return; }

        // 기존 장착 무기 제거
        var old = hand.Find("Weapon");
        if (old != null) Object.DestroyImmediate(old.gameObject);

        var obj = (GameObject)PrefabUtility.InstantiatePrefab(weapon, hand);
        obj.name = "Weapon";
        obj.transform.localPosition = offset;
        obj.transform.localRotation = Quaternion.Euler(euler);
        // 본의 스케일(FBX 단위 변환 등)에 상관없이 월드 크기가 scale이 되도록 보정
        float s = hand.lossyScale.x > 0.0001f ? scale / hand.lossyScale.x : scale;
        obj.transform.localScale = Vector3.one * s;
        foreach (var c in obj.GetComponentsInChildren<Collider>(true)) Object.DestroyImmediate(c);
        foreach (var t in obj.GetComponentsInChildren<Transform>(true)) t.gameObject.layer = root.layer;
    }
}
