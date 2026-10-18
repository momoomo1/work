using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

// 마우스 조준: 왼쪽 버튼을 누르고 있는 동안만 조준 방향을 제공한다.
// 누르지 않으면 false를 반환하므로 호출하는 쪽에서 자동 타겟팅으로 대체하면 된다.
public static class PlayerAim
{
    static Camera cam;

    // origin(보통 플레이어 위치)에서 마우스 커서가 가리키는 지면 지점까지의 수평 방향
    public static bool TryGetAimDirection(Vector3 origin, out Vector3 direction)
    {
        direction = Vector3.zero;

        var mouse = Mouse.current;
        if (mouse == null || !mouse.leftButton.isPressed) return false;
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return false; // UI 위 클릭 무시

        if (cam == null) cam = Camera.main;
        if (cam == null) return false;

        Ray ray = cam.ScreenPointToRay(mouse.position.ReadValue());
        var plane = new Plane(Vector3.up, new Vector3(0f, origin.y, 0f)); // 플레이어 높이의 수평면
        if (!plane.Raycast(ray, out float dist)) return false;

        Vector3 d = ray.GetPoint(dist) - origin;
        d.y = 0f;
        if (d.sqrMagnitude < 0.01f) return false; // 플레이어 바로 위를 클릭

        direction = d.normalized;
        return true;
    }
}
