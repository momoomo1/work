using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 화면 아래 가운데에 보유 스킬 아이콘을 한 줄로 표시하고 쿨타임을 알려준다. 씬 편집 없이 런타임에 생성된다.
// 쿨타임이 있는 스킬(투사체/폭발/장판/근접)은 아이콘이 어둡게 덮였다가 위에서부터 걷히고 남은 초를 보여준다.
// 회전 칼날·패시브는 쿨타임이 의미 없어서 아이콘과 레벨만 표시. 대쉬는 발밑 표시가 있어서 제외.
public class SkillBarUI : MonoBehaviour
{
    const float SlotSize = 64f;     // 기준 해상도(1920x1080) 픽셀
    const float SlotGap = 10f;
    const float BottomMargin = 28f;

    class Slot
    {
        public SkillData data;
        public SkillBase skill;
        public RectTransform root;
        public Image cover;
        public Text cooldownText;
        public Text levelText;
        public bool showCooldown;
        public int lastTenths = -1;
    }

    SkillManager manager;
    RectTransform holder;
    Font font;
    Sprite whiteSprite;
    readonly List<Slot> slots = new List<Slot>();

    public static void Create(SkillManager manager)
    {
        var go = new GameObject("SkillBarUI");
        go.AddComponent<SkillBarUI>().Build(manager);
    }

    void Build(SkillManager m)
    {
        manager = m;
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        whiteSprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f));

        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 30; // HUD(80), 레벨업 창(100)보다 뒤
        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        holder = new GameObject("Holder", typeof(RectTransform)).GetComponent<RectTransform>();
        holder.SetParent(transform, false);
        holder.anchorMin = holder.anchorMax = new Vector2(0.5f, 0f); // 아래 가운데
        holder.pivot = new Vector2(0.5f, 0f);
        holder.anchoredPosition = new Vector2(0f, BottomMargin);
        holder.sizeDelta = new Vector2(SlotSize, SlotSize);

        manager.OnSkillChanged += HandleChanged;
        foreach (var data in manager.AllSkills) // 이미 가진 스킬 반영
            if (data != null && manager.TryGetOwned(data, out _)) HandleChanged(data, manager.GetLevel(data));
    }

    void OnDestroy()
    {
        if (manager != null) manager.OnSkillChanged -= HandleChanged;
    }

    void HandleChanged(SkillData data, int level)
    {
        if (data == null || data.id == "dash" || data.id == "heal") return;

        int index = slots.FindIndex(s => s.data == data);
        if (level > 0 && manager.TryGetOwned(data, out var skill))
        {
            if (index < 0) { slots.Add(NewSlot(data)); index = slots.Count - 1; }
            var slot = slots[index];
            slot.skill = skill;
            slot.levelText.text = skill.IsMaxLevel ? "MAX" : "Lv" + level;
            Layout();
        }
        else if (index >= 0) // 진화로 대체된 스킬 제거
        {
            Destroy(slots[index].root.gameObject);
            slots.RemoveAt(index);
            Layout();
        }
    }

    Slot NewSlot(SkillData data)
    {
        var s = new Slot { data = data };
        s.showCooldown = data.type == SkillType.Projectile || data.type == SkillType.Explosion
                      || data.type == SkillType.Zone || data.type == SkillType.Melee;

        s.root = NewImage("Slot_" + data.id, holder, new Color(0f, 0f, 0f, 0.75f)).rectTransform;
        s.root.anchorMin = s.root.anchorMax = new Vector2(0f, 0.5f);
        s.root.pivot = new Vector2(0f, 0.5f);
        s.root.sizeDelta = new Vector2(SlotSize, SlotSize);

        var icon = NewImage("Icon", s.root, Color.white);
        Stretch(icon.rectTransform, 3f);
        if (data.icon != null) icon.sprite = data.icon;
        else
        {
            icon.sprite = whiteSprite; // 아이콘이 없으면 id 해시로 만든 색 사각형
            icon.color = Color.HSVToRGB(Mathf.Abs((data.id ?? data.name).GetHashCode() % 360) / 360f, 0.6f, 0.9f);
        }

        s.cover = NewImage("Cover", s.root, new Color(0f, 0f, 0f, 0.7f));
        Stretch(s.cover.rectTransform, 3f);
        s.cover.sprite = whiteSprite;
        s.cover.type = Image.Type.Filled;
        s.cover.fillMethod = Image.FillMethod.Vertical;
        s.cover.fillOrigin = (int)Image.OriginVertical.Top; // 위에서부터 걷힘
        s.cover.fillAmount = 0f;
        s.cover.gameObject.SetActive(s.showCooldown);

        s.cooldownText = NewText("Cooldown", s.root, 26, TextAnchor.MiddleCenter);
        Stretch(s.cooldownText.rectTransform, 0f);
        s.cooldownText.gameObject.SetActive(false);

        s.levelText = NewText("Level", s.root, 17, TextAnchor.LowerRight);
        Stretch(s.levelText.rectTransform, 4f);
        return s;
    }

    void Layout()
    {
        int n = slots.Count;
        float total = n * SlotSize + Mathf.Max(0, n - 1) * SlotGap;
        holder.sizeDelta = new Vector2(Mathf.Max(SlotSize, total), SlotSize);
        for (int i = 0; i < n; i++) slots[i].root.anchoredPosition = new Vector2(i * (SlotSize + SlotGap), 0f);
    }

    void Update()
    {
        foreach (var s in slots)
        {
            if (!s.showCooldown || s.skill == null) continue;
            float remain = s.skill.CooldownRemaining;
            float total = s.skill.Stat.cooldown / PlayerStats.AttackSpeed;
            bool show = remain > 0.05f && total >= 1f; // 쿨타임이 짧은 스킬(기본 공격 등)은 숫자 없이 덮개만
            s.cover.fillAmount = s.skill.CooldownRatio;
            s.cooldownText.gameObject.SetActive(show);
            if (!show) { s.lastTenths = -1; continue; }
            int shown = remain >= 10f ? Mathf.CeilToInt(remain) * 10 : Mathf.RoundToInt(remain * 10f); // 바뀔 때만 문자열 생성
            if (shown == s.lastTenths) continue;
            s.lastTenths = shown;
            s.cooldownText.text = remain >= 10f ? Mathf.CeilToInt(remain).ToString() : remain.ToString("0.0");
        }
    }

    static void Stretch(RectTransform rt, float inset)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(inset, inset);
        rt.offsetMax = new Vector2(-inset, -inset);
    }

    static Image NewImage(string name, Transform parent, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var img = go.GetComponent<Image>();
        img.color = color;
        img.raycastTarget = false;
        return img;
    }

    Text NewText(string name, Transform parent, int size, TextAnchor anchor)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        var t = go.GetComponent<Text>();
        t.font = font;
        t.fontSize = size;
        t.fontStyle = FontStyle.Bold;
        t.alignment = anchor;
        t.color = Color.white;
        t.raycastTarget = false;
        go.AddComponent<Outline>().effectColor = new Color(0f, 0f, 0f, 0.9f);
        return t;
    }
}