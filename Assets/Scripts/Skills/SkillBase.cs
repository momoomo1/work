using UnityEngine;

// 플레이어 자식 오브젝트에 붙는 스킬 동작의 기반 클래스
public abstract class SkillBase : MonoBehaviour
{
    public SkillData Data { get; private set; }
    public int Level { get; private set; }
    public SkillLevelStat Stat => Data.levels[Level - 1];
    public bool IsMaxLevel => Level >= Data.MaxLevel;
    public Transform Owner => transform.parent != null ? transform.parent : transform;

    float cooldownTimer;

    // 스킬 바 UI용: 다음 발동까지 남은 시간과 비율 (1 = 방금 발동, 0 = 곧 발동)
    public float CooldownRemaining => Mathf.Max(0f, cooldownTimer);
    public float CooldownRatio
    {
        get
        {
            float total = Stat.cooldown / PlayerStats.AttackSpeed;
            return total > 0f ? Mathf.Clamp01(cooldownTimer / total) : 0f;
        }
    }

    public void Init(SkillData data)
    {
        Data = data;
        Level = 1;
        cooldownTimer = 0f;
        OnInit();
    }

    public void LevelUp()
    {
        if (IsMaxLevel) return;
        Level++;
        OnLevelUp();
    }

    // 스킬이 실제로 발동했을 때 호출 (대상이 없어서 쏘지 못했으면 호출하지 않음)
    protected void PlayCastSound() => AudioManager.PlayClip(Data.castSfx, 0.8f);

    protected virtual void OnInit() { }
    protected virtual void OnLevelUp() { }

    // 쿨타임마다 호출. 상시 유지형 스킬은 OnInit/OnLevelUp에서 처리하고 Activate는 비워도 됨.
    protected abstract void Activate();

    protected virtual void Update()
    {
        if (Data == null) return;
        cooldownTimer -= Time.deltaTime;
        if (cooldownTimer > 0f) return;
        cooldownTimer = Stat.cooldown / PlayerStats.AttackSpeed;
        Activate();
    }
}
