using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatus : MonoBehaviour
{
    public static PlayerStatus instance;

    // 기본 정보
    public string unitName = "신병족";
    public int unitLV = 1;

    // 전투 스탯
    public float baseAttackPower = 30f;
    public float bonusAttackPower = 5f;
    public float armor = 0f;
    public float bonusArmor = 0f;
    public float attackSpeed = 1f;

    // 체력/쉴드
    public float currentHP = 100f;
    public float maxHP = 100f;
    public float currentShield = 50f;
    public float maxShield = 50f;

    // 크리티컬
    public float criticalChance = 10f;

    // 이속
    public float moveSpeed = 3f;

    //경험치
    public int currentEXP = 0;
    public int maxEXP = 100;
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ApplyEffect(Effect effect)
    {
        switch (effect.effectType)
        {
            case EffectType.Attack: bonusAttackPower += effect.effectValue; break;
            case EffectType.Armor: armor += effect.effectValue; break;
            case EffectType.MoveSpeed: moveSpeed += effect.effectValue; break;
            case EffectType.CriticalChance: criticalChance += effect.effectValue; break;
            case EffectType.AttackSpeed: attackSpeed += effect.effectValue; break;
        }
    }

    public void RemoveEffect(Effect effect)
    {
        switch (effect.effectType)
        {
            case EffectType.Attack: bonusAttackPower -= effect.effectValue; break;
            case EffectType.Armor: armor -= effect.effectValue; break;
            case EffectType.MoveSpeed: moveSpeed -= effect.effectValue; break;
            case EffectType.CriticalChance: criticalChance -= effect.effectValue; break;
            case EffectType.AttackSpeed: attackSpeed -= effect.effectValue; break;
        }
    }
}
