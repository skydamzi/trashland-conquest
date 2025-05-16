using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType
{
    Consumable,
    Equip
}

public enum EffectType
{
    Attack,
    Armor,
    MoveSpeed,
    CriticalChance,
    AttackSpeed
}

public enum Rarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}

[CreateAssetMenu(fileName = "New Item", menuName = "Item/Create New Item")]
public class Item : ScriptableObject
{
    [Header("정보")]
    public string itemName;
    public Sprite icon;
    public ItemType itemType;

    [Header("희귀도")]
    public Rarity rarity = Rarity.Common;  // �⺻��: Common

    [Header("기본 효과")]
    public List<Effect> effects = new List<Effect>();

    [Header("특성 효과")]
    public List<TraitEffect> traitEffects = new List<TraitEffect>();

    [Header("설명")]
    [TextArea]
    public string description;

    // 드롭될 실제 프리팹
    public GameObject dropPrefab;
}

[System.Serializable]
public class Effect
{
    public EffectType effectType;
    public float effectValue;
}

[System.Serializable]
public class TraitEffect
{
    public TraitSynergy.TraitType traitType;
    public int stackAmount;
}