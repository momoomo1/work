using System;
using System.Collections.Generic;
using UnityEngine;

public class SkillManager : MonoBehaviour
{
    [SerializeField] List<SkillData> allSkills = new List<SkillData>(); // 선택지 후보 전체
    [SerializeField] List<SkillData> startingSkills = new List<SkillData>(); // 게임 시작 때 자동으로 얻는 스킬 (예: 기본 공격)

    // 새 스킬 종류를 만들면 여기에 매핑만 추가
    static readonly Dictionary<SkillType, Type> skillClasses = new Dictionary<SkillType, Type>
    {
        { SkillType.Projectile, typeof(ProjectileSkill) },
        { SkillType.Explosion,  typeof(ExplosionSkill) },
        { SkillType.Orbit,      typeof(OrbitSkill) },
        { SkillType.Zone,       typeof(ZoneSkill) },
        { SkillType.Passive,    typeof(PassiveSkill) },
        { SkillType.Melee,      typeof(MeleeSlashSkill) },
    };

    // 같은 종류 안에서 특별한 동작이 필요한 스킬(진화 스킬 등)은 id로 클래스를 덮어씀
    static readonly Dictionary<string, Type> idOverrides = new Dictionary<string, Type>
    {
        { "vacuum", typeof(VacuumSkill) },
    };

    // 진화로 대체된 원본 스킬: 다시 선택지에 나오지 않게 함
    readonly HashSet<SkillData> evolvedAway = new HashSet<SkillData>();

    readonly Dictionary<SkillData, SkillBase> owned = new Dictionary<SkillData, SkillBase>();

    public IReadOnlyList<SkillData> AllSkills => allSkills;
    public event Action<SkillData, int> OnSkillChanged; // (스킬, 현재 레벨)

    void Start()
    {
        foreach (var s in startingSkills)
            if (s != null && !Has(s)) AddSkill(s);
    }

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

        Type cls = null;
        if (data.id == null || !idOverrides.TryGetValue(data.id, out cls)) skillClasses.TryGetValue(data.type, out cls);
        if (cls == null)
        {
            Debug.LogError($"SkillType {data.type}에 매핑된 클래스가 없습니다.");
            return null;
        }

        // 진화: 원본 스킬 제거 후 진화 스킬 추가
        if (data.evolvesFrom != null)
        {
            if (owned.TryGetValue(data.evolvesFrom, out var baseSkill))
            {
                Destroy(baseSkill.gameObject);
                owned.Remove(data.evolvesFrom);
                OnSkillChanged?.Invoke(data.evolvesFrom, 0);
            }
            evolvedAway.Add(data.evolvesFrom);
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
            if (data == null || evolvedAway.Contains(data)) continue;
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

        // 가중치(choiceWeight)에 따라 중복 없이 count개 선택 (가중치가 작은 카드는 드물게 나옴)
        int n = Mathf.Min(count, candidates.Count);
        var result = new List<SkillData>(n);
        for (int k = 0; k < n; k++)
        {
            float total = 0f;
            foreach (var c in candidates) total += Mathf.Max(0.01f, c.choiceWeight);
            float roll = UnityEngine.Random.value * total;
            int pick = candidates.Count - 1;
            for (int i = 0; i < candidates.Count; i++)
            {
                roll -= Mathf.Max(0.01f, candidates[i].choiceWeight);
                if (roll <= 0f) { pick = i; break; }
            }
            result.Add(candidates[pick]);
            candidates.RemoveAt(pick);
        }
        return result;
    }
}
