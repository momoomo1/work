using UnityEngine;

// "Player" 태그 오브젝트를 한 번만 찾아 캐시
public static class PlayerLocator
{
    static Transform player;
    static IDamageable damageable;

    public static Transform Player
    {
        get
        {
            if (player == null)
            {
                var go = GameObject.FindGameObjectWithTag("Player");
                if (go != null)
                {
                    player = go.transform;
                    damageable = go.GetComponent<IDamageable>();
                }
            }
            return player;
        }
    }

    public static IDamageable Damageable => Player != null ? damageable : null;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() { player = null; damageable = null; }
}
