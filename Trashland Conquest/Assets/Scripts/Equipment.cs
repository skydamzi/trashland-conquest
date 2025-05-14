using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Equipment : MonoBehaviour
{
    public static Equipment instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    [Header("장착 슬롯 (인스펙터에서 수정 시 자동 적용)")]
    public int maxEquipCount = 5;
    public List<Item> equippedItems = new List<Item>();  // 인스펙터 노출용

    private HashSet<Item> _equippedSet = new HashSet<Item>();  // 내부 추적용

    void Update()
    {
        // 1. 새로 들어온 아이템 감지 → 장착
        foreach (var item in equippedItems)
        {
            if (item == null) continue;

            if (!_equippedSet.Contains(item))
            {
                if (_equippedSet.Count >= maxEquipCount)
                {
                    Debug.LogWarning($"장비 슬롯 초과: {item.itemName} 장착 무시됨");
                    continue;
                }

                Equip(item);
                _equippedSet.Add(item);
                Debug.Log($"[Equipment] {item.itemName} 장착됨 (인스펙터 감지)");
            }
        }

        // 2. 제거된 아이템 감지 → 해제
        var toRemove = new List<Item>();
        foreach (var item in _equippedSet)
        {
            if (!equippedItems.Contains(item))
            {
                Unequip(item);
                toRemove.Add(item);
                Debug.Log($"[Equipment] {item.itemName} 해제됨 (인스펙터 감지)");
            }
        }
        foreach (var item in toRemove)
        {
            _equippedSet.Remove(item);
        }
    }

    // 내부용 실제 장착 처리
    public void Equip(Item item)
    {
        if (item.itemType != ItemType.Equip)
        {
            Debug.LogWarning("이 아이템은 장비할 수 없습니다.");
            return;
        }

        ApplyEffects(item.effects);
        ApplyTraitEffects(item.traitEffects);
    }

    // 내부용 실제 해제 처리
    public void Unequip(Item item)
    {
        RemoveEffects(item.effects);
        RemoveTraitEffects(item.traitEffects);
    }

    private void ApplyEffects(List<Effect> effects)
    {
        foreach (var effect in effects)
        {
            PlayerStatus.instance?.ApplyEffect(effect);
        }
    }

    private void RemoveEffects(List<Effect> effects)
    {
        foreach (var effect in effects)
        {
            PlayerStatus.instance?.RemoveEffect(effect);
        }
    }

    private void ApplyTraitEffects(List<TraitEffect> traitEffects)
    {
        foreach (var trait in traitEffects)
        {
            TraitSynergy.Instance?.AddTrait(trait.traitType, trait.stackAmount);
        }
    }

    private void RemoveTraitEffects(List<TraitEffect> traitEffects)
    {
        foreach (var trait in traitEffects)
        {
            TraitSynergy.Instance?.RemoveTrait(trait.traitType, trait.stackAmount);
        }
    }

    public void UnequipAll()
    {
        foreach (var item in _equippedSet)
        {
            Unequip(item);
        }
        _equippedSet.Clear();
        equippedItems.Clear();
        Debug.Log("모든 장비 해제 완료");
    }
}
