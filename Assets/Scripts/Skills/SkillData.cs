using UnityEngine;

[CreateAssetMenu(menuName = "Game/Skill Data", fileName = "SkillData")]
public class SkillData : ScriptableObject
{
    public string id;
    public string displayName;
    [TextArea] public string description;
    public Sprite icon;
    public SkillType type;
    [Tooltip("레벨업 카드로 뽑힐 확률 가중치. 1이 보통, 작을수록 드물게 나옴")]
    public float choiceWeight = 1f;
    public GameObject prefab;
    public AudioClip castSfx; // 발동 사운드 (비워두면 무음)
    [Tooltip("보조 비주얼 (헬파이어의 경고 원 등). 파티클 등으로 교체 가능")]
    public GameObject secondaryPrefab;
    public SkillLevelStat[] levels = new SkillLevelStat[1] { new SkillLevelStat() }; // levels[0] = 레벨 1

    [Header("이펙트 보정 (근접 베기)")]
    [Tooltip("이펙트가 옆/뒤로 틀어져 보이면 90, -90, 180으로 보정 (자동 보정 위에 더해짐)")]
    public float vfxYawOffset;
    [Tooltip("이펙트가 놓이는 월드 높이 (플레이어 중심이 약 1.1)")]
    public float vfxHeight = 1.1f;
    [Tooltip("이펙트를 플레이어에서 베는 방향으로 얼마나 떨어뜨려 띄울지 (클수록 멀리서 보임)")]
    public float vfxForwardOffset = 1.8f;

    [Header("진화")]
    public SkillData evolvesFrom; // 비어있지 않으면 진화 스킬 (원본이 최대 레벨일 때 후보)

    public int MaxLevel => levels.Length;
    public bool IsEvolution => evolvesFrom != null;
}
