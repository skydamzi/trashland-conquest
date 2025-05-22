using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossUI : MonoBehaviour
{
    public GameObject who;
    public RectTransform fillRT;
    public RectTransform orangeFillRT;

    private float prevHP = -1f;
    private Boss bossScript;
    private float delayedRate = 1f;
    private float lastHitTime = -10f;
    public float delayBuffer = 1f;
    public float delayDuration = 300f;

    private bool isShaking = false;

    void Start()
    {
        if (who != null)
            bossScript = who.GetComponent<Boss>();
    }

    void Update()
    {
        if (bossScript == null)
        {
            SetBarsActive(false);
            return;
        }

        float currentHP = Mathf.Max(bossScript.currentHP, 0f);
        float rate = currentHP / bossScript.maxHP;

        if (currentHP != prevHP)
        {
            SetHPbar(rate);
            fillRT.gameObject.SetActive(rate > 0f);

            if (rate < prevHP)
            {
                lastHitTime = Time.time;
                if (!isShaking)
                {
                    StartCoroutine(ShakeUI(fillRT, 0.1f, 3f));
                    StartCoroutine(ShakeUI(GetComponent<RectTransform>(), 0.1f, 5f));
                }
            }
            prevHP = rate;
        }

        if (Time.time - lastHitTime > delayBuffer)
        {
            if (delayedRate > rate)
            {
                delayedRate -= Time.deltaTime / delayDuration;
                delayedRate = Mathf.Max(delayedRate, rate);
                SetOrangeBar(delayedRate);
            }
        }

        orangeFillRT.gameObject.SetActive(rate > 0f);
    }

    private void SetBarsActive(bool isActive)
    {
        if (fillRT != null) fillRT.gameObject.SetActive(isActive);
        if (orangeFillRT != null) orangeFillRT.gameObject.SetActive(isActive);
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

    IEnumerator ShakeUI(RectTransform target, float duration = 0.1f, float magnitude = 5f)
    {
        isShaking = true;
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
        isShaking = false;
    }
}
