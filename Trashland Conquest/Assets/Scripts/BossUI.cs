using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossUI : MonoBehaviour
{
    public GameObject who;             // 체력을 표시할 대상 (보스)
    public RectTransform fillRT;       // 실제 체력 바 이미지 (빨간 바 등)
    public RectTransform orangeFillRT; // 지연 표시용 체력 바 (주황색 등)

    private float prevHP = -1f;
    private Boss bossScript;
    private float delayedRate = 1f;
    private float lastHitTime = -10f;
    public float delayBuffer = 1f;     // 데미지 받은 후 지연 시작까지의 대기 시간
    public float delayDuration = 300f; // 지연 체력바가 줄어드는 데 걸리는 시간 (비율 조절됨)

    void Start()
    {
        if (who != null)
            bossScript = who.GetComponent<Boss>(); // Boss 스크립트 참조 가져오기
    }

    void Update()
    {
        if (bossScript == null)
        {
            if (fillRT != null) fillRT.gameObject.SetActive(false);
            if (orangeFillRT != null) orangeFillRT.gameObject.SetActive(false);
            return;
        }

        bossScript.currentHP = Mathf.Max(bossScript.currentHP, 0f);
        float rate = bossScript.currentHP / bossScript.maxHP;

        // 실시간 체력바 즉시 반영
        SetHPbar(rate);

        // 데미지를 받았다면 lastHitTime 업데이트
        if (rate < prevHP)
        {
            lastHitTime = Time.time;
            StartCoroutine(ShakeUI(fillRT, 0.1f, 3f));
            StartCoroutine(ShakeUI(this.GetComponent<RectTransform>(), 0.1f, 5f));
        }

        // 일정 시간 후부터 주황색 체력바 천천히 감소
        if (Time.time - lastHitTime > delayBuffer)
        {
            if (delayedRate > rate)
            {
                delayedRate -= Time.deltaTime / delayDuration;
                delayedRate = Mathf.Max(delayedRate, rate);
            }
        }

        // 주황색 체력바 반영
        SetOrangeBar(delayedRate);

        // 체력이 0 이상일 때만 체력바 표시
        fillRT.gameObject.SetActive(rate > 0f);
        orangeFillRT.gameObject.SetActive(rate > 0f);

        prevHP = rate;
    }

    public void SetOrangeBar(float rate)
    {
        if (orangeFillRT != null)
            orangeFillRT.localScale = new Vector3(rate, 1f, 1f);
    }

    public void SetHPbar(float rate)
    {
        if (fillRT != null)
            fillRT.localScale = new Vector3(rate, 1f, 1f);
    }

    // 체력 깎일 때 UI를 흔들리는 효과
    IEnumerator ShakeUI(RectTransform target, float duration = 0.1f, float magnitude = 5f)
    {
        Vector3 originalPos = target.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float offsetX = Random.Range(-1f, 1f) * magnitude;
            float offsetY = Random.Range(-1f, 1f) * magnitude;
            target.anchoredPosition = originalPos + new Vector3(offsetX, offsetY, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        target.anchoredPosition = originalPos;
    }
}
