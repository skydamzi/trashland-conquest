using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    public GameObject who;             // 체력 주인
    public RectTransform fillRT;       // 체력바 fill 오브젝트
    public Text attack_powerText;
    public Text levelText;

    private Player playerScript;

    void Start()
    {
        if (who != null)
            playerScript = who.GetComponent<Player>();
    }

    void Update()
    {
        if (playerScript != null)
        {
            float rate = (float)playerScript.currentHP / playerScript.maxHP;
            SetHPbar(rate);

            attack_powerText.text = $"공격력: {playerScript.baseAttackPower} + {playerScript.bonusAttackPower}";
            levelText.text = $"레벨: {playerScript.unitLV}";
        }
    }

    public void SetHPbar(float rate)
    {
        if (playerScript.currentHP <= 0)
        {
            playerScript.currentHP = 0;
        }
        else
            fillRT.localScale = new Vector2(rate, 1f);
    }
}
