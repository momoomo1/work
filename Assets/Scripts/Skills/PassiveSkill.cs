using UnityEngine;

// 능력치 강화 스킬 (공격하지 않음). SkillData.id로 종류를 구분하고, 레벨의 value가 증가량이다.
//  max_health : 최대 체력 +value (그만큼 현재 체력도 회복)
// 새 패시브를 추가하려면 Apply의 switch에 id를 추가하면 된다.
public class PassiveSkill : SkillBase
{
    protected override void OnInit() => Apply(Stat.value);
    protected override void OnLevelUp() => Apply(Stat.value); // 레벨업할 때마다 그 레벨의 증가량만큼 추가

    void Apply(float value)
    {
        switch (Data.id)
        {
            case "max_health":
                if (Owner.TryGetComponent<Health>(out var health))
                {
                    health.SetMaxHealth(health.MaxHealth + value, false);
                    health.Heal(value);
                }
                break;
            default:
                Debug.LogWarning($"알 수 없는 패시브 id: {Data.id}");
                break;
        }
    }

    // 지속 효과가 아니라 획득/레벨업 때 한 번 적용하므로 쿨타임 동작은 없음
    protected override void Activate() { }
}
