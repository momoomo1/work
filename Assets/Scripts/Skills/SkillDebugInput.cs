using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;

// 디버그용: 1~5 = SkillManager.AllSkills[0~4] 추가/레벨업, 0 = 선택지 3개 출력
[RequireComponent(typeof(SkillManager))]
public class SkillDebugInput : MonoBehaviour
{
    SkillManager manager;

    void Awake() => manager = GetComponent<SkillManager>();

    void Update()
    {
        var kb = Keyboard.current;
        if (kb == null || LevelUpUI.BlocksInput) return; // 레벨업 카드 선택과 숫자키 충돌 방지

        var keys = new[] { kb.digit1Key, kb.digit2Key, kb.digit3Key, kb.digit4Key, kb.digit5Key };
        for (int i = 0; i < keys.Length; i++)
        {
            if (!keys[i].wasPressedThisFrame) continue;
            if (i >= manager.AllSkills.Count || manager.AllSkills[i] == null)
            {
                Debug.Log($"[Skill] {i + 1}번 슬롯에 SkillData가 없습니다.");
                continue;
            }
            var skill = manager.AddSkill(manager.AllSkills[i]);
            if (skill != null) LogSkill(skill);
        }

        if (kb.digit0Key.wasPressedThisFrame)
        {
            var sb = new StringBuilder("[Skill] 선택지: ");
            foreach (var c in manager.GetRandomChoices(3)) sb.Append(c.displayName).Append(", ");
            Debug.Log(sb.ToString());
        }
    }

    static void LogSkill(SkillBase s)
    {
        var st = s.Stat;
        Debug.Log($"[Skill] {s.Data.displayName} Lv{s.Level}/{s.Data.MaxLevel} | 데미지 {st.damage}, 쿨타임 {st.cooldown}, 개수 {st.count}, " +
                  $"범위 {st.area}, 속도 {st.speed}, 관통 {st.pierceCount}, 지속 {st.duration}, 틱 {st.tickInterval}");
    }
}
