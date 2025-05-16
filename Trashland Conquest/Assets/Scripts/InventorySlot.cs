using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    public Image icon;

    private Item item;
    private Button iconButton;

    public void SetItem(Item newItem)
{
    item = newItem;

    if (iconButton == null)
        iconButton = icon.GetComponent<Button>();

    if (item == null)
    {
        icon.sprite = null;
        icon.enabled = false;
        iconButton.onClick.RemoveAllListeners();
        return;
    }

    icon.sprite = item.icon;
    icon.enabled = true;

    iconButton.onClick.RemoveAllListeners();
    iconButton.onClick.AddListener(OnClickSlot);
}

    public void ClearSlot()
    {
        item = null;
        icon.sprite = null;
        icon.enabled = false;

        if (iconButton != null)
            iconButton.onClick.RemoveAllListeners();
    }

    private void OnClickSlot()
    {
        if (item != null)
        {
            // 중복 장착 방지
            if (Equipment.instance.equippedItems.Contains(item))
            {
                Debug.LogWarning($"[InventorySlot] {item.itemName} 이미 장착됨");
                return;
            }

            // 장착 가능한 슬롯 수 초과 방지
            if (Equipment.instance.equippedItems.Count >= Equipment.instance.maxEquipCount)
            {
                Debug.LogWarning("장착 슬롯 가득 참");
                return;
            }

            Equipment.instance.equippedItems.Add(item); // 장비 리스트에 추가
            Equipment.instance.onEquipmentChangedCallback?.Invoke(); // 이게 핵심
            Inventory.instance.Remove(item);             // 인벤토리에서 제거
        }
    }
}