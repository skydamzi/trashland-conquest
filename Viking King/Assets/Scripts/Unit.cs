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

    public float TotalAttack()
    {
        return baseAttackPower + bonusAttackPower;
    }
}
