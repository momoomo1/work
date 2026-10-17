using UnityEngine;

// 비주얼 전용: Health가 줄어들면 Renderer를 잠깐 하얗게 만든다. 모델 교체 시 Renderer 참조만 바꾸면 됨.
public class HitFlash : MonoBehaviour
{
    [SerializeField] Health health;
    [SerializeField] Renderer targetRenderer;
    [SerializeField] Color flashColor = Color.white;
    [SerializeField] float duration = 0.1f;

    static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    static readonly int ColorId = Shader.PropertyToID("_Color");

    MaterialPropertyBlock block;
    float lastHealth;
    float timer;

    void Awake()
    {
        if (health == null) health = GetComponent<Health>();
        if (targetRenderer == null) targetRenderer = GetComponentInChildren<Renderer>();
        block = new MaterialPropertyBlock();
    }

    void OnEnable()
    {
        health.OnChanged += HandleChanged;
        lastHealth = health.CurrentHealth;
    }

    void OnDisable()
    {
        health.OnChanged -= HandleChanged;
        Clear();
    }

    void HandleChanged(float current, float max)
    {
        if (current < lastHealth && current > 0f) Flash();
        lastHealth = current;
    }

    void Flash()
    {
        timer = duration;
        block.SetColor(BaseColorId, flashColor);
        block.SetColor(ColorId, flashColor);
        targetRenderer.SetPropertyBlock(block);
        enabled = true;
    }

    void Update()
    {
        if (timer <= 0f) return;
        timer -= Time.deltaTime;
        if (timer <= 0f) Clear();
    }

    void Clear()
    {
        timer = 0f;
        if (targetRenderer != null) targetRenderer.SetPropertyBlock(null);
    }
}
