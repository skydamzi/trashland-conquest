using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class Unit : MonoBehaviour
{
    public string unitName;
    public int unitLV;

    // 전투/스탯 관련 변수들
    public float baseAttackPower;   // 기본 공격력
    public float bonusAttackPower;  // 추가 공격력 (장비 등)
    public float armor;             // 방어력
    public float currentShield;     // 현재 실드
    public float maxShield;         // 최대 실드
    public float currentHP;         // 현재 체력
    public float maxHP;             // 최대 체력
    public float maxStamina;       // 최대 스태미나
    public float currentStamina;    // 현재 스태미나

    public float attackSpeed;       // 공격 속도
    public float criticalChance;    // 크리티컬 확률
    public float moveSpeed;         // 이동 속도
    public int currentEXP = 0;      // 현재 경험치
    public int maxEXP = 100;        // 최대 경험치

    // 기본 데미지 계산
    public float GetBaseDamage()
    {
        return baseAttackPower + bonusAttackPower;
    }

    // 근접 공격 데미지 (기본 데미지의 5배로 계산)
    public float GetMeleeDamage()
    {
        return GetBaseDamage() * 5f;
    }

    // 경험치 획득 함수
    public virtual void GainEXP(int amount)
    {
        // PlayerStatus에서 경험치 증가
        PlayerStatus.instance.currentEXP += amount;

        // Unit 클래스 내부에도 반영
        currentEXP = PlayerStatus.instance.currentEXP;
        maxEXP = PlayerStatus.instance.maxEXP;

        // 레벨업 반복 체크
        while (PlayerStatus.instance.currentEXP >= PlayerStatus.instance.maxEXP)
        {
            PlayerStatus.instance.currentEXP -= PlayerStatus.instance.maxEXP;

            // 내부 상태 갱신
            currentEXP = PlayerStatus.instance.currentEXP;
            maxEXP = PlayerStatus.instance.maxEXP;

            LevelUp(); // 레벨업 처리
        }
    }

    // 레벨업 처리 함수
    protected virtual void LevelUp()
    {
        // PlayerStatus 기준으로 레벨업 처리
        PlayerStatus.instance.unitLV++;
        PlayerStatus.instance.maxEXP += 20;
        PlayerStatus.instance.maxHP += 10;

        // 체력 회복 처리 (레벨 * 10만큼 회복, 단 최대 체력 초과 금지)
        PlayerStatus.instance.currentHP = Mathf.Min(
            PlayerStatus.instance.currentHP + PlayerStatus.instance.unitLV * 10,
            PlayerStatus.instance.maxHP
        );

        // Unit 클래스 내부 상태도 동기화
        unitLV = PlayerStatus.instance.unitLV;
        maxEXP = PlayerStatus.instance.maxEXP;
        maxHP = PlayerStatus.instance.maxHP;
        currentHP = PlayerStatus.instance.currentHP;
        maxStamina = PlayerStatus.instance.maxStamina;
        currentStamina = PlayerStatus.instance.currentStamina;

        Debug.Log($"{PlayerStatus.instance.unitName} 레벨업! 현재 레벨: {unitLV}");
    }
}
