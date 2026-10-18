using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] float pitch = 50f;      // 내려다보는 각도
    [SerializeField] float yaw = 0f;         // 수평 회전
    [SerializeField] float distance = 12f;   // 타겟까지 거리
    [SerializeField] float heightOffset = 1f; // 타겟 기준 시선 높이
    [SerializeField] float followSmooth = 10f;

    static CameraFollow instance;
    float shakeMagnitude;
    float shakeDuration;
    float shakeTimeLeft;

    void OnEnable() => instance = this;
    void OnDisable() { if (instance == this) instance = null; }

    // 화면 흔들림: magnitude(세기, 유닛), duration(초). 더 센 흔들림이 우선.
    public static void Shake(float magnitude, float duration)
    {
        if (instance == null || magnitude <= 0f) return;
        float current = instance.shakeTimeLeft > 0f ? instance.shakeMagnitude * (instance.shakeTimeLeft / instance.shakeDuration) : 0f;
        if (magnitude < current) return;
        instance.shakeMagnitude = magnitude;
        instance.shakeDuration = Mathf.Max(0.01f, duration);
        instance.shakeTimeLeft = duration;
    }

    void LateUpdate()
    {
        if (target == null) return;
        Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 focus = target.position + Vector3.up * heightOffset;
        Vector3 desired = focus - rot * Vector3.forward * distance;
        transform.position = Vector3.Lerp(transform.position, desired, 1f - Mathf.Exp(-followSmooth * Time.deltaTime));
        transform.rotation = rot;

        if (shakeTimeLeft > 0f)
        {
            shakeTimeLeft -= Time.deltaTime;
            float k = Mathf.Clamp01(shakeTimeLeft / shakeDuration);
            transform.position += Random.insideUnitSphere * (shakeMagnitude * k); // 시간이 갈수록 약해짐
        }
    }
}
