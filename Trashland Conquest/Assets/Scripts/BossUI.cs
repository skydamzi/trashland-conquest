using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossUI : MonoBehaviour
{
    public GameObject who;             // ü�� ����
    public RectTransform fillRT;       // ü�¹� fill ������Ʈ
    public RectTransform orangeFillRT;

    private float prevHP = -1f;
    private Boss bossScript;
    private float delayedRate = 1f;
    private float lastHitTime = -10f;
    public float delayBuffer = 1f;
    public float delayDuration = 300f;

    void Start()
    {
        if (who != null)
            bossScript = who.GetComponent<Boss>(); // Boss ��ũ��Ʈ ����
    }

    /*void Update()
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
    }*/

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

        // ���� ü�¹� �ݿ�
        SetHPbar(rate);

        // �ǰݵǾ����� lastHitTime ����
        if (rate < prevHP)
        {
            lastHitTime = Time.time;
            StartCoroutine(ShakeUI(fillRT, 0.1f, 3f));
            StartCoroutine(ShakeUI(this.GetComponent<RectTransform>(), 0.1f, 5f));
        }

        // ��Ȳ�� �ܻ�ٴ� ���� ���� �� �����ð� �ڿ� ��� ����
        if (Time.time - lastHitTime > delayBuffer)
        {
            if (delayedRate > rate)
            {
                delayedRate -= Time.deltaTime / delayDuration;
                delayedRate = Mathf.Max(delayedRate, rate);
            }
        }

        SetOrangeBar(delayedRate);

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
