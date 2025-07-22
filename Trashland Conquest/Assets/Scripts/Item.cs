using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType
{
    Consumable,
    Equip, // 장비 아이템
    Drink,    // 조합으로 만들어지는 음료 아이템
    Other     // 기타 (쓰기 나름)
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
    public string id;           // 아이템 고유 ID (예: "001_cow", "002_chocolate")
    public string itemName;     // 아이템 이름 (예: "젖소", "초콜릿")
    public string description;  // 아이템 설명 (조합 성공 메시지에 쓸 거다)

    public Sprite icon;
    public ItemType itemType;

    [Header("희귀도")]
    public Rarity rarity = Rarity.Common;

    [Header("기본 효과")]
    public List<Effect> effects = new List<Effect>();

    [Header("특성 효과")]
    public List<TraitEffect> traitEffects = new List<TraitEffect>();

    [Header("설명")]

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