using UnityEngine;
using UnityEngine.UI;

// 플레이어 발밑에 따라다니는 대쉬 쿨타임 표시.
// 화면 공간(Overlay) UI라서 바닥 풀/소품에 가려지지 않는다. 씬 편집 없이 런타임에 생성된다.
// 칸 하나가 대쉬 1회: 꽉 찬 칸은 밝은 하늘색, 충전 중인 칸은 왼쪽부터 차오르고, 빈 칸은 어두운 회색.
public class DashUI : MonoBehaviour
{
    const float PipWidth = 56f;     // 기준 해상도(1920x1080) 픽셀
    const float PipHeight = 14f;
    const float PipGap = 8f;
    const float BelowFeet = 26f;    // 발 위치에서 화면 아래로 내리는 픽셀

    static readonly Color BackColor = new Color(0f, 0f, 0f, 0.7f);
    static readonly Color ReadyColor = new Color(0.4f, 0.92f, 1f, 1f);
    static readonly Color ChargingColor = new Color(0.45f, 0.62f, 0.72f, 0.95f);

    PlayerDash dash;
    Camera cam;
    RectTransform canvasRect;
    RectTransform holder;
    RectTransform[] fills;
    RectTransform[] pips;
    Image[] fillImages;
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
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 30; // HUD(80), 레벨업 창(100)보다 뒤
        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        canvasRect = (RectTransform)transform;

        holder = new GameObject("Holder", typeof(RectTransform)).GetComponent<RectTransform>();
        holder.SetParent(transform, false);
        holder.anchorMin = holder.anchorMax = new Vector2(0.5f, 0.5f);
        holder.pivot = new Vector2(0.5f, 1f); // 위쪽 가운데 기준 (발 바로 아래에 붙음)
        holder.sizeDelta = new Vector2(totalWidth, PipHeight);

        fills = new RectTransform[n];
        pips = new RectTransform[n];
        fillImages = new Image[n];
        pulse = new float[n];
        for (int i = 0; i < n; i++)
        {
            var pip = NewImage("Pip" + i, holder, BackColor);
            pip.anchorMin = pip.anchorMax = new Vector2(0f, 0.5f);
            pip.pivot = new Vector2(0.5f, 0.5f);
            pip.sizeDelta = new Vector2(PipWidth, PipHeight);
            pip.anchoredPosition = new Vector2(PipWidth * 0.5f + i * (PipWidth + PipGap), 0f);
            pips[i] = pip;

            var fill = NewImage("Fill", pip, ReadyColor);
            fill.anchorMin = Vector2.zero;
            fill.anchorMax = new Vector2(0f, 1f);   // anchorMax.x로 채움 정도를 조절
            fill.offsetMin = new Vector2(2f, 2f);
            fill.offsetMax = new Vector2(-2f, -2f);
            fills[i] = fill;
            fillImages[i] = fill.GetComponent<Image>();
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

        // 플레이어 발 위치를 화면 좌표로 바꿔서 그 바로 아래에 둠
        Vector3 feet = dash.transform.position + Vector3.down * 1.0f;
        Vector3 screen = cam.WorldToScreenPoint(feet);
        screen.y -= BelowFeet * (Screen.height / 1080f); // 해상도 비율에 맞춰 내림
        if (screen.z > 0f && RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screen, null, out Vector2 local))
            holder.anchoredPosition = local;

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

            fills[i].anchorMax = new Vector2(Mathf.Clamp01(amount), 1f);
            fillImages[i].color = color;

            if (pulse[i] > 0f)
            {
                pulse[i] -= Time.unscaledDeltaTime;
                pips[i].localScale = Vector3.one * (1f + Mathf.Clamp01(pulse[i] / 0.3f) * 0.35f);
            }
            else pips[i].localScale = Vector3.one;
        }
    }
}
