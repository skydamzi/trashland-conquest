using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// List를 사용할 경우 필요 (현재 Item SO에 effects 등이 List이므로 필요)

public class TestEquipmentInteraction : MonoBehaviour
{
    // 인스펙터에서 드래그해서 할당할 아이템 ScriptableObject 변수들
    [Header("테스트용 아이템 할당")]
    public Item chocolateItem; // 여기에 초코 Item SO를 드래그 앤 드롭
    public Item milkItem;      // 여기에 우유 Item SO를 드래그 앤 드롭
    public Item orangeItem;    // 여기에 오렌지 Item SO를 드래그 앤 드롭
    public Item sugarItem;     // 여기에 설탕 Item SO를 드래그 앤 드롭

    void Update()
    {
        // ----------------------------------------------------
        // 단일 아이템 장착 테스트
        // ----------------------------------------------------
        if (Input.GetKeyDown(KeyCode.Alpha1)) // '1'번 키 누르면 초코 장착
        {
            if (chocolateItem != null)
            {
                // Equipment 싱글톤 인스턴스에 접근하여 TryEquipItem 호출
                bool success = Equipment.instance.TryEquipItem(chocolateItem);
                Debug.Log($"초코 장착 시도: {success}");
            }
            else
            {
                Debug.LogWarning("인스펙터에 초코 아이템 SO 할당 안 됨.");
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha2)) // '2'번 키 누르면 우유 장착
        {
            if (milkItem != null)
            {
                bool success = Equipment.instance.TryEquipItem(milkItem);
                Debug.Log($"우유 장착 시도: {success}");
            }
            else
            {
                Debug.LogWarning("인스펙터에 우유 아이템 SO 할당 안 됨.");
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha3)) // '3'번 키 누르면 오렌지 장착
        {
            if (orangeItem != null)
            {
                bool success = Equipment.instance.TryEquipItem(orangeItem);
                Debug.Log($"오렌지 장착 시도: {success}");
            }
            else
            {
                Debug.LogWarning("인스펙터에 오렌지 아이템 SO 할당 안 됨.");
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha4)) // '4'번 키 누르면 설탕 장착
        {
            if (sugarItem != null)
            {
                bool success = Equipment.instance.TryEquipItem(sugarItem);
                Debug.Log($"설탕 장착 시도: {success}");
            }
            else
            {
                Debug.LogWarning("인스펙터에 설탕 아이템 SO 할당 안 됨.");
            }
        }
        
        // ----------------------------------------------------
        // 단일 아이템 해제 테스트
        // ----------------------------------------------------
        if (Input.GetKeyDown(KeyCode.Alpha7)) // '7' 키 누르면 초코 해제 (장착되어 있는 경우 첫 번째 초코 해제)
        {
            if (chocolateItem != null)
            {
                bool success = Equipment.instance.TryUnequipItem(chocolateItem);
                Debug.Log($"초코 해제 시도: {success}");
            }
            else
            {
                Debug.LogWarning("인스펙터에 초코 아이템 SO 할당 안 됨.");
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha8)) // '8' 키 누르면 우유 해제
        {
            if (milkItem != null)
            {
                bool success = Equipment.instance.TryUnequipItem(milkItem);
                Debug.Log($"우유 해제 시도: {success}");
            }
            else
            {
                Debug.LogWarning("인스펙터에 우유 아이템 SO 할당 안 됨.");
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha9)) // '9' 키 누르면 오렌지 해제
        {
            if (orangeItem != null)
            {
                bool success = Equipment.instance.TryUnequipItem(orangeItem);
                Debug.Log($"오렌지 해제 시도: {success}");
            }
            else
            {
                Debug.LogWarning("인스펙터에 오렌지 아이템 SO 할당 안 됨.");
            }
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha0)) // '0' 키 누르면 설탕 해제
        {
            if (sugarItem != null)
            {
                bool success = Equipment.instance.TryUnequipItem(sugarItem);
                Debug.Log($"설탕 해제 시도: {success}");
            }
            else
            {
                Debug.LogWarning("인스펙터에 설탕 아이템 SO 할당 안 됨.");
            }
        }
        // ----------------------------------------------------
        // 모든 장비 해제 테스트
        // ----------------------------------------------------
        if (Input.GetKeyDown(KeyCode.Space)) // 스페이스바 누르면 모든 장비 해제
        {
            Equipment.instance.UnequipAll();
            Debug.Log("모든 장비 해제 명령 보냄.");
        }

        // ----------------------------------------------------
        // 현재 장착된 아이템 목록 확인 (선택 사항)
        // ----------------------------------------------------
        if (Input.GetKeyDown(KeyCode.Tab)) // 탭 키 누르면 현재 장비 목록 출력
        {
            List<Item> currentEquips = Equipment.instance.GetEquippedItems();
            string equipList = "현재 장착 아이템: ";
            if (currentEquips.Count == 0)
            {
                equipList += "없음";
            }
            else
            {
                foreach (var item in currentEquips)
                {
                    equipList += $"{item.itemName}, ";
                }
            }
            Debug.Log(equipList);
        }
    }

    // 초기 시너지 테스트 (버튼으로도 가능)
    [ContextMenu("테스트: 초코우유 조합 장착 (초코 1, 우유 1)")]
    void TestChocolateMilkCombo()
    {
        // 먼저 모든 장비 해제하고 시작하는 게 깔끔하다.
        Equipment.instance.UnequipAll(); 
        
        // 필요한 아이템들을 순서대로 장착 시도
        if (chocolateItem != null) Equipment.instance.TryEquipItem(chocolateItem);
        if (milkItem != null) Equipment.instance.TryEquipItem(milkItem);

        Debug.Log("초코우유 조합 장착 시도 완료.");
    }

    [ContextMenu("테스트: 똥물 조합 장착 (초코, 우유, 오렌지, 설탕 등 5개)")]
    void TestPoopWaterCombo()
    {
        Equipment.instance.UnequipAll();
        
        if (chocolateItem != null) Equipment.instance.TryEquipItem(chocolateItem);
        if (milkItem != null) Equipment.instance.TryEquipItem(milkItem);
        if (orangeItem != null) Equipment.instance.TryEquipItem(orangeItem);
        if (sugarItem != null) Equipment.instance.TryEquipItem(sugarItem);
        
        // 여기에 테스트에 필요한 다른 아이템들을 더 추가해서 5개 이상 만들어봐라.
        // 예를 들어, Item ID로 찾아서 추가하는 방식도 좋다.
        // if (CombinationSynergyManager.Instance != null)
        // {
        //     Equipment.instance.TryEquipItem(CombinationSynergyManager.Instance.allItems.Find(i => i.id == "00X"));
        // }

        Debug.Log("똥물 조합 장착 시도 완료.");
    }
}