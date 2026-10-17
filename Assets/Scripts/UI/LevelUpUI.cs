using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// 레벨업 시 게임을 멈추고 스킬 3택1 카드를 표시
public class LevelUpUI : MonoBehaviour
{
    [SerializeField] GameObject panel;
    [SerializeField] Transform cardContainer;
    [SerializeField] SkillCardUI cardPrefab;
    [SerializeField] int choiceCount = 3;
    [SerializeField] float noChoiceHeal = 30f; // 후보가 없을 때 체력 회복량

    SkillManager skillManager;
    PlayerExperience experience;
    Health playerHealth;

    readonly List<SkillCardUI> cards = new List<SkillCardUI>();
    List<SkillData> choices = new List<SkillData>();
    int pending;

    static bool isOpen;
    static int lastCloseFrame = -1;
    // 같은 프레임에 숫자키가 다른 디버그 입력과 겹치지 않게 하는 용도
    public static bool BlocksInput => isOpen || Time.frameCount == lastCloseFrame;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() { isOpen = false; lastCloseFrame = -1; }

    void Awake()
    {
        for (int i = 0; i < choiceCount; i++)
        {
            var card = Instantiate(cardPrefab, cardContainer);
            card.gameObject.SetActive(false);
            cards.Add(card);
        }
        panel.SetActive(false);
    }

    void Start()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            skillManager = player.GetComponent<SkillManager>();
            experience = player.GetComponent<PlayerExperience>();
            playerHealth = player.GetComponent<Health>();
        }
        if (experience != null) experience.OnLevelUp += HandleLevelUp;
        else Debug.LogWarning("LevelUpUI: PlayerExperience를 찾지 못했습니다.");
    }

    void OnDestroy()
    {
        if (experience != null) experience.OnLevelUp -= HandleLevelUp;
        if (isOpen) { isOpen = false; Time.timeScale = 1f; }
    }

    void HandleLevelUp(int newLevel)
    {
        pending++;
        if (!isOpen) ShowNext();
    }

    void ShowNext()
    {
        choices = skillManager != null ? skillManager.GetRandomChoices(choiceCount) : new List<SkillData>();

        if (choices.Count == 0)
        {
            // 후보가 없으면 패널 없이 보상
            pending--;
            if (playerHealth != null) playerHealth.Heal(noChoiceHeal);
            Debug.Log($"[LevelUp] 선택지가 없어 체력 {noChoiceHeal} 회복");
            if (pending > 0) ShowNext();
            return;
        }

        for (int i = 0; i < cards.Count; i++)
        {
            bool active = i < choices.Count;
            cards[i].gameObject.SetActive(active);
            if (active) cards[i].Setup(choices[i], skillManager.GetLevel(choices[i]), i, Pick);
        }

        panel.SetActive(true);
        isOpen = true;
        Time.timeScale = 0f;
    }

    void Pick(SkillData data)
    {
        if (!isOpen) return;
        skillManager.AddSkill(data);
        pending--;

        panel.SetActive(false);
        isOpen = false;
        lastCloseFrame = Time.frameCount;
        Time.timeScale = 1f;

        if (pending > 0) ShowNext(); // 한 번에 여러 레벨이 오른 경우
    }

    void Update()
    {
        if (!isOpen) return;
        var kb = Keyboard.current;
        if (kb == null) return;

        if (kb.digit1Key.wasPressedThisFrame && choices.Count > 0) Pick(choices[0]);
        else if (kb.digit2Key.wasPressedThisFrame && choices.Count > 1) Pick(choices[1]);
        else if (kb.digit3Key.wasPressedThisFrame && choices.Count > 2) Pick(choices[2]);
    }
}
