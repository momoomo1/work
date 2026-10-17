using System.Collections;
using UnityEngine;

// 보스 처치 시 "STAGE CLEAR" 텍스트를 잠시 표시 (게임은 계속 진행)
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
