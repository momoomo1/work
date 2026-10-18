using UnityEngine;

[CreateAssetMenu(menuName = "Game/Enemy Data", fileName = "EnemyData")]
public class EnemyData : ScriptableObject
{
    public string displayName = "Enemy";
    public float maxHealth = 30f;
    public float moveSpeed = 3f;
    public float contactDamage = 5f;
    public float attackInterval = 1f;
    public int expReward = 1;
    public float scale = 1f;            // 몸집 배율 (작을수록 가볍고 화면에 많이 보임)
    public GameObject prefab;
}
