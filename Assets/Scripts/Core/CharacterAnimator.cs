using UnityEngine;

// 이동 속도에 따라 Animator 파라미터를 갱신 (로직 스크립트와 분리).
// 컨트롤러 파라미터: Speed(float), RunMul(float), Attack(trigger), Death(trigger)
public class CharacterAnimator : MonoBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField] float baseRunSpeed = 3f;     // 이 속도에서 Run 재생 속도 1배
    [SerializeField] float attackRange = 0f;      // >0이면 플레이어가 이 거리 안일 때 Attack 트리거 (적/보스용)
    [SerializeField] float attackCooldown = 1f;
    [SerializeField] bool playDeath = false;      // 풀로 즉시 반환되는 적은 끔 (플레이어만 사용)

    static readonly int SpeedHash = Animator.StringToHash("Speed");
    static readonly int RunMulHash = Animator.StringToHash("RunMul");
    static readonly int AttackHash = Animator.StringToHash("Attack");
    static readonly int DeathHash = Animator.StringToHash("Death");

    Health health;
    Vector3 lastPos;
    float smoothed;
    float nextAttack;

    void Awake()
    {
        if (animator == null) animator = GetComponentInChildren<Animator>();
        health = GetComponent<Health>();
    }

    void OnEnable()
    {
        lastPos = transform.position;
        smoothed = 0f;
        if (animator != null)
        {
            animator.Rebind(); // 풀 재사용 시 상태 초기화
            animator.Update(0f);
        }
        if (health != null && playDeath) health.OnDeath += HandleDeath;
    }

    void OnDisable()
    {
        if (health != null && playDeath) health.OnDeath -= HandleDeath;
    }

    public void PlayAttack()
    {
        if (animator != null) animator.SetTrigger(AttackHash);
    }

    void HandleDeath()
    {
        if (animator == null) return;
        animator.updateMode = AnimatorUpdateMode.UnscaledTime; // 게임오버로 timeScale=0이어도 재생
        animator.SetTrigger(DeathHash);
    }

    void Update()
    {
        if (animator == null) return;
        float dt = Time.deltaTime;
        if (dt <= 0f) return;

        Vector3 d = transform.position - lastPos;
        d.y = 0f;
        lastPos = transform.position;
        float speed = d.magnitude / dt;
        smoothed = Mathf.Lerp(smoothed, speed, 1f - Mathf.Exp(-12f * dt));

        animator.SetFloat(SpeedHash, smoothed);
        animator.SetFloat(RunMulHash, Mathf.Clamp(smoothed / Mathf.Max(0.01f, baseRunSpeed), 0.6f, 2f));

        if (attackRange > 0f && Time.time >= nextAttack)
        {
            var player = PlayerLocator.Player;
            if (player != null)
            {
                Vector3 to = player.position - transform.position;
                to.y = 0f;
                if (to.sqrMagnitude <= attackRange * attackRange)
                {
                    nextAttack = Time.time + attackCooldown;
                    animator.SetTrigger(AttackHash);
                }
            }
        }
    }
}
