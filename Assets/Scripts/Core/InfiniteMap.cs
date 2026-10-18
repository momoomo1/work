using System.Collections.Generic;
using UnityEngine;

// 뱀서 스타일 무한 맵: 플레이어 주변 (2r+1)×(2r+1) 바닥 타일만 만들어 두고,
// 플레이어가 한 칸 이동할 때마다 뒤쪽 타일을 앞쪽으로 옮겨 재사용한다. 소품(나무, 바위 등)은 칸마다 같은 시드로 배치된다.
// 이 컴포넌트가 붙은 오브젝트(충돌용 바닥 Plane)도 플레이어를 따라 움직인다.
public class InfiniteMap : MonoBehaviour
{
    [SerializeField] GameObject[] groundTiles;   // 바닥 타일 프리팹 (칸마다 이 중에서 섞어 사용)
    [SerializeField] GameObject[] props;         // 흩뿌릴 소품 프리팹 (나무, 바위, 덤불 등)
    [SerializeField] int radius = 0;             // 플레이어 주변 타일 반경. 0이면 화면을 덮도록 자동 계산
    [SerializeField] float minCoverage = 90f;    // 자동 계산 시 최소로 덮을 한 변 길이(유닛)
    [SerializeField] float tileSizeOverride = 0f; // 0이면 타일 프리팹 크기에서 자동 측정
    [SerializeField] float groundY = 0f;
    [SerializeField] float tileYOffset = 0.03f;  // 바닥 Plane(충돌용, 초록색 기본 바닥) 위로 살짝 띄움 → 타일 사이 빈틈은 이 초록 바닥이 채움
    [SerializeField, Range(0f, 1f)] float tileFlatten = 0.03f; // 풀 패치의 볼록한 정도 (0=완전히 평평). 플레이어 발이 둔덕에 파묻히지 않게 눌러 둠
    [SerializeField] int propsPerTile = 6;       // 타일 한 칸당 최대 소품 수
    [SerializeField, Range(0f, 1f)] float propChance = 0.55f;
    [SerializeField] float propClearRadius = 6f; // 시작 지점 주변은 소품을 비움
    [SerializeField] Vector2 propScaleRange = new Vector2(0.9f, 1.3f);

    class Tile
    {
        public Transform root;        // 바닥 메시
        public Transform anchor;      // 칸 중심 (소품의 부모)
        public Vector2 centerOffset;  // 피벗 → 메시 중심 오프셋
        public Transform[] props;
        public Vector2Int cell = new Vector2Int(int.MinValue, int.MinValue);
    }

    readonly List<Tile> tiles = new List<Tile>();
    Transform container;
    Transform player;
    float tileSize;
    int size;      // 한 변의 타일 수
    int r;
    Vector2Int lastCenter = new Vector2Int(int.MinValue, int.MinValue);
    Vector3 startPos;
    bool ready;

    void Start()
    {
        if (groundTiles == null || groundTiles.Length == 0 || groundTiles[0] == null)
        {
            Debug.LogWarning("InfiniteMap: 바닥 타일 프리팹이 없습니다.");
            return;
        }

        container = new GameObject("InfiniteMapTiles").transform;
        player = PlayerLocator.Player;
        startPos = player != null ? player.position : Vector3.zero;

        // 첫 타일로 크기 측정
        var first = CreateTile(groundTiles[0]);
        tileSize = tileSizeOverride > 0f ? tileSizeOverride : MeasureTileSize(first.root);
        if (tileSize < 1f) tileSize = 10f;

        r = radius > 0 ? radius : Mathf.Clamp(Mathf.CeilToInt(minCoverage * 0.5f / tileSize), 1, 6);
        size = 2 * r + 1;

        tiles.Add(first);
        for (int i = 1; i < size * size; i++)
            tiles.Add(CreateTile(groundTiles[Random.Range(0, groundTiles.Length)]));

        ready = true;
    }

    Tile CreateTile(GameObject prefab)
    {
        var tile = new Tile();
        tile.root = Instantiate(prefab, Vector3.zero, Quaternion.identity, container).transform;
        foreach (var c in tile.root.GetComponentsInChildren<Collider>(true)) Destroy(c); // 충돌은 따로 있는 바닥 Plane이 담당
        tile.root.localScale = new Vector3(1f, Mathf.Max(0.001f, tileFlatten), 1f); // 둔덕을 눌러 평평하게 (충돌 바닥은 평평하므로 발이 빠지지 않게)

        // 피벗이 메시 중심이 아닐 수 있어 중심 오프셋을 기록
        var rs = tile.root.GetComponentsInChildren<Renderer>();
        if (rs.Length > 0)
        {
            var b = rs[0].bounds;
            foreach (var rd in rs) b.Encapsulate(rd.bounds);
            tile.centerOffset = new Vector2(b.center.x - tile.root.position.x, b.center.z - tile.root.position.z);
        }

        tile.anchor = new GameObject("PropAnchor").transform;
        tile.anchor.SetParent(container, false);

        if (props != null && props.Length > 0)
        {
            tile.props = new Transform[propsPerTile];
            for (int i = 0; i < propsPerTile; i++)
            {
                var prefab2 = props[Random.Range(0, props.Length)];
                if (prefab2 == null) continue;
                var p = Instantiate(prefab2, tile.anchor).transform;
                foreach (var c in p.GetComponentsInChildren<Collider>(true)) Destroy(c);
                tile.props[i] = p;
            }
        }
        return tile;
    }

    static float MeasureTileSize(Transform root)
    {
        var rs = root.GetComponentsInChildren<Renderer>();
        if (rs.Length == 0) return 10f;
        var b = rs[0].bounds;
        foreach (var rd in rs) b.Encapsulate(rd.bounds);
        return Mathf.Max(b.size.x, b.size.z);
    }

    void LateUpdate()
    {
        if (!ready) return;
        if (player == null) { player = PlayerLocator.Player; if (player == null) return; }

        // 충돌용 바닥은 플레이어를 따라다님 (높이는 유지)
        Vector3 pos = transform.position;
        transform.position = new Vector3(player.position.x, pos.y, player.position.z);

        var center = new Vector2Int(Mathf.RoundToInt(player.position.x / tileSize), Mathf.RoundToInt(player.position.z / tileSize));
        if (center == lastCenter) return;
        lastCenter = center;
        Rearrange(center);
    }

    // 각 타일 슬롯이 담당할 칸을 중심 칸 기준 [-r, r] 범위로 다시 정함 (벗어난 타일이 반대편으로 이동)
    void Rearrange(Vector2Int center)
    {
        for (int a = 0; a < size; a++)
        {
            for (int b = 0; b < size; b++)
            {
                var cell = new Vector2Int(
                    center.x - r + Mod(a - (center.x - r), size),
                    center.y - r + Mod(b - (center.y - r), size));
                var tile = tiles[a * size + b];
                if (tile.cell == cell) continue;

                tile.cell = cell;
                Vector3 cellCenter = new Vector3(cell.x * tileSize, groundY, cell.y * tileSize);
                // 타일끼리 겹쳐도 깜빡이지 않게 칸마다 아주 조금씩 높이를 다르게 함
                float y = groundY + tileYOffset + Mod(cell.x * 7 + cell.y * 3, 5) * 0.004f;
                tile.root.position = new Vector3(cellCenter.x - tile.centerOffset.x, y, cellCenter.z - tile.centerOffset.y);
                tile.anchor.position = new Vector3(cellCenter.x, groundY + tileYOffset, cellCenter.z);
                Scatter(tile, cell);
            }
        }
    }

    // 칸 좌표로 시드를 만들어서 같은 칸은 항상 같은 배치가 되게 함
    void Scatter(Tile tile, Vector2Int cell)
    {
        if (tile.props == null) return;
        var rng = new System.Random(cell.x * 73856093 ^ cell.y * 19349663 ^ 12345);
        float half = tileSize * 0.45f;
        float clearSqr = propClearRadius * propClearRadius;

        foreach (var p in tile.props)
        {
            if (p == null) continue;
            float x = (float)(rng.NextDouble() * 2 - 1) * half;
            float z = (float)(rng.NextDouble() * 2 - 1) * half;
            float yaw = (float)rng.NextDouble() * 360f;
            float scale = Mathf.Lerp(propScaleRange.x, propScaleRange.y, (float)rng.NextDouble());
            bool visible = rng.NextDouble() < propChance;

            p.localPosition = new Vector3(x, 0f, z);
            p.localRotation = Quaternion.Euler(0f, yaw, 0f);
            p.localScale = Vector3.one * scale;

            Vector3 w = p.position;
            w.y = startPos.y;
            if ((new Vector3(w.x - startPos.x, 0f, w.z - startPos.z)).sqrMagnitude < clearSqr) visible = false;
            p.gameObject.SetActive(visible);
        }
    }

    static int Mod(int v, int m) => ((v % m) + m) % m;
}
