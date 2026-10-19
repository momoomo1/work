using UnityEngine;
using UnityEngine.SceneManagement;

// 패시브 스킬이 올려주는 전역 배율. 씬이 다시 로드되면 초기화된다.
public static class PlayerStats
{
    public static float AttackSpeed = 1f; // 클수록 스킬 쿨타임이 짧아짐
    public static float MoveSpeed = 1f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Init()
    {
        Reset();
        SceneManager.sceneLoaded += (_, __) => Reset();
    }

    static void Reset()
    {
        AttackSpeed = 1f;
        MoveSpeed = 1f;
    }
}