using System;
using UnityEngine;

public class PlayerExperience : MonoBehaviour
{
    [Header("레벨 곡선: 필요 경험치 = baseExp × growth^(level-1) + addPerLevel × (level-1)")]
    [SerializeField] float baseExp = 5f;
    [SerializeField] float growth = 1.15f;
    [SerializeField] float addPerLevel = 3f;

    [Header("획득")]
    [SerializeField] float pickupRange = 4f;

    [SerializeField] int level = 1;
    [SerializeField] int currentExp;

    public static PlayerExperience Current { get; private set; }

    public event Action<int> OnLevelUp;          // (새 레벨)
    public event Action<int, int> OnExpChanged;  // (현재 경험치, 다음 레벨 필요 경험치)

    public int Level => level;
    public int CurrentExp => currentExp;
    public int ExpToNext => GetRequired(level);
    public float PickupRange => pickupRange;

    int GetRequired(int lv) => Mathf.RoundToInt(baseExp * Mathf.Pow(growth, lv - 1) + addPerLevel * (lv - 1));

    void Awake() => Current = this;

    void OnDestroy()
    {
        if (Current == this) Current = null;
    }

    void Start() => OnExpChanged?.Invoke(currentExp, ExpToNext);

    public void AddExp(int amount)
    {
        if (amount <= 0) return;
        currentExp += amount;

        // 한 번에 여러 레벨이 오를 수 있음
        while (currentExp >= ExpToNext)
        {
            currentExp -= ExpToNext;
            level++;
            OnLevelUp?.Invoke(level);
        }
        OnExpChanged?.Invoke(currentExp, ExpToNext);
    }
}
