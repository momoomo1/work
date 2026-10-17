using UnityEngine;

[CreateAssetMenu(menuName = "Game/Skill Data", fileName = "SkillData")]
public class SkillData : ScriptableObject
{
    public string id;
    public string displayName;
    [TextArea] public string description;
    public Sprite icon;
    public SkillType type;
    public GameObject prefab;
    [Tooltip("보조 비주얼 (헬파이어의 경고 원 등). 파티클 등으로 교체 가능")]
    public GameObject secondaryPrefab;
    public SkillLevelStat[] levels = new SkillLevelStat[1] { new SkillLevelStat() }; // levels[0] = 레벨 1

    [Header("진화")]
    public SkillData evolvesFrom; // 비어있지 않으면 진화 스킬 (원본이 최대 레벨일 때 후보)

    public int MaxLevel => levels.Length;
    public bool IsEvolution => evolvesFrom != null;
}
