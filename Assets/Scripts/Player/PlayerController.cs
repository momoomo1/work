using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] float moveSpeed = 6f;
    [SerializeField] float rotateSpeed = 12f;
    [SerializeField] float gravity = -20f;
    [SerializeField] Transform cameraTransform; // 비어있으면 Camera.main 사용

    CharacterController cc;
    float verticalVelocity;

    public Vector3 MoveDirection { get; private set; } // 마지막 이동 입력 방향 (수평, 입력 없으면 0)
    public bool InputLocked { get; set; }               // true면 이동 입력 무시 (대쉬 중)

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        if (cameraTransform == null && Camera.main != null) cameraTransform = Camera.main.transform;
    }

    void Update()
    {
        Vector2 input = InputLocked ? Vector2.zero : ReadInput();

        // 카메라 기준 방향 (수평면에 투영)
        Vector3 fwd = Vector3.forward, right = Vector3.right;
        if (cameraTransform != null)
        {
            fwd = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;
            right = Vector3.ProjectOnPlane(cameraTransform.right, Vector3.up).normalized;
        }
        Vector3 dir = fwd * input.y + right * input.x;
        if (dir.sqrMagnitude > 1f) dir.Normalize();
        if (!InputLocked) MoveDirection = dir;

        if (cc.isGrounded && verticalVelocity < 0f) verticalVelocity = -2f;
        verticalVelocity += gravity * Time.deltaTime;

        Vector3 move = dir * moveSpeed;
        move.y = verticalVelocity;
        cc.Move(move * Time.deltaTime);

        if (!InputLocked && dir.sqrMagnitude > 0.001f)
        {
            Quaternion target = Quaternion.LookRotation(dir, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, target, rotateSpeed * Time.deltaTime);
        }
    }

    static Vector2 ReadInput()
    {
        var kb = Keyboard.current;
        if (kb == null) return Vector2.zero;
        float x = (kb.dKey.isPressed ? 1f : 0f) - (kb.aKey.isPressed ? 1f : 0f);
        float y = (kb.wKey.isPressed ? 1f : 0f) - (kb.sKey.isPressed ? 1f : 0f);
        return new Vector2(x, y);
    }
}
