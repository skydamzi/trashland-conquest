using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TraitSynergy : MonoBehaviour
{
    // 싱글톤 인스턴스 (다른 곳에서 TraitSynergy.Instance로 접근 가능)
    public static TraitSynergy Instance;

    void Awake()
    {
        // 중복 인스턴스 방지
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬이 바뀌어도 유지
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 속성 종류 정의 (TraitType이라는 이름의 열거형 enum)
    public enum TraitType
    {
        None,           // 기본값 (아무 속성도 없음)
        Milk,
        Slush,
        Alcohol,
        Soda,
        EnergyDrink,
        Coffee,
        Pesticide,
        PurifiedWater
    }

    // 속성과 해당 스택 수를 저장하는 클래스
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

    // 현재 보유 중인 속성 목록
    public List<TraitStack> activeTraits = new List<TraitStack>();

    // 속성 당 최대 스택 수
    private const int MaxStack = 5;

    // 속성 추가 함수 (중복이면 스택 증가, 없으면 새로 추가)
    public void AddTrait(TraitType trait, int amount = 1)
    {
        if (trait == TraitType.None) return;

        TraitStack found = activeTraits.Find(t => t.trait == trait);

        if (found != null)
        {
            // 기존에 있다면 스택만 증가
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
    // 특정 속성의 현재 스택 수를 확인
    public int GetStack(TraitType trait)
    {
        foreach (TraitStack t in activeTraits)
        {
            if (t.trait == trait)
                return t.stack;
        }
        return 0; // 없으면 0
    }

    // 특정 속성을 가지고 있는지 확인
    public bool HasTrait(TraitType trait)
    {
        return GetStack(trait) > 0;
    }

    // 두 속성을 모두 가지고 있는지 (시너지 조건)
    public bool HasSynergy(TraitType traitA, TraitType traitB)
    {
        return HasTrait(traitA) && HasTrait(traitB);
    }

    private bool randomTraitsGiven = false;

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

    public void ResetTraits()
    {
        activeTraits.Clear();
        randomTraitsGiven = false;
    }
}
