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
    public int currentShield;     // 현재 실드
    public int maxShield;         // 최대 실드
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
}
