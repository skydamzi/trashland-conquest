using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TraitSynergy : MonoBehaviour
{
    public static TraitSynergy Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public enum TraitType
    {
        None,
        Milk,
        Slush,
        Alcohol,
        Soda,
        EnergyDrink,
        Coffee,
        Pesticide,
        PurifiedWater
    }

    [System.Serializable]
    public class TraitStack
    {
        public TraitType trait;
        public int stack;

        public TraitStack(TraitType trait, int stack)
        {
            this.trait = trait;
            this.stack = stack;
        }
    }

    // 현재 활성화된 속성들의 목록
    public List<TraitStack> activeTraits = new List<TraitStack>();

    // 속성 최대 스택 수
    private const int MaxStack = 5;

    // 속성 추가 함수 (중복이면 스택 증가, 최대값 제한)
    public void AddTrait(TraitType trait, int amount = 1)
    {
        if (trait == TraitType.None) return;

        TraitStack found = activeTraits.Find(t => t.trait == trait);

        if (found != null)
        {
            // 이미 있다면 스택만 증가
            found.stack = Mathf.Min(found.stack + amount, MaxStack);
        }
        else
        {
            // 없으면 새로 추가
            int stackToAdd = Mathf.Min(amount, MaxStack);
            activeTraits.Add(new TraitStack(trait, stackToAdd));
        }

        Debug.Log($"[TraitSynergy] 속성 추가됨: {trait}, 현재 스택: {GetStack(trait)}");
    }

    // 속성 제거 함수 (스택이 0 이하면 목록에서 삭제)
    public void RemoveTrait(TraitType trait, int amount = 1)
    {
        TraitStack found = activeTraits.Find(t => t.trait == trait);
        if (found != null)
        {
            found.stack -= amount;
            if (found.stack <= 0)
            {
                activeTraits.Remove(found);
            }
        }

        Debug.Log($"[TraitSynergy] 속성 제거됨: {trait}, 현재 스택: {GetStack(trait)}");
    }

    // 특정 속성의 현재 스택 수 반환
    public int GetStack(TraitType trait)
    {
        foreach (TraitStack t in activeTraits)
        {
            if (t.trait == trait)
                return t.stack;
        }
        return 0; // 없으면 0
    }

    // 특정 속성이 존재하는지 여부 반환
    public bool HasTrait(TraitType trait)
    {
        return GetStack(trait) > 0;
    }

    // 두 속성이 동시에 있는지 확인 (시너지 조건)
    public bool HasSynergy(TraitType traitA, TraitType traitB)
    {
        return HasTrait(traitA) && HasTrait(traitB);
    }

    // 랜덤 속성 부여 여부 플래그
    private bool randomTraitsGiven = false;

    // 한 번만 랜덤 속성 부여
    public void AssignRandomTraitsOnce()
    {
        if (randomTraitsGiven) return;

        AddTrait(TraitType.Milk, Random.Range(0, 6));
        AddTrait(TraitType.Slush, Random.Range(0, 6));
        AddTrait(TraitType.Alcohol, Random.Range(0, 6));
        AddTrait(TraitType.Soda, Random.Range(0, 6));
        AddTrait(TraitType.EnergyDrink, Random.Range(0, 6));
        AddTrait(TraitType.Coffee, Random.Range(0, 6));
        AddTrait(TraitType.Pesticide, Random.Range(0, 6));
        AddTrait(TraitType.PurifiedWater, Random.Range(0, 6));

        randomTraitsGiven = true;
        Debug.Log("[TraitSynergy] 랜덤 속성 초기화 완료");
    }

    // 전체 속성 초기화 (게임 리셋 등)
    public void ResetTraits()
    {
        activeTraits.Clear();
        randomTraitsGiven = false;
    }
}
