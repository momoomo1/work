using System;
using UnityEngine;

public class Health : MonoBehaviour, IDamageable
{
    [SerializeField] float maxHealth = 100f;
    [SerializeField] float currentHealth = 100f;

    public event Action OnDeath;
    public event Action<float, float> OnChanged; // (current, max)

    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;
    public bool IsDead => currentHealth <= 0f;
    public bool Invulnerable { get; set; } // true인 동안 피해를 받지 않음 (대쉬 등)

    // 풀에서 재사용할 때도 호출
    void OnEnable() => ResetHealth();

    public void ResetHealth()
    {
        currentHealth = maxHealth;
        OnChanged?.Invoke(currentHealth, maxHealth);
    }

    public void SetMaxHealth(float value, bool refill = true)
    {
        maxHealth = value;
        currentHealth = refill ? maxHealth : Mathf.Min(currentHealth, maxHealth);
        OnChanged?.Invoke(currentHealth, maxHealth);
    }

    public void TakeDamage(float amount)
    {
        if (IsDead || Invulnerable || amount <= 0f) return;
        currentHealth = Mathf.Max(0f, currentHealth - amount);
        OnChanged?.Invoke(currentHealth, maxHealth);
        if (currentHealth <= 0f) OnDeath?.Invoke();
    }

    public void Heal(float amount)
    {
        if (IsDead || amount <= 0f) return;
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        OnChanged?.Invoke(currentHealth, maxHealth);
    }
}
