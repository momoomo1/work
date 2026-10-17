using UnityEngine;

public class ExpOrb : MonoBehaviour, IPoolable
{
    [SerializeField] float attractSpeed = 8f;
    [SerializeField] float attractAccel = 20f;
    [SerializeField] float collectDistance = 0.6f;

    int value;
    bool attracted;
    float speed;

    // 풀에서 꺼낸 직후 호출
    public void Init(int expValue) => value = expValue;

    public void OnSpawn()
    {
        attracted = false;
        speed = attractSpeed;
    }

    public void OnDespawn() { }

    void Update()
    {
        var exp = PlayerExperience.Current;
        if (exp == null) return;

        Vector3 to = exp.transform.position - transform.position;
        to.y = 0f;
        float sqr = to.sqrMagnitude;

        if (!attracted && sqr <= exp.PickupRange * exp.PickupRange) attracted = true;
        if (!attracted) return;

        if (sqr <= collectDistance * collectDistance)
        {
            exp.AddExp(value);
            ObjectPool.Release(gameObject);
            return;
        }

        speed += attractAccel * Time.deltaTime; // 가까워질수록 빨라져서 플레이어를 따라잡음
        transform.position += to.normalized * (speed * Time.deltaTime);
    }
}
