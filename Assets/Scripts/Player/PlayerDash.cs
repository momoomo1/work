using UnityEngine;
using UnityEngine.InputSystem;

// 대쉬: 마우스 우클릭 또는 스페이스바. 이동 입력 방향(없으면 우클릭은 커서 방향, 스페이스는 바라보는 방향)으로 짧게 돌진.
// 충전식: 최대 maxCharges번 연속 사용할 수 있고, 빈 칸은 rechargeTime마다 한 칸씩 채워진다.
// 대쉬 중(+약간의 여유)에는 피해를 받지 않는다. 쿨타임 UI(DashUI)는 플레이어 발밑에 자동으로 생긴다.
[RequireComponent(typeof(CharacterController))]
public class PlayerDash : MonoBehaviour
{
    [SerializeField] int maxCharges = 2;
    [SerializeField] float rechargeTime = 3f;        // 한 칸이 채워지는 시간(초)
    [SerializeField] float dashDistance = 5.5f;
    [SerializeField] float dashDuration = 0.16f;
    [SerializeField] float invulnerableTime = 0.25f; // 대쉬 직후 여유까지 포함한 무적 시간
    [SerializeField] float minInterval = 0.15f;      // 연속 사용 사이 최소 간격

    CharacterController cc;
    PlayerController pc;
    Health health;

    int charges;
    float recharge;
    bool dashing;
    float dashTimer;
    float invulnTimer;
    float lastDash = -10f;
    Vector3 dashDir;

    public int MaxCharges => maxCharges;
    public int Charges => charges;
    // 현재 채워지고 있는 칸의 진행도 0~1 (모두 찼으면 1)
    public float RechargeProgress => charges >= maxCharges ? 1f : Mathf.Clamp01(recharge / rechargeTime);
    public float RechargeRemaining => charges >= maxCharges ? 0f : Mathf.Max(0f, rechargeTime - recharge);

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        pc = GetComponent<PlayerController>();
        health = GetComponent<Health>();
        charges = maxCharges;
    }

    // 대쉬 UI는 시작 화면이 닫힌 뒤에 만듦
    System.Collections.IEnumerator Start()
    {
        while (StartMenuUI.IsOpen) yield return null;
        DashUI.Create(this);
    }

    // 레벨업: 충전 시간 감소 + 칸 추가 (추가된 칸은 바로 사용 가능)
    public void Upgrade(float rechargeReduce, int extraCharges)
    {
        rechargeTime = Mathf.Max(0.5f, rechargeTime - rechargeReduce);
        maxCharges += extraCharges;
        charges += extraCharges;
    }

    void OnDisable() => EndDash();

    void Update()
    {
        float dt = Time.deltaTime;
        if (dt <= 0f) return; // 일시정지(레벨업 선택 중 등)에는 동작하지 않음
        if (GameManager.Instance.IsGameOver) return;

        // 충전
        if (charges < maxCharges)
        {
            recharge += dt;
            if (recharge >= rechargeTime) { charges++; recharge = 0f; }
        }

        // 무적 종료
        if (invulnTimer > 0f)
        {
            invulnTimer -= dt;
            if (invulnTimer <= 0f && health != null) health.Invulnerable = false;
        }

        if (dashing)
        {
            cc.Move(dashDir * (dashDistance / dashDuration) * dt);
            dashTimer -= dt;
            if (dashTimer <= 0f) EndDash();
            return;
        }

        bool rightClick = Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame;
        bool space = Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;
        if ((rightClick || space) && charges > 0 && Time.time - lastDash >= minInterval) StartDash(rightClick);
    }

    void StartDash(bool useCursor)
    {
        // 방향: 이동 입력 > (우클릭이면) 커서 방향 > 바라보는 방향
        Vector3 dir = pc != null ? pc.MoveDirection : Vector3.zero;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.01f)
        {
            if (!(useCursor && PlayerAim.TryGetCursorDirection(transform.position, out dir))) dir = transform.forward;
            dir.y = 0f;
        }
        dashDir = dir.sqrMagnitude > 0.0001f ? dir.normalized : Vector3.forward;

        charges--;
        dashing = true;
        dashTimer = dashDuration;
        lastDash = Time.time;
        transform.rotation = Quaternion.LookRotation(dashDir, Vector3.up);

        if (pc != null) pc.InputLocked = true;
        if (health != null) health.Invulnerable = true;
        invulnTimer = invulnerableTime;
    }

    void EndDash()
    {
        dashing = false;
        if (pc != null) pc.InputLocked = false;
    }
}
