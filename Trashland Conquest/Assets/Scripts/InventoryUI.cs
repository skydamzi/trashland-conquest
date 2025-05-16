using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public Transform slotGrid; // 인벤토리 영역
    public GameObject slotPrefab; // 슬롯 프리팹
    public Transform equipSlotGrid; // 장착창 영역

    private List<GameObject> slots = new List<GameObject>(); // 인벤토리 슬롯
    private List<GameObject> equipSlotObjects = new List<GameObject>(); // 장비 슬롯 오브젝트 저장

    void Start()
    {
        Inventory.instance.onInventoryChangedCallback += UpdateUI;
        Equipment.instance.onEquipmentChangedCallback += UpdateUI;
        UpdateUI();
    }

    public void UpdateUI()
    {
        // 기존 인벤토리 슬롯 정리
        foreach (GameObject slot in slots)
        {
            Destroy(slot);
        }
        slots.Clear();

        // 인벤토리 아이템 표시
        List<Item> items = Inventory.instance.items;
        foreach (Item item in items)
        {
            GameObject newSlot = Instantiate(slotPrefab, slotGrid);
            InventorySlot slotScript = newSlot.GetComponent<InventorySlot>();
            slotScript.SetItem(item);
            slots.Add(newSlot);
        }

        // 기존 장비 슬롯 정리
        foreach (GameObject slot in equipSlotObjects)
        {
            Destroy(slot);
        }
        equipSlotObjects.Clear();

        // 장비 아이템 표시 (자동 생성)
        List<Item> equipped = Equipment.instance.equippedItems;
        foreach (Item item in equipped)
        {
            GameObject newEquipSlot = Instantiate(slotPrefab, equipSlotGrid);
            InventorySlot slotScript = newEquipSlot.GetComponent<InventorySlot>();
            slotScript.SetItem(item);
            equipSlotObjects.Add(newEquipSlot);
        }
    }
}
