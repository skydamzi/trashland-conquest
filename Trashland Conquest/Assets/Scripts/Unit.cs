using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class Unit : MonoBehaviour
{
    public string unitName;
    public int unitLV;

    // 공격/방어 관련
    public float baseAttackPower;   // 기본 공격력
    public float bonusAttackPower;  // 추가 공격력 (버프 등)
    public float armor;             // 방어력
    public float currentShield;     // 현재 쉴드
    public float maxShield;         // 최대 쉴드
    public float currentHP;
    public float maxHP;
    public int currentEXP = 0;
    public int maxEXP = 100;
    public float GetBaseDamage()
    {
        return baseAttackPower + bonusAttackPower;
    }
    public float GetMeleeDamage()
    {
        return GetBaseDamage() * 3f;
    }

    public virtual void GainEXP(int amount)
    {
        // PlayerStatus 갱신
        PlayerStatus.Instance.currentEXP += amount;

        // Unit 변수에도 반영
        currentEXP = PlayerStatus.Instance.currentEXP;
        maxEXP = PlayerStatus.Instance.maxEXP;

        while (PlayerStatus.Instance.currentEXP >= PlayerStatus.Instance.maxEXP)
        {
            PlayerStatus.Instance.currentEXP -= PlayerStatus.Instance.maxEXP;

            // 동기화
            currentEXP = PlayerStatus.Instance.currentEXP;
            maxEXP = PlayerStatus.Instance.maxEXP;

            LevelUp();
        }
    }

    protected virtual void LevelUp()
    {
        // PlayerStatus 내부 값 갱신
        PlayerStatus.Instance.unitLV++;
        PlayerStatus.Instance.maxEXP += 20;
        PlayerStatus.Instance.maxHP += 10;

        // 체력은 회복하되, maxHP를 넘지 않도록 처리
        PlayerStatus.Instance.currentHP = Mathf.Min(
            PlayerStatus.Instance.currentHP + PlayerStatus.Instance.unitLV * 10,
            PlayerStatus.Instance.maxHP
        );

        // Unit 쪽 변수도 동기화
        unitLV = PlayerStatus.Instance.unitLV;
        maxEXP = PlayerStatus.Instance.maxEXP;
        maxHP = PlayerStatus.Instance.maxHP;
        currentHP = PlayerStatus.Instance.currentHP;

        Debug.Log($"{PlayerStatus.Instance.unitName} 레벨업! 현재 레벨: {unitLV}");
    }
}
