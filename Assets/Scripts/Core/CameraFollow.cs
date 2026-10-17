using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] float pitch = 50f;      // 내려다보는 각도
    [SerializeField] float yaw = 0f;         // 수평 회전
    [SerializeField] float distance = 12f;   // 타겟까지 거리
    [SerializeField] float heightOffset = 1f; // 타겟 기준 시선 높이
    [SerializeField] float followSmooth = 10f;

    void LateUpdate()
    {
        if (target == null) return;
        Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 focus = target.position + Vector3.up * heightOffset;
        Vector3 desired = focus - rot * Vector3.forward * distance;
        transform.position = Vector3.Lerp(transform.position, desired, 1f - Mathf.Exp(-followSmooth * Time.deltaTime));
        transform.rotation = rot;
    }
}
