using System;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;

// 플레이어/적/보스의 비주얼 모델을 교체하고 Animator Controller(Idle/Run/Attack/Death)를 만들어 연결한다.
// 로직 스크립트는 건드리지 않고 "Visual" 자식 오브젝트만 교체한다.
public class SetupCharactersWindow : EditorWindow
{
    [Serializable]
    class Slot
    {
        public GameObject model;
        public AnimationClip idle, move, attack, death;
        public float scale = 1f;
        public float offsetY = -1.1f;     // 루트 중심(캡슐 중심) 기준으로 모델 발이 바닥에 오도록
        public float baseRunSpeed = 3f;
        public float attackRange = 0f;    // >0이면 플레이어 접근 시 Attack 재생 (적/보스)
        public bool playDeath;
        public bool foldout = true;
    }

    [SerializeField] Slot player = new Slot { offsetY = -1.1f, baseRunSpeed = 6f, playDeath = true };
    [SerializeField] Slot enemy = new Slot { offsetY = -1.1f, baseRunSpeed = 3f, attackRange = 1.6f };
    [SerializeField] Slot boss = new Slot { offsetY = -2.5f, scale = 2.5f, baseRunSpeed = 2.5f, attackRange = 3.2f };
    Vector2 scroll;

    [MenuItem("Tools/Setup/Character Visuals")]
    static void Open() => GetWindow<SetupCharactersWindow>("Character Visuals");

    void OnGUI()
    {
        EditorGUILayout.HelpBox("모델 프리팹(FBX 포함)과 애니메이션 클립을 넣고 Apply를 누르세요. 비워 둔 캐릭터는 건드리지 않습니다.\n" +
            "플레이어는 현재 씬의 Player 오브젝트, 적/보스는 Assets/Prefabs의 Enemy_Basic / Boss 프리팹에 적용됩니다.", MessageType.Info);
        scroll = EditorGUILayout.BeginScrollView(scroll);
        DrawSlot("Player", player);
        DrawSlot("Enemy (Enemy_Basic)", enemy);
        DrawSlot("Boss", boss);
        EditorGUILayout.EndScrollView();

        if (GUILayout.Button("Apply", GUILayout.Height(32))) Apply();
    }

    static void DrawSlot(string label, Slot s)
    {
        s.foldout = EditorGUILayout.Foldout(s.foldout, label, true);
        if (!s.foldout) return;
        EditorGUI.indentLevel++;
        s.model = (GameObject)EditorGUILayout.ObjectField("모델", s.model, typeof(GameObject), false);
        s.idle = (AnimationClip)EditorGUILayout.ObjectField("Idle", s.idle, typeof(AnimationClip), false);
        s.move = (AnimationClip)EditorGUILayout.ObjectField("Run / Walk", s.move, typeof(AnimationClip), false);
        s.attack = (AnimationClip)EditorGUILayout.ObjectField("Attack", s.attack, typeof(AnimationClip), false);
        s.death = (AnimationClip)EditorGUILayout.ObjectField("Death", s.death, typeof(AnimationClip), false);
        s.scale = EditorGUILayout.FloatField("스케일", s.scale);
        s.offsetY = EditorGUILayout.FloatField("Y 오프셋", s.offsetY);
        s.baseRunSpeed = EditorGUILayout.FloatField("기준 이동 속도", s.baseRunSpeed);
        s.attackRange = EditorGUILayout.FloatField("공격 모션 거리 (0=끔)", s.attackRange);
        s.playDeath = EditorGUILayout.Toggle("Death 재생", s.playDeath);
        EditorGUI.indentLevel--;
        EditorGUILayout.Space();
    }

    void Apply()
    {
        if (!Directory.Exists("Assets/Animations")) AssetDatabase.CreateFolder("Assets", "Animations");

        if (player.model != null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
            {
                Undo.RegisterFullObjectHierarchyUndo(p, "Player Visual");
                ApplyTo(p, player, "Player");
                EditorSceneManager.MarkSceneDirty(p.scene);
            }
            else Debug.LogWarning("씬에서 Player 태그 오브젝트를 찾지 못했습니다.");
        }
        ApplyToPrefab("Assets/Prefabs/Enemy_Basic.prefab", enemy, "Enemy");
        ApplyToPrefab("Assets/Prefabs/Boss.prefab", boss, "Boss");

        AssetDatabase.SaveAssets();
        Debug.Log("캐릭터 비주얼 적용 완료. 플레이어가 바뀌었다면 씬을 저장(Ctrl+S)하세요.");
    }

    static void ApplyToPrefab(string path, Slot slot, string name)
    {
        if (slot.model == null) return;
        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) == null)
        {
            Debug.LogWarning($"{path} 가 없습니다. 먼저 해당 Setup 메뉴를 실행하세요.");
            return;
        }
        var root = PrefabUtility.LoadPrefabContents(path);
        try
        {
            ApplyTo(root, slot, name);
            PrefabUtility.SaveAsPrefabAsset(root, path);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
    }

    static void ApplyTo(GameObject root, Slot slot, string name)
    {
        // 기존 임시 비주얼 제거 (플레이어는 루트의 캡슐 렌더러를 끔)
        var old = root.transform.Find("Visual");
        if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
        if (root.TryGetComponent<MeshRenderer>(out var mr)) mr.enabled = false;

        var visual = (GameObject)PrefabUtility.InstantiatePrefab(slot.model, root.transform);
        visual.name = "Visual";
        visual.transform.localPosition = new Vector3(0f, slot.offsetY, 0f);
        visual.transform.localRotation = Quaternion.identity;
        visual.transform.localScale = Vector3.one * slot.scale;

        // 모델에 딸린 콜라이더는 루트 콜라이더와 겹치므로 제거, 레이어는 루트와 동일하게
        foreach (var c in visual.GetComponentsInChildren<Collider>(true)) UnityEngine.Object.DestroyImmediate(c);
        foreach (var t in visual.GetComponentsInChildren<Transform>(true)) t.gameObject.layer = root.layer;

        var anim = visual.GetComponentInChildren<Animator>(true);
        if (anim == null) anim = visual.AddComponent<Animator>();
        anim.applyRootMotion = false;
        anim.runtimeAnimatorController = BuildController(name, slot);

        var ca = root.GetComponent<CharacterAnimator>();
        if (ca == null) ca = root.AddComponent<CharacterAnimator>();
        var so = new SerializedObject(ca);
        so.FindProperty("animator").objectReferenceValue = anim;
        so.FindProperty("baseRunSpeed").floatValue = slot.baseRunSpeed;
        so.FindProperty("attackRange").floatValue = slot.attackRange;
        so.FindProperty("playDeath").boolValue = slot.playDeath;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static AnimatorController BuildController(string name, Slot s)
    {
        string path = $"Assets/Animations/{name}.controller";
        AssetDatabase.DeleteAsset(path);
        var c = AnimatorController.CreateAnimatorControllerAtPath(path);

        c.AddParameter("Speed", AnimatorControllerParameterType.Float);
        c.AddParameter(new AnimatorControllerParameter { name = "RunMul", type = AnimatorControllerParameterType.Float, defaultFloat = 1f });
        c.AddParameter("Attack", AnimatorControllerParameterType.Trigger);
        c.AddParameter("Death", AnimatorControllerParameterType.Trigger);

        var sm = c.layers[0].stateMachine;
        var idle = sm.AddState("Idle");
        idle.motion = s.idle;
        if (s.idle == null && s.move != null)
        {
            // Idle 클립이 없으면 Run의 첫 포즈에 정지시켜 T-pose로 서 있는 것을 방지
            idle.motion = s.move;
            idle.speed = 0f;
        }
        sm.defaultState = idle;

        if (s.move != null)
        {
            var run = sm.AddState("Run");
            run.motion = s.move;
            run.speedParameterActive = true;      // 이동 속도에 비례해 재생 속도 조절
            run.speedParameter = "RunMul";

            var toRun = idle.AddTransition(run);
            toRun.hasExitTime = false;
            toRun.duration = 0.1f;
            toRun.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");
            var toIdle = run.AddTransition(idle);
            toIdle.hasExitTime = false;
            toIdle.duration = 0.1f;
            toIdle.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");
        }

        if (s.attack != null)
        {
            var atk = sm.AddState("Attack");
            atk.motion = s.attack;
            var any = sm.AddAnyStateTransition(atk);
            any.hasExitTime = false;
            any.duration = 0.05f;
            any.canTransitionToSelf = false;
            any.AddCondition(AnimatorConditionMode.If, 0f, "Attack");
            var back = atk.AddTransition(idle);
            back.hasExitTime = true;
            back.exitTime = 0.9f;
            back.duration = 0.1f;
        }

        if (s.death != null)
        {
            var die = sm.AddState("Death");
            die.motion = s.death;
            var any = sm.AddAnyStateTransition(die);
            any.hasExitTime = false;
            any.duration = 0.05f;
            any.canTransitionToSelf = false;
            any.AddCondition(AnimatorConditionMode.If, 0f, "Death");
        }
        return c;
    }
}
