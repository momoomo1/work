using UnityEngine;

// 회전 칼날의 고리 하나: 세워 둔 도넛(토러스) 모양을 코드로 만들어 제자리에서 빙글빙글 돌린다.
// 옆에서 보면 타원, 정면에서 보면 원으로 번갈아 보여서 "고리가 도는" 느낌이 난다.
// 색은 프리팹의 머티리얼로 정한다. 타격 판정(OrbitBlade)과 같은 오브젝트에 붙는다.
[RequireComponent(typeof(MeshFilter))]
public class OrbitRingMesh : MonoBehaviour, IPoolable
{
    [SerializeField] float ringRadius = 0.55f;   // 고리 반지름
    [SerializeField] float tubeRadius = 0.07f;   // 고리 굵기
    [SerializeField] int ringSegments = 40;
    [SerializeField] int tubeSegments = 10;
    [SerializeField] float spinSpeed = 220f;     // 제자리 회전 속도(도/초)

    static Mesh cachedMesh;
    static float cachedRing, cachedTube;
    static int cachedRingSeg, cachedTubeSeg;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() => cachedMesh = null;

    void Awake()
    {
        GetComponent<MeshFilter>().sharedMesh = GetMesh(ringRadius, tubeRadius, ringSegments, tubeSegments);
    }

    public void OnSpawn()
    {
        // 고리마다 시작 각도를 다르게 해서 서로 다른 모양으로 보이게 함
        transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
    }

    public void OnDespawn() { }

    void Update()
    {
        transform.Rotate(0f, spinSpeed * Time.deltaTime, 0f, Space.World);
    }

    static Mesh GetMesh(float ringR, float tubeR, int ringSeg, int tubeSeg)
    {
        if (cachedMesh != null && cachedRing == ringR && cachedTube == tubeR && cachedRingSeg == ringSeg && cachedTubeSeg == tubeSeg)
            return cachedMesh;

        ringSeg = Mathf.Max(8, ringSeg);
        tubeSeg = Mathf.Max(4, tubeSeg);
        var vertices = new Vector3[(ringSeg + 1) * (tubeSeg + 1)];
        var normals = new Vector3[vertices.Length];
        var triangles = new int[ringSeg * tubeSeg * 6];

        // XY 평면에 놓인 토러스 (축은 Z)
        for (int i = 0; i <= ringSeg; i++)
        {
            float u = i / (float)ringSeg * Mathf.PI * 2f;
            float cu = Mathf.Cos(u), su = Mathf.Sin(u);
            for (int j = 0; j <= tubeSeg; j++)
            {
                float v = j / (float)tubeSeg * Mathf.PI * 2f;
                float cv = Mathf.Cos(v), sv = Mathf.Sin(v);
                int idx = i * (tubeSeg + 1) + j;
                vertices[idx] = new Vector3((ringR + tubeR * cv) * cu, (ringR + tubeR * cv) * su, tubeR * sv);
                normals[idx] = new Vector3(cv * cu, cv * su, sv);
            }
        }

        int t = 0;
        for (int i = 0; i < ringSeg; i++)
        {
            for (int j = 0; j < tubeSeg; j++)
            {
                int a = i * (tubeSeg + 1) + j;
                int b = (i + 1) * (tubeSeg + 1) + j;
                triangles[t++] = a; triangles[t++] = b; triangles[t++] = a + 1;
                triangles[t++] = b; triangles[t++] = b + 1; triangles[t++] = a + 1;
            }
        }

        var mesh = new Mesh { name = "OrbitRingTorus", vertices = vertices, normals = normals, triangles = triangles };
        mesh.RecalculateBounds();

        cachedMesh = mesh;
        cachedRing = ringR; cachedTube = tubeR; cachedRingSeg = ringSeg; cachedTubeSeg = tubeSeg;
        return mesh;
    }
}
