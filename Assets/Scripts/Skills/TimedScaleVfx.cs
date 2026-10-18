using UnityEngine;

// 지정 시간 동안 표시되다가 풀로 반환되는 비주얼 (폭발 구체, 경고 원 공용).
// 파티클로 교체할 때는 이 컴포넌트를 붙인 프리팹에 파티클을 자식으로 넣으면 됨.
public class TimedScaleVfx : MonoBehaviour, IPoolable, IVfx
{
    [SerializeField] bool grow = true;   // true: 0 → 지름까지 커짐, false: 지름 고정
    [SerializeField] bool flat = false;  // true: Y 스케일은 프리팹 값 유지 (납작한 원판)

    float baseY;
    float diameter;
    float duration;
    float elapsed;
    bool playing;

    void Awake() => baseY = transform.localScale.y;

    public void Play(float diameter, float duration)
    {
        this.diameter = diameter;
        this.duration = Mathf.Max(0.01f, duration);
        elapsed = 0f;
        playing = true;
        Apply(grow ? 0f : 1f);
    }

    public void OnSpawn() { playing = false; }
    public void OnDespawn() { playing = false; }

    void Update()
    {
        if (!playing) return;
        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / duration);
        if (grow) Apply(t);
        if (t >= 1f) ObjectPool.Release(gameObject);
    }

    void Apply(float t)
    {
        float d = diameter * t;
        transform.localScale = new Vector3(d, flat ? baseY : d, d);
    }
}
