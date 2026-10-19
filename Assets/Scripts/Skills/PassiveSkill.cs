using UnityEngine;

// 능력치 강화 스킬 (공격하지 않음). SkillData.id로 종류를 구분하고, 레벨의 value가 증가량이다.
//  attack_speed / move_speed : 전역 배율 +value (PlayerStats)
//  dash : 충전 시간 -value초, 칸 +count (레벨 1은 기본 제공이라 value 0, count 0)
//  max_health : 최대 체력 +value, 그리고 체력 회복 (늘어난 만큼 + 최대 체력의 25%)
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
                    health.Heal(value + health.MaxHealth * 0.25f); // 늘어난 만큼 + 최대 체력의 25% 회복
                }
                break;
            case "attack_speed":
                PlayerStats.AttackSpeed += value; // value 0.1 = 공격 속도 +10%
                break;
            case "move_speed":
                PlayerStats.MoveSpeed += value;
                break;
            case "dash":
                // value = 충전 시간 감소(초), count = 이 레벨에서 늘어나는 대쉬 칸 수
                if (Owner.TryGetComponent<PlayerDash>(out var dash)) dash.Upgrade(value, Stat.count);
                break;
            default:
                Debug.LogWarning($"알 수 없는 패시브 id: {Data.id}");
                break;
        }
    }

    // 지속 효과가 아니라 획득/레벨업 때 한 번 적용하므로 쿨타임 동작은 없음
    protected override void Activate() { }
}
