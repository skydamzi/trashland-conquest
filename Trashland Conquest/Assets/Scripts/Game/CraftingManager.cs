using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq; // LINQ (Where, ToList 등)를 사용하기 위해 필요

public class CraftingManager : MonoBehaviour
{
    [Header("===== 아이템 및 레시피 데이터 =====")]
    public List<Item> allItems = new List<Item>(); 
    public List<Recipe> allRecipes = new List<Recipe>(); // 이 레시피 리스트는 이제 고정된 조합법에만 사용

    [Header("===== 조합 슬롯 =====")]
    public List<string> currentCraftingMaterialIds = new List<string>(5); 



    void Awake()
    {
        ClearCraftingSlots();
        InitializeDebugData();

    }

    void InitializeDebugData()
    {
        // ===== 아이템 데이터 추가 =====
        allItems.Add(new Item { id = "001", itemName = "젖소", itemType = ItemType.Equip, description = "싱싱한 젖소 재료." });
        allItems.Add(new Item { id = "002", itemName = "초콜릿", itemType = ItemType.Equip, description = "달콤한 초콜릿 바 재료." });
        allItems.Add(new Item { id = "003", itemName = "꿀", itemType = ItemType.Equip, description = "끈적하고 달콤한 꿀 재료." });
        allItems.Add(new Item { id = "004", itemName = "오렌지", itemType = ItemType.Equip, description = "상큼한 오렌지 재료." });
        
        allItems.Add(new Item { id = "005", itemName = "초코우유", itemType = ItemType.Drink, description = "와! 달콤한 초코우유가 만들어졌다! 맛있겠다!" });
        allItems.Add(new Item { id = "006", itemName = "똥물", itemType = ItemType.Drink, description = "이런 젠장! 똥물이다! 절대 마시지 마세요..." });
        allItems.Add(new Item { id = "007", itemName = "평범한 물", itemType = ItemType.Drink, description = "흐음... 그냥 물이군. 아무 일도 일어나지 않았다." });

        // 테스트용 추가 재료 (5개 다른 재료 똥물 테스트용)
        allItems.Add(new Item { id = "008", itemName = "사과", itemType = ItemType.Equip, description = "싱싱한 사과." });
        allItems.Add(new Item { id = "009", itemName = "바나나", itemType = ItemType.Equip, description = "맛있는 바나나." });

        // NOTE: 여기서는 이제 고정된 레시피(젖소4+초코1 등)는 넣지 않을 거다.
        // 네가 원하는 "젖소+초코만 있으면 초코우유" 같은 조건은 CraftItem 함수 안에서 직접 처리할 거니까.
        // 만약 특정 고정 조합이 여전히 필요하다면 여기에 추가해도 된다.
        // 예: allRecipes.Add(new Recipe { materialIds = new List<string> { "001", "001", "003" }, resultItemId = "010" }); // 우유+우유+꿀 = 벌꿀우유 같은 거
        
        Debug.Log("디버그용 아이템 및 레시피 데이터 초기화 완료.");
    }

    public void AddMaterialToSlot(int index, string itemId)
    {
        if (index < 0 || index >= currentCraftingMaterialIds.Count)
        {
            Debug.LogError($"잘못된 슬롯 인덱스다, 야! {index}. 0~4 사이여야 한다.");
            return;
        }
        currentCraftingMaterialIds[index] = itemId; 
        Debug.Log($"슬롯 {index}에 '{itemId}' 추가됨.");
        
        //uiManager?.UpdateCraftingSlotsUI();
    }

    public void RemoveMaterialFromSlot(int index)
    {
        if (index < 0 || index >= currentCraftingMaterialIds.Count)
        {
            Debug.LogError($"잘못된 슬롯 인덱스다, 야! {index}. 0~4 사이여야 한다.");
            return;
        }
        currentCraftingMaterialIds[index] = null; 
        Debug.Log($"슬롯 {index} 비워짐.");

        //uiManager?.UpdateCraftingSlotsUI();
    }

    // "조합하기" 버튼을 누르면 호출될 핵심 함수 (핵심 수정 부분)
    [ContextMenu("테스트: [조합] 현재 슬롯 아이템으로 조합하기")] // << 이 줄 추가
    public Item CraftItem()
    {
        Debug.Log("\n--- 조합 시작 ---");

        List<string> materialsInSlots = currentCraftingMaterialIds
            .Where(id => !string.IsNullOrEmpty(id))
            .ToList();

        if (materialsInSlots.Count == 0)
        {
            Debug.LogWarning("조합할 재료가 하나도 없잖아, 야! 슬롯에 뭐라도 넣어봐.");
            //uiManager?.resultText.SetText("조합할 재료가 없다, 야!");
            return null;
        }

        Debug.Log($"입력된 재료: {string.Join(", ", materialsInSlots)}");

        Item resultItem = null; // 최종 결과 아이템

        // 1. "젖소 + 초콜릿" 조건 검사 (개수 무관, 포함 여부만 확인)
        bool hasCow = materialsInSlots.Contains("001"); // 젖소(ID "001")가 포함되어 있는가?
        bool hasChocolate = materialsInSlots.Contains("002"); // 초콜릿(ID "002")이 포함되어 있는가?

        if (hasCow && hasChocolate)
        {
            Debug.Log("젖소와 초콜릿이 발견되었다! 초코우유 조합이다!");
            resultItem = allItems.Find(item => item.id == "005"); // 초코우유 ID
        }
        // 이 외에 다른 1순위 조합 조건이 있다면 여기에 else if로 추가해라.
        // 예: else if (materialsInSlots.Contains("003") && materialsInSlots.Contains("004")) { // 꿀과 오렌지
        //     resultItem = allItems.Find(item => item.id == "011"); // 꿀오렌지 주스 같은 거
        // }


        // 2. 1번 조건에 해당하지 않고, 재료가 5개이며 모두 다른 재료일 경우 "똥물"
        // 이때는 똥물 조건을 모든 레시피보다 우선순위가 낮도록 배치해야 한다.
        // 만약 젖소+초코가 들어있어도 5개 다른 재료면 똥물을 만들고 싶다면, 이 조건을 위로 올려라.
        // 지금은 젖소+초코가 먼저 인식된다.
        if (resultItem == null && materialsInSlots.Count == 5 && materialsInSlots.Distinct().Count() == 5)
        {
            Debug.Log("일치하는 레시피는 없지만, 5개의 재료가 모두 다르다! 똥물이다, 야!");
            resultItem = allItems.Find(item => item.id == "006"); // 똥물 ID
        }


        // 3. 위에 어떤 조건에도 해당하지 않으면 "평범한 물" (기본 실패작)
        // 이건 가장 마지막에 오는 최종 실패 로직이다.
        if (resultItem == null)
        {
            Debug.Log("일치하는 특수 조합이 없다. 평범한 물이 나온다.");
            resultItem = allItems.Find(item => item.id == "007"); // 평범한 물 ID
        }

        // 최종 결과 처리
        if (resultItem != null)
        {
            Debug.Log($"최종 결과물: '{resultItem.itemName}'(을)를 만들었다!");
            Debug.Log($"설명: {resultItem.description}");

            ClearCraftingSlots();
            //uiManager?.UpdateCraftingSlotsUI(); // UI 슬롯 초기화
            // uiManager?.resultText.SetText(resultItem.description); // UI 결과 텍스트 업데이트
            return resultItem;
        }
        else
        {
            // 사실 이 else 블록은 위에 resultItem = null 일 때 최종 처리하는 로직 때문에 거의 실행될 일 없다.
            Debug.LogError("조합 결과 아이템을 찾을 수 없다! 데이터 오류다, 야!");
            ClearCraftingSlots();
            //uiManager?.UpdateCraftingSlotsUI();
            //uiManager?.resultText.SetText("조합 실패! 시스템 오류!");
            return null;
        }
    }

    public void ClearCraftingSlots()
    {
        for (int i = 0; i < currentCraftingMaterialIds.Count; i++)
        {
            currentCraftingMaterialIds[i] = null; 
        }
        Debug.Log("조합 슬롯 초기화 완료.");
       // uiManager?.UpdateCraftingSlotsUI();
    }

    // =======================================================
    // 유니티 인스펙터에서 편하게 테스트할 수 있는 Context Menu (선택 사항)
    // =======================================================
    [ContextMenu("테스트: 슬롯 0에 젖소 넣기")]
    void TestAddMaterial0Cow() { AddMaterialToSlot(0, "001"); }
    [ContextMenu("테스트: 슬롯 1에 초콜릿 넣기")]
    void TestAddMaterial1Choco() { AddMaterialToSlot(1, "002"); }
    [ContextMenu("테스트: 슬롯 2에 꿀 넣기")]
    void TestAddMaterial2Honey() { AddMaterialToSlot(2, "003"); }
    [ContextMenu("테스트: 슬롯 3에 오렌지 넣기")]
    void TestAddMaterial3Orange() { AddMaterialToSlot(3, "004"); }
    [ContextMenu("테스트: 슬롯 4에 사과 넣기")] // 새 재료 테스트용
    void TestAddMaterial4Apple() { AddMaterialToSlot(4, "008"); }
    [ContextMenu("테스트: 슬롯 0에 바나나 넣기")] // 새 재료 테스트용
    void TestAddMaterial0Banana() { AddMaterialToSlot(0, "009"); }


    [ContextMenu("테스트: [조합] 초코우유 (젖소1+초코1, 다른 슬롯)")]
    void TestCraftChocoMilkSimple()
    {
        ClearCraftingSlots();
        AddMaterialToSlot(0, "001"); // 젖소
        AddMaterialToSlot(3, "002"); // 초콜릿 (다른 슬롯)
        CraftItem();
    }
    
    [ContextMenu("테스트: [조합] 초코우유 (젖소2+초코1, 다른 슬롯)")]
    void TestCraftChocoMilkMoreCow()
    {
        ClearCraftingSlots();
        AddMaterialToSlot(0, "001"); // 젖소
        AddMaterialToSlot(1, "001"); // 젖소
        AddMaterialToSlot(3, "002"); // 초콜릿
        CraftItem();
    }

    [ContextMenu("테스트: [조합] 똥물 (5개 모두 다른 재료)")]
    void TestCraftPoopWaterDifferentMaterials()
    {
        ClearCraftingSlots();
        AddMaterialToSlot(0, "001"); // 젖소
        AddMaterialToSlot(1, "002"); // 초콜릿
        AddMaterialToSlot(2, "003"); // 꿀
        AddMaterialToSlot(3, "004"); // 오렌지
        AddMaterialToSlot(4, "008"); // 사과 (새로운 재료)
        CraftItem();
    }
    
    [ContextMenu("테스트: [조합] 없는 조합 (평범한 물)")]
    void TestCraftNoRecipe()
    {
        ClearCraftingSlots();
        AddMaterialToSlot(0, "001"); // 젖소
        AddMaterialToSlot(1, "003"); // 꿀
        AddMaterialToSlot(2, "003"); // 꿀
        CraftItem();
    }
    
    [ContextMenu("테스트: 모든 조합 슬롯 비우기")]
    void TestClearCraftingSlots()
    {
        ClearCraftingSlots();
        //uiManager?.resultText.SetText(""); // 결과 텍스트도 비움
    }
}