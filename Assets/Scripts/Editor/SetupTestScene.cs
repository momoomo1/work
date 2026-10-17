using UnityEditor;
using UnityEngine;

public static class SetupTestScene
{
    [MenuItem("Tools/Setup/Create Test Scene")]
    static void Create()
    {
        // 기존 테스트 오브젝트 제거 후 재생성
        foreach (var n in new[] { "Ground", "Player" })
        {
            var old = GameObject.Find(n);
            if (old != null) Undo.DestroyObjectImmediate(old);
        }

        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.localScale = new Vector3(10f, 1f, 10f); // 100x100
        Undo.RegisterCreatedObjectUndo(ground, "Create Ground");

        var player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Player";
        player.tag = "Player";
        player.transform.position = new Vector3(0f, 1.1f, 0f);
        Object.DestroyImmediate(player.GetComponent<CapsuleCollider>());
        player.AddComponent<CharacterController>();
        var pc = player.AddComponent<PlayerController>();
        Undo.RegisterCreatedObjectUndo(player, "Create Player");

        var cam = Camera.main;
        if (cam == null)
        {
            var go = new GameObject("Main Camera");
            go.tag = "MainCamera";
            cam = go.AddComponent<Camera>();
            go.AddComponent<AudioListener>();
        }
        var follow = cam.GetComponent<CameraFollow>();
        if (follow == null) follow = cam.gameObject.AddComponent<CameraFollow>();

        var so = new SerializedObject(follow);
        so.FindProperty("target").objectReferenceValue = player.transform;
        so.ApplyModifiedProperties();
        var pso = new SerializedObject(pc);
        pso.FindProperty("cameraTransform").objectReferenceValue = cam.transform;
        pso.ApplyModifiedProperties();

        Selection.activeGameObject = player;
        Debug.Log("테스트 씬 생성 완료. 씬을 저장(Ctrl+S)하세요.");
    }
}
