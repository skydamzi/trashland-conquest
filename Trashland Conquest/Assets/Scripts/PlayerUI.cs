using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    public GameObject who;
    public RectTransform fillRT;
    public RectTransform shieldRT;
    public Text hpText;
    public RectTransform expRT;
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

    [Header("Ammo UI")]
    public Text bulletGaugeText;

    private PlayerStatus playerStatus;
    private TraitSynergy traitSynergy;

    private float prevHP = -1f;
    private float prevShield = -1f;
    private int prevEXP = -1;
    private int prevLV = -1;
    private float prevBaseAtk = -1;
    private float prevBonusAtk = -1;
    private float prevArmor = -1;

    private Dictionary<TraitSynergy.TraitType, int> prevTraits = new();

    private bool isShaking = false;

    void Start()
    {
        playerStatus = PlayerStatus.instance;
        traitSynergy = TraitSynergy.Instance;
    }

    void Update()
    {
        if (playerStatus == null) return;

        UpdateHP();
        UpdateShield();
        UpdateEXP();
        UpdateStatusText();
        UpdateTraits();
    }

    void UpdateHP()
    {
        float currentHP = Mathf.Max(playerStatus.currentHP, 0);
        if (currentHP != prevHP)
        {
            float rate = currentHP / playerStatus.maxHP;
            SetHPbar(rate);
            hpText.text = $"{currentHP} / {playerStatus.maxHP}";
            fillRT.gameObject.SetActive(rate > 0);
            if (currentHP < prevHP && !isShaking)
            {
                StartCoroutine(ShakeUI(fillRT, 0.1f, 3f));
                StartCoroutine(ShakeUI(GetComponent<RectTransform>(), 0.1f, 5f));
            }
            prevHP = currentHP;
        }
    }

    void UpdateShield()
    {
        float currentShield = Mathf.Max(playerStatus.currentShield, 0);
        if (currentShield != prevShield)
        {
            shieldRT.gameObject.SetActive(currentShield > 0);
            SetShieldbar(currentShield / playerStatus.maxShield);
            prevShield = currentShield;
        }
    }

    void UpdateEXP()
    {
        if (playerStatus.currentEXP != prevEXP)
        {
            float rate = (float)playerStatus.currentEXP / playerStatus.maxEXP;
            expRT.localScale = new Vector3(rate, 1f, 1f);
            expText.text = $"{playerStatus.currentEXP} / {playerStatus.maxEXP}";
            prevEXP = playerStatus.currentEXP;
        }
    }

    void UpdateStatusText()
    {
        if (playerStatus.unitLV != prevLV)
        {
            levelText.text = $"레벨: {playerStatus.unitLV}";
            prevLV = playerStatus.unitLV;
        }
        if (playerStatus.unitName != nameText.text)
        {
            nameText.text = $"이름: {playerStatus.unitName}";
        }
        if (playerStatus.armor != prevArmor)
        {
            armorText.text = $"방어력: {playerStatus.armor}";
            prevArmor = playerStatus.armor;
        }
        if (playerStatus.baseAttackPower != prevBaseAtk || playerStatus.bonusAttackPower != prevBonusAtk)
        {
            attack_powerText.text = $"공격력: {playerStatus.baseAttackPower} (+ {playerStatus.bonusAttackPower})";
            prevBaseAtk = playerStatus.baseAttackPower;
            prevBonusAtk = playerStatus.bonusAttackPower;
        }
    }

    void UpdateTraits()
    {
        foreach (TraitSynergy.TraitType type in System.Enum.GetValues(typeof(TraitSynergy.TraitType)))
        {
            int current = traitSynergy.GetStack(type);
            if (!prevTraits.ContainsKey(type) || prevTraits[type] != current)
            {
                prevTraits[type] = current;
                UpdateTraitText(type, current);
            }
        }
    }

    void UpdateTraitText(TraitSynergy.TraitType type, int value)
    {
        switch (type)
        {
            case TraitSynergy.TraitType.Milk: milkText.text = $"우유: {value}"; break;
            case TraitSynergy.TraitType.Slush: slushText.text = $"슬러시: {value}"; break;
            case TraitSynergy.TraitType.Alcohol: alcoholText.text = $"알코올: {value}"; break;
            case TraitSynergy.TraitType.Soda: sodaText.text = $"탄산: {value}"; break;
            case TraitSynergy.TraitType.EnergyDrink: energyDrinkText.text = $"이온: {value}"; break;
            case TraitSynergy.TraitType.Coffee: coffeeText.text = $"커피: {value}"; break;
            case TraitSynergy.TraitType.Pesticide: pesticideText.text = $"농약: {value}"; break;
            case TraitSynergy.TraitType.PurifiedWater: purifiedWaterText.text = $"정제수: {value}"; break;
        }
    }

    public void UpdateBulletGauge(int currentAmmo, int maxAmmo)
    {
        if (currentAmmo > 9)
            bulletGaugeText.text = $"{currentAmmo}";
        else
            bulletGaugeText.text = $"  {currentAmmo}";
    }

    public IEnumerator ReloadBulletGauge(int from, int to, float interval = 0.05f)
    {
        for (int i = from; i <= to; i++)
        {
            UpdateBulletGauge(i, to);
            yield return new WaitForSeconds(interval);
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
        if (isShaking) yield break;
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
