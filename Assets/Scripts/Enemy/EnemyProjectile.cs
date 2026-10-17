using UnityEngine;

// 적(보스)이 쏘는 투사체. 플레이어와 거리 판정으로 적중.
public class EnemyProjectile : MonoBehaviour, IPoolable
{
    [SerializeField] float hitRadius = 0.8f; // 투사체 + 플레이어 몸 반경 합

    Vector3 direction = Vector3.forward;
    float speed;
    float damage;
    float timeLeft;

    public void Launch(Vector3 dir, float speed, float damage, float lifetime)
    {
        direction = dir.normalized;
        this.speed = speed;
        this.damage = damage;
        timeLeft = lifetime;
    }

    public void OnSpawn() { }
    public void OnDespawn() { }

    void Update()
    {
        transform.position += direction * (speed * Time.deltaTime);

        timeLeft -= Time.deltaTime;
        if (timeLeft <= 0f) { ObjectPool.Release(gameObject); return; }

        var player = PlayerLocator.Player;
        if (player == null) return;

        Vector3 d = player.position - transform.position;
        d.y = 0f;
        if (d.sqrMagnitude <= hitRadius * hitRadius)
        {
            PlayerLocator.Damageable?.TakeDamage(damage);
            ObjectPool.Release(gameObject);
        }
    }
}
