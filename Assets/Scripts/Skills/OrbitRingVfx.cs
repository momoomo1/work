using System.Collections;
using UnityEngine;

// 회전 칼날의 "고리" 이펙트 하나 (예: Cartoon FX Sword Trail 360 Spiral).
// 플레이어 중심에 놓이고, 이펙트의 한 바퀴 궤적이 칼날 회전 주기에 맞춰 계속 반복되도록 재생 속도를 맞춘다.
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
            main.loop = true; // 한 바퀴 궤적을 계속 반복
        }
        // 고리가 여러 개일 때 라이트가 쌓이지 않게 끔
        foreach (var l in GetComponentsInChildren<Light>(true)) l.enabled = false;
    }

    // period: 칼날이 한 바퀴 도는 시간(초), startDelay: 고리끼리 간격을 벌리기 위한 시작 지연
    public void Begin(float period, float startDelay, float scale)
    {
        transform.localScale = Vector3.one * scale;

        // 이펙트의 한 사이클(duration)이 period가 되도록 재생 속도 조절
        float natural = 0f;
        foreach (var ps in systems) if (ps != null) natural = Mathf.Max(natural, ps.main.duration);
        float speed = natural > 0.05f ? natural / Mathf.Max(0.2f, period) : 1f;

        foreach (var ps in systems)
        {
            if (ps == null) continue;
            var main = ps.main;
            main.simulationSpeed = speed;
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear); // playOnAwake로 이미 재생 중이면 멈춰서 지연 시작에 맞춤
        }

        StopAllCoroutines();
        StartCoroutine(PlayAfter(startDelay));
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
