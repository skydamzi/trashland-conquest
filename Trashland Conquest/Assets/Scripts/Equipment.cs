using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq; // LINQ가 필요할 수도 있으니 일단 남겨두자

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
            // Debug.LogWarning("씬에 Equipment가 이미 존재한다! 중복된 인스턴스를 파괴한다, 야!"); // 로그는 네 자유
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Awake 다음에 Start에서 초기 장비 아이템들의 효과를 적용한다.
        // 인스펙터에서 미리 할당된 아이템들도 처리될 수 있도록.
        // 이때, 리스트를 순회하면서 원본 리스트가 변경될 수 있으므로 ToList()로 복사해서 순회하는 게 안전하다.
        foreach (var item in equippedItems.ToList()) 
        {
            if (item != null)
            {
                ApplyEffects(item.effects);
                // ApplyTraitEffects(item.traitEffects); // 특성 효과가 있다면 이전에 말한대로 추가해라
                Debug.Log($"[Equipment] {item.itemName} (초기 장착) 효과 적용됨.");
            }
            else
            {
                // 만약 인스펙터에 null 값이 있다면 제거
                equippedItems.Remove(item); 
            }
        }
        // 초기 장비 상태 변경을 알린다. (시너지 매니저가 이 이벤트를 받아서 초기 시너지 체크)
        onEquipmentChangedCallback?.Invoke(); 
    }


    [Header("장착 슬롯")]
    public int maxEquipCount = 5;
    public List<Item> equippedItems = new List<Item>(); // 인스펙터 노출용이자 실제 장착된 아이템 리스트

    // private HashSet<Item> _equippedSet = new HashSet<Item>(); // 이거 통째로 삭제해라!
    public delegate void OnEquipmentChanged();
    public OnEquipmentChanged onEquipmentChangedCallback; // 장비 변경 시 호출될 이벤트


    // =======================================================================
    // Update 메서드는 통째로 삭제한다. 인스펙터에서 직접 수정하는 방식은 지양해라!
    // 모든 장비 장착/해제는 아래의 TryEquipItem/TryUnequipItem 함수를 통해 이루어져야 함.
    // =======================================================================
    // void Update()
    // {
    //     // 이전에 설명한 이유로 이 로직은 문제가 많다. 삭제!
    // }


    // =======================================================================
    // 이제 이 두 함수(Equip, Unequip)는 내부용이 아니라, 외부에 노출되는 핵심 장착/해제 함수다.
    // 이름을 TryEquipItem, TryUnequipItem으로 바꾸고 bool 반환을 통해 성공 여부를 알리자.
    // =======================================================================

    /// <summary>
    /// 아이템을 장착하려고 시도하는 외부 호출용 메서드
    /// </summary>
    /// <param name="itemToEquip">장착할 아이템</param>
    /// <returns>장착 성공 시 true, 실패 시 false</returns>
    public bool TryEquipItem(Item itemToEquip)
    {
        if (itemToEquip == null)
        {
            Debug.LogWarning("[Equipment] 장착하려는 아이템이 null이다, 야!");
            return false;
        }

        // 아이템 타입 검사 (네 기획에 따라 'Equip'이 아닌 다른 타입도 허용 가능)
        // 예를 들어 초코, 우유 등은 Material 또는 Drink 타입일 수 있으니, 네 Enum에 맞춰서 조정해라.
        // 모든 장착 가능한 아이템 타입을 여기에 명시해야 한다.
        if (itemToEquip.itemType != ItemType.Equip && 
            itemToEquip.itemType != ItemType.Drink) // 예시: Material과 Drink도 장착 가능하다고 가정
        {
            Debug.LogWarning($"[Equipment] '{itemToEquip.itemName}'은(는) 장비할 수 있는 아이템 타입이 아니다! (타입: {itemToEquip.itemType})");
            return false;
        }

        // maxEquipCount를 초과하면 장착 불가
        if (equippedItems.Count >= maxEquipCount)
        {
            Debug.LogWarning($"[Equipment] 장비 슬롯이 꽉 찼다! ({equippedItems.Count}/{maxEquipCount}) '{itemToEquip.itemName}' 장착 실패!");
            return false;
        }

        // --- 실제 장착 로직 ---
        equippedItems.Add(itemToEquip); // 리스트에 추가 (중복 허용)
        
        ApplyEffects(itemToEquip.effects); // 아이템 개별 효과 적용
        // ApplyTraitEffects(itemToEquip.traitEffects); // 특성 효과가 있다면 적용

        Debug.Log($"[Equipment] '{itemToEquip.itemName}'(을)를 장착했다! 현재 장비 개수: {equippedItems.Count}");
        
        onEquipmentChangedCallback?.Invoke(); // 장비 변경 이벤트 호출 (시너지 매니저에게 알림)
        return true; 
    }

    /// <summary>
    /// 아이템을 해제하려고 시도하는 외부 호출용 메서드
    /// </summary>
    /// <param name="itemToUnequip">해제할 아이템 인스턴스</param>
    /// <returns>해제 성공 시 true, 실패 시 false</returns>
    public bool TryUnequipItem(Item itemToUnequip)
    {
        if (itemToUnequip == null)
        {
            Debug.LogWarning("[Equipment] 해제하려는 아이템이 null이다, 야!");
            return false;
        }

        // 리스트에서 해당 아이템을 찾아서 제거 (중복된 아이템 중 첫 번째 발견된 것만 제거)
        // 만약 특정 인스턴스만 정확히 제거해야 한다면, 리스트의 FindIndex 후 RemoveAt을 써야 할 수도 있다.
        // 하지만 지금처럼 Item SO 자체를 넘긴다면 Contains와 Remove로 충분.
        if (!equippedItems.Contains(itemToUnequip))
        {
            Debug.LogWarning($"[Equipment] '{itemToUnequip.itemName}'은(는) 장착 중이 아니다, 야!");
            return false;
        }

        // --- 실제 해제 로직 ---
        equippedItems.Remove(itemToUnequip); // 리스트에서 제거 (중복된 경우 첫 번째 것만)

        RemoveEffects(itemToUnequip.effects); // 아이템 개별 효과 제거
        // RemoveTraitEffects(itemToUnequip.traitEffects); // 특성 효과가 있다면 제거

        Debug.Log($"[Equipment] '{itemToUnequip.itemName}'(을)를 해제했다! 현재 장비 개수: {equippedItems.Count}");

        onEquipmentChangedCallback?.Invoke(); // 장비 변경 이벤트 호출 (시너지 매니저에게 알림)
        return true; 
    }

    /// <summary>
    /// 모든 장비를 해제하는 메서드
    /// </summary>
    public void UnequipAll()
    {
        // equippedItems 리스트를 역순으로 순회하며 해제
        // for 문을 역순으로 돌려야 리스트에서 제거해도 인덱스 문제가 발생하지 않는다.
        for (int i = equippedItems.Count - 1; i >= 0; i--)
        {
            TryUnequipItem(equippedItems[i]); // 개별 해제 로직 호출
        }
        // TryUnequipItem 내부에서 이미 리스트에서 제거하고 효과를 해제하므로,
        // 아래의 Clear()는 사실상 불필요하지만, 혹시 모를 상황에 대비해 명시적으로 남겨둘 수도 있다.
        // equippedItems.Clear(); 
        
        Debug.Log("모든 장비 해제 완료!");
        onEquipmentChangedCallback?.Invoke(); // 최종 장비 변경 이벤트 호출
    }

    /// <summary>
    /// 현재 장착된 아이템 리스트를 외부에서 가져갈 때 사용 (읽기 전용)
    /// </summary>
    public List<Item> GetEquippedItems()
    {
        return new List<Item>(equippedItems); // 원본 리스트가 수정되지 않도록 복사해서 반환
    }


    // =======================================================
    // 아이템 효과 적용/제거 함수들 (이 부분은 이전과 동일)
    // =======================================================
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

    // TraitEffects 관련 함수들은 Item 스크립트와 시너지 스크립트에 TraitEffect가 있다면 추가해라.
    // private void ApplyTraitEffects(List<TraitEffect> traitEffects) { /* ... */ }
    // private void RemoveTraitEffects(List<TraitEffect> traitEffects) { /* ... */ }
}