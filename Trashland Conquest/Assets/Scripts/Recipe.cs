using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq; // LINQ (OrderBy)를 사용하기 위해 필요

[System.Serializable] // << 이것도 인스펙터에서 볼 수 있게!
public class Recipe
{
    public string recipeId;             // 레시피 고유 ID (예: "REC_CHOCO_MILK_1")
    public List<string> materialIds;    // 이 레시피에 필요한 재료 아이템들의 ID 리스트
    public string resultItemId;         // 이 레시피로 만들어지는 결과 아이템의 ID

    // 재료 목록이 들어오면 이 레시피랑 일치하는지 확인하는 함수.
    // 순서 상관없이 재료가 맞으면 true를 반환한다.
    public bool Matches(List<string> inputMaterialIds)
    {
        // 1. 입력된 재료들을 오름차순으로 정렬한다. (예: "001", "002", "001" -> "001", "001", "002")
        var sortedInput = inputMaterialIds.OrderBy(id => id).ToList();
        // 2. 이 레시피에 정의된 재료들도 오름차순으로 정렬한다.
        var sortedRecipeMaterials = materialIds.OrderBy(id => id).ToList();

        // 3. 재료 개수가 다르면 무조건 일치하지 않음
        if (sortedInput.Count != sortedRecipeMaterials.Count)
        {
            return false;
        }

        // 4. 각 재료의 ID가 순서대로 모두 일치하는지 확인
        for (int i = 0; i < sortedInput.Count; i++)
        {
            if (sortedInput[i] != sortedRecipeMaterials[i])
            {
                return false; // 하나라도 다르면 불일치
            }
        }
        return true; // 모든 조건 만족하면 일치!
    }
}