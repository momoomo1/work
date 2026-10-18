using UnityEditor;
using UnityEngine;

// 던전 바닥/벽 프리팹을 지정하면 정사각형 아레나를 타일로 채우고, 보이지 않는 경계 콜라이더를 만든다.
public class SetupMapWindow : EditorWindow
{
    [SerializeField] GameObject floorPrefab;
    [SerializeField] GameObject wallPrefab;
    [SerializeField] float arenaSize = 60f;     // 한 변 길이(유닛)
    [SerializeField] float tileSizeOverride = 0f; // 0이면 바닥 프리팹의 크기에서 자동 계산
    [SerializeField] float wallOffset = 0f;     // 벽을 아레나 경계선에서 바깥으로 밀어낼 거리
    [SerializeField] float floorY = 0f;
    [SerializeField] bool hideGroundPlane = true;

    [MenuItem("Tools/Setup/Build Dungeon Map")]
    static void Open() => GetWindow<SetupMapWindow>("Dungeon Map");

    void OnGUI()
    {
        EditorGUILayout.HelpBox("바닥(필수)과 벽(선택) 프리팹을 넣고 Build를 누르세요. 경계 콜라이더는 벽 프리팹이 없어도 항상 만들어집니다.", MessageType.Info);
        floorPrefab = (GameObject)EditorGUILayout.ObjectField("바닥 타일 프리팹", floorPrefab, typeof(GameObject), false);
        wallPrefab = (GameObject)EditorGUILayout.ObjectField("벽 프리팹", wallPrefab, typeof(GameObject), false);
        arenaSize = EditorGUILayout.FloatField("아레나 한 변 길이", arenaSize);
        tileSizeOverride = EditorGUILayout.FloatField("타일 크기 (0=자동)", tileSizeOverride);
        wallOffset = EditorGUILayout.FloatField("벽 바깥 오프셋", wallOffset);
        floorY = EditorGUILayout.FloatField("바닥 높이(Y)", floorY);
        hideGroundPlane = EditorGUILayout.Toggle("기존 Ground 렌더러 숨김", hideGroundPlane);

        if (GUILayout.Button("Build")) Build();
    }

    void Build()
    {
        var old = GameObject.Find("DungeonMap");
        if (old != null) Undo.DestroyObjectImmediate(old);
        var oldWalls = GameObject.Find("BoundaryWalls");
        if (oldWalls != null) Undo.DestroyObjectImmediate(oldWalls);

        float half = arenaSize * 0.5f;
        var root = new GameObject("DungeonMap");
        Undo.RegisterCreatedObjectUndo(root, "Build Dungeon Map");

        if (floorPrefab != null)
        {
            float tile = tileSizeOverride > 0f ? tileSizeOverride : MeasureTile(floorPrefab);
            int n = Mathf.Max(1, Mathf.CeilToInt(arenaSize / tile));
            float start = -n * tile * 0.5f + tile * 0.5f;

            var floors = new GameObject("Floors").transform;
            floors.SetParent(root.transform, false);
            for (int x = 0; x < n; x++)
                for (int z = 0; z < n; z++)
                    Place(floorPrefab, floors, new Vector3(start + x * tile, floorY, start + z * tile), Quaternion.identity);

            if (wallPrefab != null) BuildWalls(root.transform, n * tile * 0.5f + wallOffset, tile);
        }
        else Debug.LogWarning("바닥 프리팹이 없어 바닥/벽 타일은 만들지 않았습니다.");

        BuildBoundary(half);

        if (hideGroundPlane && floorPrefab != null)
        {
            var ground = GameObject.Find("Ground");
            if (ground != null && ground.TryGetComponent<MeshRenderer>(out var mr)) mr.enabled = false; // 충돌용으로 콜라이더는 유지
        }
        Debug.Log("던전 맵 생성 완료. 씬을 저장(Ctrl+S)하세요.");
    }

    // 벽은 4면에 바깥쪽 → 안쪽을 바라보게 배치 (프리팹의 앞면이 +Z라고 가정)
    void BuildWalls(Transform parent, float edge, float tile)
    {
        var walls = new GameObject("Walls").transform;
        walls.SetParent(parent, false);

        float wallTile = MeasureTile(wallPrefab);
        int count = Mathf.CeilToInt(edge * 2f / wallTile);
        float start = -count * wallTile * 0.5f + wallTile * 0.5f;

        for (int i = 0; i < count; i++)
        {
            float p = start + i * wallTile;
            Place(wallPrefab, walls, new Vector3(p, floorY, edge), Quaternion.Euler(0f, 180f, 0f));   // 북쪽 벽: 남쪽(안쪽)을 봄
            Place(wallPrefab, walls, new Vector3(p, floorY, -edge), Quaternion.Euler(0f, 0f, 0f));    // 남쪽 벽
            Place(wallPrefab, walls, new Vector3(edge, floorY, p), Quaternion.Euler(0f, -90f, 0f));   // 동쪽 벽
            Place(wallPrefab, walls, new Vector3(-edge, floorY, p), Quaternion.Euler(0f, 90f, 0f));   // 서쪽 벽
        }
    }

    // 플레이 영역 밖으로 못 나가게 하는 보이지 않는 벽
    static void BuildBoundary(float half)
    {
        var root = new GameObject("BoundaryWalls");
        Undo.RegisterCreatedObjectUndo(root, "Create BoundaryWalls");
        const float thick = 4f, height = 10f;
        float len = half * 2f + thick * 2f;

        MakeBox(root.transform, "North", new Vector3(0f, height * 0.5f, half + thick * 0.5f), new Vector3(len, height, thick));
        MakeBox(root.transform, "South", new Vector3(0f, height * 0.5f, -half - thick * 0.5f), new Vector3(len, height, thick));
        MakeBox(root.transform, "East", new Vector3(half + thick * 0.5f, height * 0.5f, 0f), new Vector3(thick, height, len));
        MakeBox(root.transform, "West", new Vector3(-half - thick * 0.5f, height * 0.5f, 0f), new Vector3(thick, height, len));
    }

    static void MakeBox(Transform parent, string name, Vector3 pos, Vector3 size)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = pos;
        go.AddComponent<BoxCollider>().size = size;
    }

    static void Place(GameObject prefab, Transform parent, Vector3 pos, Quaternion rot)
    {
        var obj = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
        obj.transform.SetPositionAndRotation(pos, rot);
    }

    // 프리팹 렌더러 전체 바운즈에서 가로 길이(x/z 중 큰 값)를 타일 크기로 사용
    static float MeasureTile(GameObject prefab)
    {
        var rs = prefab.GetComponentsInChildren<Renderer>();
        if (rs.Length == 0) return 4f;
        var b = rs[0].bounds;
        foreach (var r in rs) b.Encapsulate(r.bounds);
        float size = Mathf.Max(b.size.x, b.size.z);
        return size > 0.1f ? size : 4f;
    }
}
