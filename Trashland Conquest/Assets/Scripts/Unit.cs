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
    public float criticalChance;
    public float moveSpeed;
    public int currentEXP = 0;
    public int maxEXP = 100;
    public float GetBaseDamage()
    {
        return baseAttackPower + bonusAttackPower;
    }
    public float GetMeleeDamage()
    {
        return GetBaseDamage() * 5f;
    }

    public virtual void GainEXP(int amount)
    {
        // PlayerStatus 갱신
        PlayerStatus.instance.currentEXP += amount;

        // Unit 변수에도 반영
        currentEXP = PlayerStatus.instance.currentEXP;
        maxEXP = PlayerStatus.instance.maxEXP;

        while (PlayerStatus.instance.currentEXP >= PlayerStatus.instance.maxEXP)
        {
            PlayerStatus.instance.currentEXP -= PlayerStatus.instance.maxEXP;

            // 동기화
            currentEXP = PlayerStatus.instance.currentEXP;
            maxEXP = PlayerStatus.instance.maxEXP;

            LevelUp();
        }
    }

    protected virtual void LevelUp()
    {
        // PlayerStatus 내부 값 갱신
        PlayerStatus.instance.unitLV++;
        PlayerStatus.instance.maxEXP += 20;
        PlayerStatus.instance.maxHP += 10;

        // 체력은 회복하되, maxHP를 넘지 않도록 처리
        PlayerStatus.instance.currentHP = Mathf.Min(
            PlayerStatus.instance.currentHP + PlayerStatus.instance.unitLV * 10,
            PlayerStatus.instance.maxHP
        );

        // Unit 쪽 변수도 동기화
        unitLV = PlayerStatus.instance.unitLV;
        maxEXP = PlayerStatus.instance.maxEXP;
        maxHP = PlayerStatus.instance.maxHP;
        currentHP = PlayerStatus.instance.currentHP;

        Debug.Log($"{PlayerStatus.instance.unitName} 레벨업! 현재 레벨: {unitLV}");
    }
}
