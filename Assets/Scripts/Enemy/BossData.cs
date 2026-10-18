using UnityEngine;

[CreateAssetMenu(menuName = "Game/Boss Data", fileName = "BossData")]
public class BossData : ScriptableObject
{
    public string displayName = "Boss";
    public GameObject prefab;
    public float maxHealth = 1500f;
    public float moveSpeed = 2.5f;
    public float spawnRadius = 14f;
    public float spawnY = 2.5f;           // 프리팹 크기에 맞춘 높이

    [Header("접촉")]
    public float contactRange = 2.8f;
    public float contactDamage = 15f;
    public float contactInterval = 1f;

    [Header("패턴: 돌진")]
    public float patternInterval = 4f;    // 추적한 뒤 다음 돌진까지 간격
    public float chargeWarnTime = 1f;
    public float chargeSpeed = 18f;
    public float chargeDuration = 0.8f;
    public float chargeDamage = 30f;
    public float telegraphWidth = 3f;

    [Header("처치 보상")]
    public int expReward = 100;           // 총 경험치 (구슬 개수로 나눠 드롭)
    public int orbCount = 20;
}
