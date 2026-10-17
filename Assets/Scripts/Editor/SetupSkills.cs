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
        sura.levels = new SkillLevelStat[5];
        for (int i = 0; i < 5; i++)
            sura.levels[i] = new SkillLevelStat
            {
                damage = 10f + 5f * i, cooldown = 1.2f - 0.1f * i, count = 1 + i / 2,
                area = 0.5f, speed = 15f, pierceCount = 1 + i, duration = 1.5f, tickInterval = 0f
            };
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
