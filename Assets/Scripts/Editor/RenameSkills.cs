using UnityEditor;
using UnityEngine;

// 스킬 이름/설명만 바꾼다 (프리팹/수치 연결은 건드리지 않음)
public static class RenameSkills
{
    [MenuItem("Tools/Setup/Rename Skills (Hellfire / Meteor)")]
    static void Rename()
    {
        Set("Vacuum", "헬파이어", "수라참의 강화형. 거대한 불덩이가 적을 끌어당기며 관통한다.");
        Set("Hellfire", "메테오", "적이 가장 밀집한 곳에 경고 후 폭발을 일으킨다.");
        AssetDatabase.SaveAssets();
        Debug.Log("스킬 이름 변경 완료: 진공참 → 헬파이어, 기존 헬파이어 → 메테오");
    }

    static void Set(string key, string name, string description)
    {
        var data = AssetDatabase.LoadAssetAtPath<SkillData>($"Assets/Data/Skills/Skill_{key}.asset");
        if (data == null) { Debug.LogWarning($"Skill_{key} 에셋을 찾지 못했습니다."); return; }
        data.displayName = name;
        data.description = description;
        EditorUtility.SetDirty(data);
    }
}
