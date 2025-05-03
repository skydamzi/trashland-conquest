using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    public GameObject who;             // 체력 주인
    public RectTransform fillRT;       // 체력바 fill 오브젝트
    public RectTransform shieldRT;
    public Text hpText;
    public Text attack_powerText;
    public Text armorText;
    public Text levelText;
    public Text nameText;
    private Player playerScript;
    private float prevHP = -1f;

    void Start()
    {
        if (who != null)
            playerScript = who.GetComponent<Player>();
    }

    void Update()
    {
        if (playerScript != null)
        {
            playerScript.currentHP = Mathf.Max(playerScript.currentHP, 0);
            playerScript.currentShield = Mathf.Max(playerScript.currentShield, 0);

            float rateHP = playerScript.currentHP / playerScript.maxHP;
            float rateShield = playerScript.currentShield / playerScript.maxShield;
            if (playerScript.currentHP < prevHP)
            {
                StartCoroutine(ShakeUI(fillRT, 0.1f, 3f));
                StartCoroutine(ShakeUI(this.GetComponent<RectTransform>(), 0.1f, 5f));
            }
            if (playerScript.currentShield > 0f)
            {
                shieldRT.gameObject.SetActive(true);
                SetShieldbar(rateShield);
            }
            else
                shieldRT.gameObject.SetActive(false);

            prevHP = playerScript.currentHP;
            if (rateHP <= 0f)
            {
                fillRT.gameObject.SetActive(false);
            }
            else
            {
                fillRT.gameObject.SetActive(true);
                SetHPbar(rateHP);
            }
            hpText.text = $"{playerScript.currentHP} / {playerScript.maxHP}";
            nameText.text = $"이름: {playerScript.unitName}";
            levelText.text = $"레벨: {playerScript.unitLV}";
            attack_powerText.text = $"공격력: {playerScript.baseAttackPower} (+ {playerScript.bonusAttackPower})";
            armorText.text = $"방어력: {playerScript.armor}";
        }
    }
    public void SetHPbar(float rate)
    {
        fillRT.localScale = new Vector3(rate, 1f, 1f);
    }
    public void SetShieldbar(float rate)
    {
        shieldRT.localScale = new Vector3(rate, 1f, 1f);
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
