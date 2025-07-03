using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatus : MonoBehaviour
{
    public static PlayerStatus instance;

    public string unitName = "신병이";
    public int unitLV = 1;

    public float baseAttackPower = 30f;
    public float bonusAttackPower = 5f;
    public float armor = 0f;
    public float bonusArmor = 0f;
    public float attackSpeed = 1f;

    public float currentHP = 100f;
    public float maxHP = 100f;
    public float maxStamina = 100f;       // 최대 스태미나
    public float currentStamina = 100f;    // 현재 스태미나
    public int currentShield = 5;
    public int maxShield = 5;

    public float criticalChance = 10f;

    public float moveSpeed = 3f;

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
