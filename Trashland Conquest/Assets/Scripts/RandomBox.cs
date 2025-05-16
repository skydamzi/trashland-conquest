using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomBox : MonoBehaviour
{
    public Item[] possibleItems; // 인스펙터에서 아이템들 넣기
    private bool playerInRange = false;
private void Start()
{
    Debug.Log("[RandomBox] Start 호출됨");
}
    void Update()
    {
        if (playerInRange)
            Debug.Log("플레이어 범위 안에 있음");

        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("E키 입력 감지됨!");
            GiveRandomItem();
            Destroy(gameObject);
        }
    }

    private void GiveRandomItem()
    {
        if (possibleItems.Length == 0) return;

        int rand = Random.Range(0, possibleItems.Length);
        Item selectedItem = possibleItems[rand];

        Inventory.instance.Add(selectedItem); // 인벤토리에 추가
        Debug.Log($"[RandomBox] {selectedItem.itemName} 획득!");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
         Debug.Log($"뭔가 들어옴: {other.name}");

         if (other.CompareTag("Player"))
         {
            Debug.Log("플레이어 범위 안에 들어옴");
            playerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = false;
    }
}