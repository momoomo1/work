public interface IPoolable
{
    void OnSpawn();   // 풀에서 꺼내 활성화된 직후
    void OnDespawn(); // 풀로 반환되어 비활성화되기 직전
}
