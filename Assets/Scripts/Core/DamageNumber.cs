using TMPro;
using UnityEngine;

// 떠오르며 사라지는 데미지 숫자 (월드 공간 TextMeshPro, 풀 사용).
// 프리팹 에셋 없이 런타임에 템플릿을 만들어 풀의 원본으로 쓴다.
public class DamageNumber : MonoBehaviour, IPoolable
{
    const int MaxAlive = 45;           // 동시에 떠 있는 숫자 상한 (성능)
    const float Life = 0.75f;
    const float RiseSpeed = 1.8f;
    const float PopTime = 0.12f;

    static GameObject template;
    static int alive;
    static Camera cam;

    TextMeshPro text;
    Color baseColor;
    float baseScale;
    float age;
    Vector3 drift;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() { template = null; alive = 0; cam = null; }

    public static void Spawn(Vector3 position, float amount, Color color, float scale = 1f)
    {
        if (alive >= MaxAlive) return;

        if (template == null) template = CreateTemplate();

        // 겹치지 않게 약간 흩뿌림
        position += new Vector3(Random.Range(-0.4f, 0.4f), 0f, Random.Range(-0.2f, 0.2f));
        var obj = ObjectPool.Get(template, position, Quaternion.identity);
        obj.GetComponent<DamageNumber>().Show(NumberString(Mathf.Max(1, Mathf.RoundToInt(amount))), color, scale);
    }

    // 자주 나오는 작은 숫자는 문자열을 한 번만 만들어 재사용 (피격마다 할당 방지)
    static readonly string[] numberCache = new string[1000];

    static string NumberString(int v)
    {
        if (v >= numberCache.Length) return v.ToString();
        return numberCache[v] ?? (numberCache[v] = v.ToString());
    }

    static GameObject CreateTemplate()
    {
        var go = new GameObject("DamageNumberTemplate");
        go.hideFlags = HideFlags.HideInHierarchy;
        go.SetActive(false); // 풀의 원본은 비활성 상태로 둠

        var tmp = go.AddComponent<TextMeshPro>();
        tmp.fontSize = 5f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;
        tmp.raycastTarget = false;
        tmp.sortingOrder = 50;

        go.AddComponent<DamageNumber>();
        return go;
    }

    void Awake() => text = GetComponent<TextMeshPro>();

    void Show(string value, Color color, float scale)
    {
        text.text = value;
        baseColor = color;
        baseScale = scale;
        text.color = color;
        age = 0f;
        drift = new Vector3(Random.Range(-0.4f, 0.4f), 0f, 0f);
        transform.localScale = Vector3.one * (scale * 1.5f);
        Face();
    }

    public void OnSpawn() { alive++; }
    public void OnDespawn() { alive = Mathf.Max(0, alive - 1); }

    void Update()
    {
        age += Time.deltaTime;
        if (age >= Life) { ObjectPool.Release(gameObject); return; }

        transform.position += (Vector3.up * RiseSpeed + drift) * Time.deltaTime;

        // 처음에 크게 튀었다가 원래 크기로
        float pop = Mathf.Clamp01(age / PopTime);
        transform.localScale = Vector3.one * (baseScale * Mathf.Lerp(1.5f, 1f, pop));

        // 후반부에 서서히 투명하게
        float fade = Mathf.Clamp01((Life - age) / (Life * 0.4f));
        var c = baseColor;
        c.a = fade;
        text.color = c;

        Face();
    }

    void Face()
    {
        if (cam == null) cam = Camera.main;
        if (cam != null) transform.rotation = cam.transform.rotation; // 항상 카메라를 향함
    }
}
