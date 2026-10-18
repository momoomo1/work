using UnityEngine;
using UnityEngine.UI;

// 플레이어 발밑에 따라다니는 대쉬 쿨타임 표시 (월드 공간 UI, 씬 편집 없이 런타임에 생성).
// 칸 하나가 대쉬 1회: 꽉 찬 칸은 밝은 하늘색, 충전 중인 칸은 아래에서부터 차오르고, 빈 칸은 어두운 회색.
public class DashUI : MonoBehaviour
{
    const float PipWidth = 60f;
    const float PipHeight = 16f;
    const float PipGap = 10f;
    const float WorldScale = 0.014f;   // UI 픽셀 → 월드 유닛

    static readonly Color BackColor = new Color(0f, 0f, 0f, 0.65f);
    static readonly Color ReadyColor = new Color(0.4f, 0.92f, 1f, 1f);
    static readonly Color ChargingColor = new Color(0.45f, 0.62f, 0.72f, 0.95f);

    [Header("위치")]
    float footDrop = 0.8f;       // 플레이어 중심에서 아래로 (발 근처)
    float towardCamera = 1.0f;   // 화면상 아래쪽으로 밀어내는 거리

    PlayerDash dash;
    Camera cam;
    RectTransform[] fills;
    RectTransform[] pips;
    float[] pulse;
    int lastCharges;

    public static void Create(PlayerDash dash)
    {
        var go = new GameObject("DashUI");
        go.AddComponent<DashUI>().Build(dash);
    }

    void Build(PlayerDash target)
    {
        dash = target;
        int n = dash.MaxCharges;
        float totalWidth = n * PipWidth + (n - 1) * PipGap;

        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 10;
        var rt = (RectTransform)transform;
        rt.sizeDelta = new Vector2(totalWidth, PipHeight);
        rt.localScale = Vector3.one * WorldScale;

        fills = new RectTransform[n];
        pips = new RectTransform[n];
        pulse = new float[n];
        for (int i = 0; i < n; i++)
        {
            var pip = NewImage("Pip" + i, transform, BackColor);
            pip.anchorMin = pip.anchorMax = new Vector2(0f, 0.5f);
            pip.pivot = new Vector2(0.5f, 0.5f);
            pip.sizeDelta = new Vector2(PipWidth, PipHeight);
            pip.anchoredPosition = new Vector2(PipWidth * 0.5f + i * (PipWidth + PipGap), 0f);
            pips[i] = pip;

            var fill = NewImage("Fill", pip, ReadyColor);
            fill.anchorMin = Vector2.zero;
            fill.anchorMax = new Vector2(0f, 1f);   // 오른쪽 끝(anchorMax.x)으로 채움 정도를 조절
            fill.offsetMin = new Vector2(2f, 2f);
            fill.offsetMax = new Vector2(-2f, -2f);
            fills[i] = fill;
        }
        lastCharges = dash.Charges;
    }

    static RectTransform NewImage(string name, Transform parent, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        var rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        var img = go.GetComponent<Image>();
        img.color = color;
        img.raycastTarget = false;
        return rt;
    }

    void LateUpdate()
    {
        if (dash == null) { Destroy(gameObject); return; }
        if (cam == null) cam = Camera.main;
        if (cam == null) return;

        // 플레이어 발밑(화면상 아래쪽)에 두고 카메라를 바라보게 함
        Vector3 camForward = Vector3.ProjectOnPlane(cam.transform.forward, Vector3.up).normalized;
        transform.position = dash.transform.position + Vector3.down * footDrop - camForward * towardCamera;
        transform.rotation = cam.transform.rotation;

        // 칸 채우기
        int charges = dash.Charges;
        if (charges > lastCharges) pulse[charges - 1] = 0.3f; // 충전 완료 시 잠깐 커지는 연출
        lastCharges = charges;

        for (int i = 0; i < fills.Length; i++)
        {
            float amount;
            Color color;
            if (i < charges) { amount = 1f; color = ReadyColor; }
            else if (i == charges) { amount = dash.RechargeProgress; color = ChargingColor; }   // 지금 차오르는 칸
            else { amount = 0f; color = ChargingColor; }

            var fill = fills[i];
            fill.anchorMax = new Vector2(Mathf.Clamp01(amount), 1f);
            fill.GetComponent<Image>().color = color;

            if (pulse[i] > 0f)
            {
                pulse[i] -= Time.unscaledDeltaTime;
                pips[i].localScale = Vector3.one * (1f + Mathf.Clamp01(pulse[i] / 0.3f) * 0.35f);
            }
            else pips[i].localScale = Vector3.one;
        }
    }
}
