using System;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    [Serializable]
    public class PrewarmEntry
    {
        public GameObject prefab;
        public int count = 10;
    }

    [SerializeField] List<PrewarmEntry> prewarm = new List<PrewarmEntry>();

    static ObjectPool instance;
    // 씬에 없으면 자동 생성
    public static ObjectPool Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<ObjectPool>();
                if (instance == null) instance = new GameObject("ObjectPool").AddComponent<ObjectPool>();
            }
            return instance;
        }
    }

    readonly Dictionary<GameObject, Queue<GameObject>> pools = new Dictionary<GameObject, Queue<GameObject>>();
    readonly Dictionary<GameObject, Transform> containers = new Dictionary<GameObject, Transform>();
    readonly Dictionary<GameObject, GameObject> instanceToPrefab = new Dictionary<GameObject, GameObject>();
    readonly Dictionary<GameObject, IPoolable[]> poolables = new Dictionary<GameObject, IPoolable[]>();

    void Awake()
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;
        foreach (var e in prewarm)
            if (e.prefab != null) Prewarm(e.prefab, e.count);
    }

    void OnDestroy()
    {
        if (instance == this) instance = null;
    }

    public static GameObject Get(GameObject prefab, Vector3 pos, Quaternion rot)
        => Instance.GetInternal(prefab, pos, rot);

    public static void Release(GameObject obj)
    {
        if (obj == null) return;
        if (instance == null) { Destroy(obj); return; } // 종료 중이거나 풀 없음
        instance.ReleaseInternal(obj);
    }

    // 미리 생성해서 풀에 채워 둔다
    public void Prewarm(GameObject prefab, int count)
    {
        var queue = GetQueue(prefab);
        for (int i = 0; i < count; i++)
        {
            var obj = CreateInstance(prefab);
            obj.SetActive(false);
            queue.Enqueue(obj);
        }
    }

    GameObject GetInternal(GameObject prefab, Vector3 pos, Quaternion rot)
    {
        var queue = GetQueue(prefab);
        GameObject obj = null;
        while (queue.Count > 0 && obj == null) obj = queue.Dequeue(); // 외부에서 파괴된 것은 건너뜀
        if (obj == null) obj = CreateInstance(prefab);

        obj.transform.SetParent(null);
        obj.transform.SetPositionAndRotation(pos, rot);
        obj.SetActive(true);

        var list = GetPoolables(obj);
        for (int i = 0; i < list.Length; i++) list[i].OnSpawn();
        return obj;
    }

    void ReleaseInternal(GameObject obj)
    {
        if (!instanceToPrefab.TryGetValue(obj, out var prefab))
        {
            Destroy(obj); // 풀에서 만든 것이 아님
            return;
        }
        if (!obj.activeSelf) return; // 이미 반환됨

        var list = GetPoolables(obj);
        for (int i = 0; i < list.Length; i++) list[i].OnDespawn();

        obj.SetActive(false);
        obj.transform.SetParent(containers[prefab], false);
        pools[prefab].Enqueue(obj);
    }

    IPoolable[] GetPoolables(GameObject obj)
    {
        if (!poolables.TryGetValue(obj, out var list))
        {
            list = obj.GetComponentsInChildren<IPoolable>(true);
            poolables[obj] = list;
        }
        return list;
    }

    Queue<GameObject> GetQueue(GameObject prefab)
    {
        if (!pools.TryGetValue(prefab, out var queue))
        {
            queue = new Queue<GameObject>();
            pools[prefab] = queue;
            var container = new GameObject($"Pool_{prefab.name}").transform;
            container.SetParent(transform, false);
            containers[prefab] = container;
        }
        return queue;
    }

    GameObject CreateInstance(GameObject prefab)
    {
        var obj = Instantiate(prefab, containers[prefab]);
        obj.name = prefab.name;
        instanceToPrefab[obj] = prefab;
        return obj;
    }
}
