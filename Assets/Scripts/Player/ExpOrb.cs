using UnityEngine;

public class ExpOrb : MonoBehaviour, IPoolable
{
    [SerializeField] float attractSpeed = 8f;
    [SerializeField] float attractAccel = 20f;
    [SerializeField] float collectDistance = 0.6f;

    public static int ActiveCount { get; private set; }
    static readonly System.Collections.Generic.List<ExpOrb> all = new System.Collections.Generic.List<ExpOrb>();

    // 필드에 떨어진 구슬을 전부 즉시 플레이어 쪽으로 끌어당김 (보스 처치 보상 등)
    public static void AttractAll(float speedMultiplier = 2.5f)
    {
        foreach (var orb in all)
        {
            orb.attracted = true;
            orb.speed = Mathf.Max(orb.speed, orb.attractSpeed * speedMultiplier);
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() { ActiveCount = 0; all.Clear(); }

    int value;
    bool attracted;
    float speed;

    // 풀에서 꺼낸 직후 호출
    public void Init(int expValue) => value = expValue;

    public void OnSpawn()
    {
        attracted = false;
        speed = attractSpeed;
        ActiveCount++;
        all.Add(this);
    }

    public void OnDespawn()
    {
        ActiveCount = Mathf.Max(0, ActiveCount - 1);
        all.Remove(this);
    }

    void OnDestroy() => all.Remove(this);

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
