// SynergyCombination.cs (Simplifed and Cleaned Up!)
using UnityEngine;
using System.Collections.Generic;
using System.Linq; // ToList() 등 LINQ 사용을 위해 필요

[CreateAssetMenu(fileName = "NewSynergyCombination", menuName = "Synergy/Combination")]
public class SynergyCombination : ScriptableObject
{
    public string combinationName; // 조합 이름 (예: "초코우유", "오렌지 주스", "똥물")
    [TextArea] public string description; // 조합 설명

    [Header("필수 조합 태그")]
    // 이 시너지를 발동하기 위해 필요한 아이템 태그와 각 태그의 최소 개수.
    // 똥물 시너지는 이 리스트가 비어있을 것이다.
    public List<RequiredTag> requiredTags; 

    [Header("조합 효과")]
    public List<Effect> synergyEffects; // 이 조합이 발동했을 때 적용될 효과
    public List<TraitEffect> synergyTraitEffects; // 이 조합이 발동했을 때 적용될 특성 효과

    [Header("조합 결과 아이템 (선택 사항)")]
    public Item resultItem; 
    public bool destroyIngredientsOnCombine = true;

    // =========================================================
    // 여기에 disablesCombinations 필드를 다시 추가한다, 야!
    // =========================================================
    [Header("시너지 관계")] // 이 헤더도 다시 넣어라.
    // 이 시너지가 발동되면, 다른 어떤 시너지들을 비활성화해야 하는가?
    public List<SynergyCombination> disablesCombinations; // <-- 이 필드를 다시 추가해라!

    // CanActivate 메서드: 현재 가진 태그로 이 시너지가 발동 가능한지 체크하고, 발동 가능하다면 태그를 소모한다.
    public bool CanActivate(Dictionary<string, int> currentAvailableTags, bool consumeTags = true)
    {
        // 시너지 발동에 필요한 태그가 있는지 확인 (필수 태그 개수만 체크)
        foreach (var required in requiredTags)
        {
            if (!currentAvailableTags.ContainsKey(required.tag) || currentAvailableTags[required.tag] < required.count)
            {
                return false; // 필수 태그가 없거나 개수가 부족하면 발동 불가
            }
        }
        
        // 발동 가능하다면, consumeTags가 true일 때만 태그를 소모한다.
        if (consumeTags)
        {
            foreach (var required in requiredTags)
            {
                currentAvailableTags[required.tag] -= required.count; 
            }
        }
        return true; 
    }
}

// RequiredTag, Effect, TraitEffect, Item, ItemType 클래스 정의는 별도 파일에 잘 있는지 확인해라.
// 이전에 빨간 줄 떴던 부분들 해결했어야 한다.
// RequiredTag.cs, Effect.cs, TraitEffect.cs, Item.cs 파일들이 모두 프로젝트에 있고
// 필요한 using 지시문(System.Collections.Generic, UnityEngine, System 등)이 잘 되어 있는지 확인.

[System.Serializable]
public class RequiredTag
{
    public string tag;
    public int count;
}