// CombinationSynergyManager.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq; 
using UnityEngine.UI; // 레거시 Text를 사용하는 경우
// using TMPro; // TextMeshPro를 사용하는 경우

public class CombinationSynergyManager : MonoBehaviour
{
    public static CombinationSynergyManager Instance { get; private set; }

    [Header("모든 조합 정의 (ScriptableObject)")]
    [Tooltip("모든 SynergyCombination ScriptableObject를 여기에 할당하세요. (특수 시너지는 제외)")]
    public List<SynergyCombination> allCombinations; 

    [Header("모든 아이템 정의 (ScriptableObject)")]
    [Tooltip("테스트용 ContextMenu 및 기타 시스템에서 Item ScriptableObject를 ID로 찾기 위해 사용됩니다.")]
    public List<Item> allItems; 
    
    // UI 텍스트 연결을 위한 필드 추가 (이전 대화에서 UI 연결 문제 해결용으로 추가된 부분)
    [Header("UI 설정")]
    [Tooltip("현재 시너지 이름을 표시할 UI Text 요소")]
    public Text currentSynergyNameText; // 레거시 Text용

    private HashSet<SynergyCombination> _activeSynergies = new HashSet<SynergyCombination>(); 

    // '똥물' 시너지를 특별하게 관리하기 위한 필드 (이전 요청으로 제거되었으나, 
    // 만약 다시 '똥물' 같은 예외 처리가 필요하다면 이 필드를 되살리고
    // Start(), CheckAndManageSynergies() 내의 관련 로직을 복구해야 합니다.)
    // private SynergyCombination _poopWaterSynergy; 

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // '똥물' 시너지 관련 로직은 이전 요청에 따라 제거된 상태입니다.
        // 만약 다시 '똥물' 처리가 필요하다면 여기에 _poopWaterSynergy 초기화 로직을 다시 추가하세요.

        // 시너지 목록을 복잡도(요구 태그 수의 합) 내림차순으로 정렬
        allCombinations = allCombinations
            .OrderByDescending(c => c.requiredTags.Sum(r => r.count)) 
            .ToList(); 

        if (Equipment.instance != null)
        {
            Equipment.instance.onEquipmentChangedCallback += CheckAndManageSynergies; 
            Debug.Log("[SynergyManager] Equipment 변경 이벤트 구독 완료.");
            
            CheckAndManageSynergies(); // 초기 장비 상태에 따라 시너지 체크
        }
        else
        {
            Debug.LogError("[SynergyManager] Equipment 인스턴스를 찾을 수 없습니다! 시너지 체크 불가! 씬에 Equipment 오브젝트가 있고 스크립트가 붙어있는지 확인하세요.");
        }
    }

    private void OnDisable()
    {
        if (Equipment.instance != null)
        {
            Equipment.instance.onEquipmentChangedCallback -= CheckAndManageSynergies; 
            Debug.Log("[SynergyManager] Equipment 변경 이벤트 구독 해제 완료.");
        }
    }

    public void CheckAndManageSynergies()
    {
        Debug.Log("[SynergyManager] 장비 변경 감지! 시너지 관리 시작.");
        List<Item> currentEquippedItems = Equipment.instance.GetEquippedItems(); 

        // 장착 아이템이 1개 이하일 경우 모든 시너지 비활성화
        if (currentEquippedItems.Count <= 1)
        {
            DeactivateAllSynergies(); 
            Debug.Log("[SynergyManager] 장착 아이템 1개 이하, 시너지 없음.");
            UpdateSynergyDisplayName(null); // UI 업데이트
            return; 
        }

        // 현재 장착된 모든 아이템의 태그를 집계
        Dictionary<string, int> baseItemTags = new Dictionary<string, int>();
        foreach (var item in currentEquippedItems)
        {
            if (item == null) continue; 
            foreach (var tag in item.combinationTags)
            {
                if (baseItemTags.ContainsKey(tag))
                {
                    baseItemTags[tag]++;
                }
                else
                {
                    baseItemTags.Add(tag, 1);
                }
            }
        }
        
        // --- 시너지 선택 로직: 현재 활성화된 시너지 유지 우선 & 복잡도 기반 선택 ---
        SynergyCombination bestCandidateSynergy = null;
        SynergyCombination currentActiveNormalSynergy = _activeSynergies.FirstOrDefault(); // 현재 활성화된 시너지 (만약 있다면)

        // 1. 현재 가능한 시너지들 중 가장 높은 복잡도(요구 태그 개수)를 찾고, '정확한 매칭' 조건도 함께 확인
        int maxPossibleComplexity = 0;
        foreach (var combination in allCombinations)
        {
            Dictionary<string, int> tempAvailableTagsForCheck = new Dictionary<string, int>(baseItemTags);
            // CanActivate에 true를 전달하여 태그 소모를 시뮬레이션
            if (combination.CanActivate(tempAvailableTagsForCheck, true)) 
            {
                // 여기에서 '정확한 매칭' 조건을 추가합니다.
                // 시너지가 요구하는 태그를 모두 사용한 후에도 baseItemTags에 남는 태그가 있다면 이 시너지는 '정확한 매칭'이 아님
                bool hasRemainingTags = tempAvailableTagsForCheck.Any(kvp => kvp.Value > 0);

                if (!hasRemainingTags) // 남는 태그가 없어야만 유효한 후보로 간주
                {
                    int comboComplexity = combination.requiredTags.Sum(r => r.count);
                    if (comboComplexity > maxPossibleComplexity)
                    {
                        maxPossibleComplexity = comboComplexity;
                    }
                }
            }
        }

        // 2. 만약 현재 활성화된 시너지가 있고, 그 시너지가 여전히 유효하며, 
        //    최고 복잡도를 만족하고 '정확한 매칭'이라면 그 시너지를 유지합니다.
        if (currentActiveNormalSynergy != null)
        {
            Dictionary<string, int> tempAvailableTagsForCurrent = new Dictionary<string, int>(baseItemTags);
            if (currentActiveNormalSynergy.CanActivate(tempAvailableTagsForCurrent, true))
            {
                // 현재 시너지도 '정확한 매칭'이어야만 유지
                bool currentHasRemainingTags = tempAvailableTagsForCurrent.Any(kvp => kvp.Value > 0);

                if (!currentHasRemainingTags) // 남는 태그가 없어야 유지
                {
                    int currentSynergyComplexity = currentActiveNormalSynergy.requiredTags.Sum(r => r.count);
                    if (currentSynergyComplexity == maxPossibleComplexity) // 최고 복잡도와 같다면 유지
                    {
                        bestCandidateSynergy = currentActiveNormalSynergy;
                        Debug.Log($"<color=blue>[SynergyManager] 현재 시너지 '{bestCandidateSynergy.combinationName}' 유지 (최고 우선순위 일치 및 정확한 매칭).</color>");
                    }
                }
            }
        }

        // 3. 현재 활성화된 시너지를 유지하지 않거나, 없거나, 최고 우선순위가 아니라면,
        //    새로운 최고 우선순위 시너지를 '정확한 매칭' 조건으로 찾습니다.
        if (bestCandidateSynergy == null)
        {
            foreach (var combination in allCombinations) // 이미 복잡도 내림차순 정렬되어 있음
            {
                int comboComplexity = combination.requiredTags.Sum(r => r.count);
                if (comboComplexity == maxPossibleComplexity) // 최고 복잡도에 해당하는 시너지 중
                {
                    Dictionary<string, int> tempAvailableTagsForCheck = new Dictionary<string, int>(baseItemTags);
                    if (combination.CanActivate(tempAvailableTagsForCheck, true))
                    {
                        // 새로운 후보도 '정확한 매칭'이어야만 선택
                        bool candidateHasRemainingTags = tempAvailableTagsForCheck.Any(kvp => kvp.Value > 0);

                        if (!candidateHasRemainingTags) // 남는 태그가 없어야 선택
                        {
                            bestCandidateSynergy = combination; 
                            Debug.Log($"<color=blue>[SynergyManager] 새로운 최고 우선순위 시너지 발견: '{bestCandidateSynergy.combinationName}' (정확한 매칭).</color>");
                            break; // 찾았으니 루프 종료
                        }
                    }
                }
            }
        }
        // --- 시너지 선택 로직 변경 끝 ---

        DeactivateAllSynergies(); // 기존 시너지 모두 비활성화

        if (bestCandidateSynergy != null)
        {
            ActivateSynergy(bestCandidateSynergy); // 새로운(또는 유지된) 시너지 활성화
            _activeSynergies.Add(bestCandidateSynergy); 
            UpdateSynergyDisplayName(bestCandidateSynergy.combinationName); // UI 업데이트
        }
        // 만약 '똥물' 시너지를 다시 추가한다면 이 else if 블록을 되살리세요.
        // else if (_poopWaterSynergy != null && currentEquippedItems.Count >= 2)
        // {
        //     ActivateSynergy(_poopWaterSynergy);
        //     _activeSynergies.Add(_poopWaterSynergy);
        //     UpdateSynergyDisplayName(_poopWaterSynergy.combinationName);
        // }
        else // 어떤 '정상' 시너지도 발동되지 않았을 때 (e.g., 장착 아이템이 2개 이상이지만, 어떤 시너지도 조건을 만족하지 못할 때)
        {
            UpdateSynergyDisplayName(null); // UI 업데이트
        }
        
        Debug.Log("[SynergyManager] 시너지 관리 완료. 현재 활성화된 시너지 개수: " + _activeSynergies.Count);
        foreach (var activeSyn in _activeSynergies)
        {
            Debug.Log($"<color=purple>[SynergyManager] 최종 활성화된 시너지: '{activeSyn.combinationName}'</color>");
        }
    }

    private void ActivateSynergy(SynergyCombination synergy)
    {
        ApplySynergyEffects(synergy.synergyEffects);
        ApplySynergyTraitEffects(synergy.synergyTraitEffects);
        Debug.Log($"<color=lime>[SynergyManager] 시너지 발동! '{synergy.combinationName}' 효과 적용됨!</color>");
        
        // 이 시너지가 다른 시너지를 비활성화해야 하는 경우 처리
        if (synergy.disablesCombinations != null && synergy.disablesCombinations.Any())
        {
            foreach (var disabledSynergy in synergy.disablesCombinations)
            {
                if (_activeSynergies.Contains(disabledSynergy)) 
                {
                    DeactivateSynergy(disabledSynergy); // 이미 활성화된 시너지 중 비활성화 대상이 있으면 해제
                }
            }
        }
    }

    private void DeactivateSynergy(SynergyCombination synergy)
    {
        RemoveSynergyEffects(synergy.synergyEffects);
        RemoveSynergyTraitEffects(synergy.synergyTraitEffects);
        _activeSynergies.Remove(synergy); // 활성화 목록에서 제거

        Debug.Log($"<color=red>[SynergyManager] 시너지 해제! '{synergy.combinationName}' 효과 제거됨.</color>");
    }

    private void DeactivateAllSynergies()
    {
        if (_activeSynergies.Count == 0) return; 

        // _activeSynergies 컬렉션을 수정하는 중에 순회하지 않도록 ToList() 사용
        foreach (var activeSyn in _activeSynergies.ToList()) 
        {
            RemoveSynergyEffects(activeSyn.synergyEffects);
            RemoveSynergyTraitEffects(activeSyn.synergyTraitEffects);
            // Debug.Log($"<color=red>[SynergyManager] 기존 시너지 해제 중: '{activeSyn.combinationName}' 효과 제거됨.</color>"); // 이 로그는 DeactivateSynergy에서 이미 출력됨
        }
        _activeSynergies.Clear(); 
        Debug.Log("[SynergyManager] 모든 기존 시너지 비활성화 완료.");
    }

    // =======================================================
    // 시너지 효과 적용/제거 (PlayerStatus, TraitSynergy에 위임)
    // =======================================================
    private void ApplySynergyEffects(List<Effect> effects)
    {
        foreach (var effect in effects)
        {
            PlayerStatus.instance?.ApplyEffect(effect); // PlayerStatus 인스턴스가 있다면 효과 적용
        }
    }

    private void RemoveSynergyEffects(List<Effect> effects)
    {
        foreach (var effect in effects)
        {
            PlayerStatus.instance?.RemoveEffect(effect); // PlayerStatus 인스턴스가 있다면 효과 제거
        }
    }

    private void ApplySynergyTraitEffects(List<TraitEffect> traitEffects)
    {
        foreach (var trait in traitEffects)
        {
            TraitSynergy.Instance?.AddTrait(trait.traitType, trait.stackAmount); // TraitSynergy 인스턴스가 있다면 특성 추가
        }
    }

    private void RemoveSynergyTraitEffects(List<TraitEffect> traitEffects)
    {
        foreach (var trait in traitEffects)
        {
            TraitSynergy.Instance?.RemoveTrait(trait.traitType, trait.stackAmount); // TraitSynergy 인스턴스가 있다면 특성 제거
        }
    }

    // =======================================================
    // UI 업데이트 메서드 (이전에 추가된 부분)
    // =======================================================
    private void UpdateSynergyDisplayName(string synergyName)
    {
        // currentSynergyNameText 필드가 없다면, 이 메서드는 UI 업데이트를 수행하지 않습니다.
        // 이 필드는 SynergyUITextConnector 스크립트를 통해 연결되어야 합니다.
        if (currentSynergyNameText != null) 
        {
            if (string.IsNullOrEmpty(synergyName))
            {
                currentSynergyNameText.text = "현재 활성화된 시너지: 없음"; 
            }
            else
            {
                currentSynergyNameText.text = "현재 활성화된 시너지: " + synergyName;
            }
        }
        else
        {
            Debug.LogWarning("[SynergyManager] 시너지 이름을 표시할 UI Text 요소가 할당되지 않았습니다! SynergyUITextConnector가 활성화된 씬의 UI를 연결했는지 확인하세요.");
        }
    }

    // =======================================================
    // 테스트용 Context Menu (Item SO를 ID로 찾는 도우미 함수)
    // =======================================================
    private Item GetItemById(string itemId)
    {
        if (Instance != null) 
        {
            return Instance.allItems.FirstOrDefault(item => item != null && item.id == itemId);
        }
        Debug.LogError("CombinationSynergyManager 인스턴스를 찾을 수 없어 Item SO를 가져올 수 없습니다!");
        return null;
    }
}