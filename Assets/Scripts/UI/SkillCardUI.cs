using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillCardUI : MonoBehaviour
{
    [SerializeField] Button button;
    [SerializeField] Image icon;
    [SerializeField] TMP_Text nameText;
    [SerializeField] TMP_Text levelText;
    [SerializeField] TMP_Text descText;
    [SerializeField] TMP_Text keyText;

    SkillData data;
    Action<SkillData> onPick;

    void Awake() => button.onClick.AddListener(() => onPick?.Invoke(data));

    public void Setup(SkillData skill, int currentLevel, int index, Action<SkillData> pick)
    {
        data = skill;
        onPick = pick;

        nameText.text = skill.displayName;
        descText.text = skill.description;
        keyText.text = $"[{index + 1}]";

        if (skill.id == "heal")
            levelText.text = "즉시 회복";
        else if (skill.IsEvolution)
            levelText.text = $"진화! {skill.evolvesFrom.displayName} → {skill.displayName}";
        else if (currentLevel <= 0)
            levelText.text = "NEW";
        else
            levelText.text = $"Lv {currentLevel} → Lv {currentLevel + 1}";

        if (skill.icon != null)
        {
            icon.sprite = skill.icon;
            icon.color = Color.white;
        }
        else
        {
            icon.sprite = null; // 아이콘이 없으면 id 해시로 만든 색 사각형
            float hue = Mathf.Abs((skill.id ?? skill.name).GetHashCode() % 360) / 360f;
            icon.color = Color.HSVToRGB(hue, 0.6f, 0.9f);
        }
    }
}
