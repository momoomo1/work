using System.Collections;
using TMPro;
using UnityEngine;

// 보스 처치 시 "WAVE n CLEAR!" 텍스트를 잠시 표시 (게임은 계속 진행)
public class StageClearUI : MonoBehaviour
{
    [SerializeField] GameObject textObject;
    [SerializeField] float showTime = 4f;

    GameManager gm;

    void Start()
    {
        textObject.SetActive(false);
        gm = GameManager.Instance;
        gm.OnBossDefeated += Show;
    }

    void OnDestroy()
    {
        if (gm != null) gm.OnBossDefeated -= Show;
    }

    void Show()
    {
        // 무한 웨이브: 몇 번째 웨이브를 클리어했는지 표시
        var label = textObject.GetComponent<TMP_Text>();
        if (label != null) label.text = $"BOSS CLEAR!  난이도 {gm.Wave}";

        StopAllCoroutines();
        StartCoroutine(ShowRoutine());
    }

    IEnumerator ShowRoutine()
    {
        textObject.SetActive(true);
        yield return new WaitForSecondsRealtime(showTime);
        textObject.SetActive(false);
    }
}
