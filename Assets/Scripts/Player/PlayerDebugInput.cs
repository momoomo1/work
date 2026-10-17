using UnityEngine;
#if UNITY_EDITOR
using UnityEngine.InputSystem;
#endif

// 에디터 전용 디버그: T키 = 자신에게 10 데미지
public class PlayerDebugInput : MonoBehaviour
{
#if UNITY_EDITOR
    [SerializeField] float debugDamage = 10f;
    Health health;

    void Awake() => health = GetComponent<Health>();

    void Update()
    {
        var kb = Keyboard.current;
        if (kb != null && kb.tKey.wasPressedThisFrame) health.TakeDamage(debugDamage);
    }
#endif
}
