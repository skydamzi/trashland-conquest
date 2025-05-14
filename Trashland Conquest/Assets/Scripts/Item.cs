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
    [Header("기본 정보")]
    public string itemName;
    public Sprite icon;
    public ItemType itemType;

    [Header("희귀도 설정")]
    public Rarity rarity = Rarity.Common;  // 기본값: Common

    [Header("효과 설정")]
    public List<Effect> effects = new List<Effect>();

    [Header("특성 시너지 설정")]
    public List<TraitEffect> traitEffects = new List<TraitEffect>();

    [Header("아이템 설명")]
    [TextArea]
    public string description;
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