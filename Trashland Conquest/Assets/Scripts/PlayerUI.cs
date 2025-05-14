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
    public RectTransform expRT;  // 경험치 바
    public Text expText;

    public Text attack_powerText;
    public Text armorText;
    public Text levelText;
    public Text nameText;

    public Text milkText;
    public Text slushText;
    public Text alcoholText;
    public Text sodaText;
    public Text energyDrinkText;
    public Text coffeeText;
    public Text pesticideText;
    public Text purifiedWaterText;

    private PlayerStatus playerStatus;
    private TraitSynergy traitSynergy;
    private float prevHP = -1f;

    void Start()
    {
        playerStatus = PlayerStatus.instance;
        traitSynergy = TraitSynergy.Instance;

    }
    void Update()
    {
        if (playerStatus != null)
        {
            playerStatus.currentHP = Mathf.Max(playerStatus.currentHP, 0);
            playerStatus.currentShield = Mathf.Max(playerStatus.currentShield, 0);

            float rateHP = playerStatus.currentHP / playerStatus.maxHP;
            float rateShield = playerStatus .currentShield / playerStatus.maxShield;
            if (playerStatus.currentHP < prevHP)
            {
                StartCoroutine(ShakeUI(fillRT, 0.1f, 3f));
                StartCoroutine(ShakeUI(this.GetComponent<RectTransform>(), 0.1f, 5f));
            }
            if (playerStatus.currentShield > 0f)
            {
                shieldRT.gameObject.SetActive(true);
                SetShieldbar(rateShield);
            }
            else
                shieldRT.gameObject.SetActive(false);

            prevHP = playerStatus.currentHP;
            if (rateHP <= 0f)
            {
                fillRT.gameObject.SetActive(false);
            }
            else
            {
                fillRT.gameObject.SetActive(true);
                SetHPbar(rateHP);
            }
            hpText.text = $"{playerStatus.currentHP} / {playerStatus.maxHP}";

            float expRate = (float)playerStatus.currentEXP / playerStatus.maxEXP;
            expRT.localScale = new Vector3(expRate, 1f, 1f);
            expText.text = $"{playerStatus.currentEXP} / {playerStatus.maxEXP}";

            // 속성 시너지 표시 UI
            nameText.text = $"이름: {playerStatus.unitName}";
            levelText.text = $"레벨: {playerStatus.unitLV}";
            attack_powerText.text = $"공격력: {playerStatus.baseAttackPower} (+ {playerStatus.bonusAttackPower})";
            armorText.text = $"방어력: {playerStatus.armor}";
            milkText.text = $"우유: {traitSynergy.GetStack(TraitSynergy.TraitType.Milk)}";
            slushText.text = $"슬러시: {traitSynergy.GetStack(TraitSynergy.TraitType.Slush)}";
            alcoholText.text = $"술: {traitSynergy.GetStack(TraitSynergy.TraitType.Alcohol)}";
            sodaText.text = $"탄산: {traitSynergy.GetStack(TraitSynergy.TraitType.Soda)}";
            energyDrinkText.text = $"이온: {traitSynergy.GetStack(TraitSynergy.TraitType.EnergyDrink)}";
            coffeeText.text = $"커피: {traitSynergy.GetStack(TraitSynergy.TraitType.Coffee)}";
            pesticideText.text = $"농약: {traitSynergy.GetStack(TraitSynergy.TraitType.Pesticide)}";
            purifiedWaterText.text = $"정제수: {traitSynergy.GetStack(TraitSynergy.TraitType.PurifiedWater)}";
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
