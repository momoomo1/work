using System;
using System.Collections.Generic;
using UnityEngine;

public class SkillManager : MonoBehaviour
{
    [SerializeField] List<SkillData> allSkills = new List<SkillData>(); // 선택지 후보 전체

    // 새 스킬 종류를 만들면 여기에 매핑만 추가
    static readonly Dictionary<SkillType, Type> skillClasses = new Dictionary<SkillType, Type>
    {
        { SkillType.Projectile, typeof(ProjectileSkill) },
        { SkillType.Explosion,  typeof(PlaceholderSkill) },
        { SkillType.Orbit,      typeof(PlaceholderSkill) },
        { SkillType.Zone,       typeof(PlaceholderSkill) },
    };

    readonly Dictionary<SkillData, SkillBase> owned = new Dictionary<SkillData, SkillBase>();

    public IReadOnlyList<SkillData> AllSkills => allSkills;
    public event Action<SkillData, int> OnSkillChanged; // (스킬, 현재 레벨)

    public bool Has(SkillData data) => owned.ContainsKey(data);
    public int GetLevel(SkillData data) => owned.TryGetValue(data, out var s) ? s.Level : 0;

    // 새로 얻으면 생성, 이미 있으면 레벨업. 진화 스킬을 얻으면 원본은 제거.
    public SkillBase AddSkill(SkillData data)
    {
        if (data == null) return null;

        if (owned.TryGetValue(data, out var skill))
        {
            skill.LevelUp();
            OnSkillChanged?.Invoke(data, skill.Level);
            return skill;
        }

        if (!skillClasses.TryGetValue(data.type, out var cls))
        {
            Debug.LogError($"SkillType {data.type}에 매핑된 클래스가 없습니다.");
            return null;
        }

        if (data.evolvesFrom != null && owned.TryGetValue(data.evolvesFrom, out var baseSkill))
        {
            Destroy(baseSkill.gameObject);
            owned.Remove(data.evolvesFrom);
        }

        var go = new GameObject($"Skill_{data.displayName}");
        go.transform.SetParent(transform, false);
        skill = (SkillBase)go.AddComponent(cls);
        skill.Init(data);
        owned[data] = skill;
        OnSkillChanged?.Invoke(data, skill.Level);
        return skill;
    }

    // 최대 레벨 미만인 보유 스킬 + 미획득 스킬 + 조건 충족한 진화 스킬 중 중복 없이 count개
    public List<SkillData> GetRandomChoices(int count)
    {
        var candidates = new List<SkillData>();
        foreach (var data in allSkills)
        {
            if (data == null) continue;
            if (data.IsEvolution)
            {
                // 원본이 최대 레벨이고 아직 진화 스킬이 없을 때만
                if (!owned.ContainsKey(data) &&
                    owned.TryGetValue(data.evolvesFrom, out var b) && b.IsMaxLevel)
                    candidates.Add(data);
            }
            else if (!owned.TryGetValue(data, out var s) || !s.IsMaxLevel)
            {
                candidates.Add(data);
            }
        }

        // 부분 셔플로 count개 선택
        int n = Mathf.Min(count, candidates.Count);
        for (int i = 0; i < n; i++)
        {
            int j = UnityEngine.Random.Range(i, candidates.Count);
            (candidates[i], candidates[j]) = (candidates[j], candidates[i]);
        }
        return candidates.GetRange(0, n);
    }
}
