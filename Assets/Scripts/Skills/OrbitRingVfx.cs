using System.Collections;
using UnityEngine;

// 회전 칼날의 고리 하나에 붙는 이펙트 제어 (예: Cartoon FX Sword Trail 360 Spiral).
// 풀에서 꺼낼 때마다 처음부터 다시 재생하고, 한 번 훑는 궤적이 끝나면 계속 반복되도록 루프시킨다.
public class OrbitRingVfx : MonoBehaviour, IPoolable
{
    ParticleSystem[] systems;
    Component[] graphEffects;

    void Awake()
    {
        PooledVfx.StripAutoClear(gameObject); // 에셋의 자동 Destroy/카메라 흔들림 스크립트 제거
        systems = GetComponentsInChildren<ParticleSystem>(true);
        graphEffects = VisualEffectUtil.GetAll(gameObject);

        foreach (var ps in systems)
        {
            var main = ps.main;
            main.scalingMode = ParticleSystemScalingMode.Hierarchy;
            main.stopAction = ParticleSystemStopAction.None;
            main.loop = true;
        }
        // 고리가 여러 개일 때 라이트가 쌓이지 않게 끔
        foreach (var l in GetComponentsInChildren<Light>(true)) l.enabled = false;
    }

    // scale: 이펙트 크기, startDelay: 고리끼리 모양이 달라 보이도록 시작을 어긋나게 함
    public void Begin(float scale, float startDelay, float brightness = 1f)
    {
        transform.localScale = Vector3.one * scale;
        ApplyBrightness(brightness);
        foreach (var ps in systems)
            if (ps != null) ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear); // playOnAwake로 이미 재생 중이면 멈춤

        StopAllCoroutines();
        StartCoroutine(PlayAfter(startDelay));
    }

    // 파티클의 시작 색을 밝게 (원본 색은 처음 한 번만 저장해서 풀 재사용 때 계속 곱해지지 않게 함)
    Color[] baseColors;
    void ApplyBrightness(float brightness)
    {
        if (baseColors == null)
        {
            baseColors = new Color[systems.Length];
            for (int i = 0; i < systems.Length; i++)
                baseColors[i] = systems[i].main.startColor.mode == ParticleSystemGradientMode.Color ? systems[i].main.startColor.color : Color.white;
        }
        for (int i = 0; i < systems.Length; i++)
        {
            if (systems[i] == null || systems[i].main.startColor.mode != ParticleSystemGradientMode.Color) continue;
            var main = systems[i].main;
            Color c = baseColors[i] * brightness;
            c.a = baseColors[i].a;
            main.startColor = c;
        }
    }

    IEnumerator PlayAfter(float delay)
    {
        if (delay > 0f) yield return new WaitForSeconds(delay);
        foreach (var ps in systems) if (ps != null) ps.Play(true);
        VisualEffectUtil.Restart(graphEffects);
    }

    public void OnSpawn() { }
    public void OnDespawn() { StopAllCoroutines(); }
}
