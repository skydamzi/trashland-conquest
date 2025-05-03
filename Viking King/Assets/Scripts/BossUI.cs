using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossUI : MonoBehaviour
{
    public GameObject who;             // 체력 주인
    public RectTransform fillRT;       // 체력바 fill 오브젝트
    private float prevHP = -1f;
    private Boss bossScript;

    void Start()
    {
        if (who != null)
            bossScript = who.GetComponent<Boss>(); // Boss 스크립트 연결
    }

    void Update()
    {
        if (bossScript == null)
        {
            if (fillRT != null)
                fillRT.gameObject.SetActive(false);
            return;
        }
        if (bossScript != null)
        {
            bossScript.currentHP = Mathf.Max(bossScript.currentHP, 0);

            float rate = bossScript.currentHP / bossScript.maxHP;
            if (bossScript.currentHP < prevHP)
            {
                StartCoroutine(ShakeUI(fillRT, 0.1f, 3f));
                StartCoroutine(ShakeUI(this.GetComponent<RectTransform>(), 0.1f, 5f));
            }

            prevHP = bossScript.currentHP;
            if (rate <= 0f)
            {
                fillRT.gameObject.SetActive(false);
            }
            else
            {
                fillRT.gameObject.SetActive(true);
                SetHPbar(rate);
            }
        }
    }

    public void SetHPbar(float rate)
    {
        fillRT.localScale = new Vector3(rate, 1f, 1f);
    }

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
