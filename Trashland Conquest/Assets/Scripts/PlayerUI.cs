using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // UI 관련 네임스페이스는 기본이지

public class PlayerUI : MonoBehaviour
{
    // 누가봐도 무슨 용도인지 모르는 변수는 그냥 놔뒀다 ㅋㅋㅋㅋ
    public GameObject who; 
    
    // HP UI
    public RectTransform fillRT; // HP 바 (fill)
    public Text hpText; // HP 텍스트

    // EXP UI (주석 처리된거 살릴거면 니가 살려라)
    //public RectTransform expRT; // 경험치 바
    public Text expText; // 경험치 텍스트

    // 스탯 텍스트
    public Text attack_powerText;
    public Text armorText;
    public Text levelText;
    public Text nameText;

    // 트레잇 텍스트
    public Text milkText;
    public Text slushText;
    public Text alcoholText;
    public Text sodaText;
    public Text energyDrinkText;
    public Text coffeeText;
    public Text pesticideText;
    public Text purifiedWaterText;

    [Header("Ammo UI")]
    public Text bulletGaugeText;

    // 스태미나 UI (니가 새로 추가한 부분 그대로 살려둠)
    [Header("Stamina UI")]
    public RectTransform staminaFillRT; // 스태미나 바 fill RectTransform
    public Text staminaText;            // 스태미나 텍스트 (예: 100/100)
    
    // 이 씨발놈의 쉴드 UI (2D 오브젝트 리스트로 관리)
    [Header("Shield UI - New")] // 헤더는 알아서 보기 좋게 바꿔라
    public List<GameObject> shieldIcons = new List<GameObject>(); // 여기에 쉴드 사각형 오브젝트들을 직접 연결해라
    public GameObject shieldContainer; // 쉴드 아이콘들을 감싸는 부모 오브젝트 (쉴드 0개일 때 통째로 끄거나 켤 용도)

    // 내부에서 사용할 레퍼런스
    private PlayerStatus playerStatus;
    private TraitSynergy traitSynergy;

    // 이전 값 추적용 (변경사항 있을 때만 업데이트하려고)
    private float prevHP = -1f;
    // 이전 쉴드 값 추적용. 이제 int형으로 카운트할 거니까 float은 필요없음.
    private int prevShieldCount = -1; 
    //private int prevEXP = -1;
    private int prevLV = -1;
    private float prevBaseAtk = -1;
    private float prevBonusAtk = -1;
    private float prevArmor = -1;

    // 스태미나 러프 보간용 변수
    private float currentStaminaRate = 0f; 
    public float staminaLerpSpeed = 1f; 
    
    // 트레잇 이전 상태 저장용
    private Dictionary<TraitSynergy.TraitType, int> prevTraits = new();

    // UI 흔들림 효과 제어용
    private bool isShaking = false;

    // 시작할 때 딱 한 번 호출됨
    void Start()
    {
        // PlayerStatus랑 TraitSynergy 인스턴스 가져오기 (싱글톤 방식이겠지?)
        playerStatus = PlayerStatus.instance;
        traitSynergy = TraitSynergy.Instance;
        
        // PlayerStatus가 없으면 걍 꺼져라
        if (playerStatus == null)
        {
            Debug.LogError("PlayerStatus 인스턴스를 찾을 수 없습니다. 게임 시작 시 PlayerStatus가 활성화되어 있는지 확인하세요.");
            enabled = false; // 이 스크립트 비활성화
            return;
        }

        // 스태미나 바 초기화
        currentStaminaRate = playerStatus.currentStamina / playerStatus.maxStamina;
        SetStaminaBar(currentStaminaRate);

        // 쉴드 UI 초기 상태 업데이트 (Start에서 한 번 해주는 게 좋음)
        UpdateShield(); 
    }

    // 매 프레임마다 호출됨
    void Update()
    {
        // playerStatus 없으면 아무것도 안 함
        if (playerStatus == null) return;

        // 각종 UI 업데이트 함수 호출
        UpdateHP();
        UpdateShield(); // 쉴드 카운트 변화 감지 및 UI 업데이트
        //UpdateEXP(); // 주석 처리된 경험치 업데이트
        UpdateStatusText();
        UpdateTraits();
        UpdateStamina(); // 스태미나 UI 업데이트
    }

    // HP UI 업데이트
    void UpdateHP()
    {
        // currentHP는 0보다 작아질 수 없게 Mathf.Max로 방어
        float currentHP = Mathf.Max(playerStatus.currentHP, 0); 
        
        // HP가 변했을 때만 UI 업데이트
        if (currentHP != prevHP)
        {
            float rate = currentHP / playerStatus.maxHP; // HP 비율 계산
            SetHPbar(rate); // HP 바 크기 조절
            hpText.text = $"{currentHP} / {playerStatus.maxHP}"; // HP 텍스트 업데이트
            fillRT.gameObject.SetActive(rate > 0); // HP가 0보다 클 때만 HP 바 활성화
            
            // 데미지 입었을 때 UI 흔들림 효과
            if (currentHP < prevHP && !isShaking)
            {
                StartCoroutine(ShakeUI(GetComponent<RectTransform>(), 0.1f, 5f)); // 전체 UI도 흔들기 (원래 있던 코드)
            }
            prevHP = currentHP; // 이전 HP 값 저장
        }
    }

    // 쉴드 UI 업데이트 (핵심!)
    void UpdateShield()
    {
        // PlayerStatus에서 현재 쉴드 카운트 가져오기
        int currentShield = playerStatus.currentShield; 
        
        // **!!! 여기를 수정한다 !!!**
        // 쉴드 아이콘을 표시할 최대 개수를 PlayerStatus의 maxShield 값으로 설정한다.
        // shieldIcons.Count는 프리팹에 연결된 전체 아이콘의 갯수이고,
        // totalDisplayShieldSlots는 실제 maxShield 값만큼만 표시하겠다는 뜻이다.
        int totalDisplayShieldSlots = playerStatus.maxShield; 

        // 쉴드 카운트가 변했을 때만 UI 업데이트
        if (currentShield != prevShieldCount)
        {
            // 모든 쉴드 아이콘의 활성화/비활성화 상태를 현재 쉴드 카운트에 맞춰 조절
            // 루프는 미리 연결해둔 shieldIcons 리스트의 총 개수만큼 돈다.
            for (int i = 0; i < shieldIcons.Count; i++) // 주의: shieldIcons.Count를 기준으로 루프 돈다.
            {
                if (shieldIcons[i] == null) continue; // 혹시 연결 끊어진 아이콘 있으면 스킵

                // i번째 쉴드 아이콘을 활성화할 조건:
                // 1. 현재 쉴드 카운트보다 작을 때 (즉, 0, 1, 2... currentShield-1 까지)
                // 2. 그리고 playerStatus.maxShield 값보다 작을 때 (UI에 표시할 최대 쉴드 개수 제한)
                // 이 두 조건을 모두 만족해야 활성화 된다.
                shieldIcons[i].SetActive(i < currentShield && i < totalDisplayShieldSlots); 
            }

            // 쉴드 아이콘들을 감싸는 부모 오브젝트 활성화/비활성화 (선택 사항)
            if (shieldContainer != null)
            {
                // 쉴드가 하나라도 있거나, 쉴드를 보여줄 슬롯 자체가 하나라도 있으면 켜둔다.
                shieldContainer.SetActive(currentShield > 0 || totalDisplayShieldSlots > 0); 
            }
            
            prevShieldCount = currentShield; // 이전 쉴드 카운트 저장
        }
    }

    // UpdateEXP()는 주석 처리되어 있으니 건드리지 않음
    //void UpdateEXP()
    //{
    //    if (playerStatus.currentEXP != prevEXP)
    //    {
    //        float rate = (float)playerStatus.currentEXP / playerStatus.maxEXP;
    //        expRT.localScale = new Vector3(rate, 1f, 1f);
    //        expText.text = $"{playerStatus.currentEXP} / {playerStatus.maxEXP}";
    //        prevEXP = playerStatus.currentEXP;
    //    }
    //}

    // 스탯 텍스트 업데이트
    void UpdateStatusText()
    {
        if (playerStatus.unitLV != prevLV)
        {
            levelText.text = $"레벨: {playerStatus.unitLV}";
            prevLV = playerStatus.unitLV;
        }
        if (playerStatus.unitName != nameText.text)
        {
            nameText.text = $"이름: {playerStatus.unitName}";
        }
        if (playerStatus.armor != prevArmor)
        {
            armorText.text = $"방어력: {playerStatus.armor}";
            prevArmor = playerStatus.armor;
        }
        // 공격력은 Base와 Bonus가 모두 변했을 때만 업데이트
        if (playerStatus.baseAttackPower != prevBaseAtk || playerStatus.bonusAttackPower != prevBonusAtk)
        {
            attack_powerText.text = $"공격력: {playerStatus.baseAttackPower} (+ {playerStatus.bonusAttackPower})";
            prevBaseAtk = playerStatus.baseAttackPower;
            prevBonusAtk = playerStatus.bonusAttackPower;
        }
    }

    // 트레잇 텍스트 업데이트
    void UpdateTraits()
    {
        foreach (TraitSynergy.TraitType type in System.Enum.GetValues(typeof(TraitSynergy.TraitType)))
        {
            int current = traitSynergy.GetStack(type);
            if (!prevTraits.ContainsKey(type) || prevTraits[type] != current)
            {
                prevTraits[type] = current;
                UpdateTraitText(type, current);
            }
        }
    }

    // 트레잇 타입에 따른 텍스트 설정
    void UpdateTraitText(TraitSynergy.TraitType type, int value)
    {
        switch (type)
        {
            case TraitSynergy.TraitType.Milk: milkText.text = $"우유: {value}"; break;
            case TraitSynergy.TraitType.Slush: slushText.text = $"슬러시: {value}"; break;
            case TraitSynergy.TraitType.Alcohol: alcoholText.text = $"알코올: {value}"; break;
            case TraitSynergy.TraitType.Soda: sodaText.text = $"탄산: {value}"; break;
            case TraitSynergy.TraitType.EnergyDrink: energyDrinkText.text = $"이온: {value}"; break;
            case TraitSynergy.TraitType.Coffee: coffeeText.text = $"커피: {value}"; break;
            case TraitSynergy.TraitType.Pesticide: pesticideText.text = $"농약: {value}"; break;
            case TraitSynergy.TraitType.PurifiedWater: purifiedWaterText.text = $"정제수: {value}"; break;
        }
    }

    // 탄약 게이지 업데이트
    public void UpdateBulletGauge(int currentAmmo, int maxAmmo)
    {
        // 10발 미만일 때 띄어쓰기로 정렬하는거 ㅋㅋㅋㅋㅋ 센스 있네
        if (currentAmmo > 9)
            bulletGaugeText.text = $"{currentAmmo}";
        else
            bulletGaugeText.text = $"  {currentAmmo}"; 
    }

    // 탄약 재장전 코루틴
    public IEnumerator ReloadBulletGauge(int from, int to, float interval = 0.05f)
    {
        for (int i = from; i <= to; i++)
        {
            UpdateBulletGauge(i, to);
            yield return new WaitForSeconds(interval);
        }
    }

    // 스태미나 UI 업데이트 (Lerp로 부드럽게)
    void UpdateStamina()
    {
        float targetStamina = Mathf.Max(playerStatus.currentStamina, 0); 
        float maxStamina = playerStatus.maxStamina;
        float targetRate = targetStamina / maxStamina;

        // 스태미나 바를 Lerp로 부드럽게 움직임 (니가 추가한거 그대로)
        currentStaminaRate = Mathf.Lerp(currentStaminaRate, targetRate, Time.deltaTime * staminaLerpSpeed);
        SetStaminaBar(currentStaminaRate); 

        // 스태미나 텍스트는 즉시 업데이트 (소수점 없이 반올림)
        if (staminaText != null)
        {
            staminaText.text = $"{Mathf.RoundToInt(targetStamina)} / {Mathf.RoundToInt(maxStamina)}";
        }
    }

    // HP 바 크기 조절
    public void SetHPbar(float rate)
    {
        fillRT.localScale = new Vector3(rate, 1f, 1f);
    }

    // SetShieldbar는 이제 필요 없으니까 삭제했다.

    // 스태미나 바 크기 조절
    public void SetStaminaBar(float rate)
    {
        staminaFillRT.localScale = new Vector3(rate, 1f, 1f);
    }

    // UI 흔들림 코루틴
    IEnumerator ShakeUI(RectTransform target, float duration = 0.1f, float magnitude = 5f)
    {
        if (isShaking) yield break; // 이미 흔들리고 있으면 중복 실행 방지
        isShaking = true;

        Vector3 originalPos = target.anchoredPosition; // 원래 위치 저장
        float elapsed = 0f; // 경과 시간

        while (elapsed < duration)
        {
            // 랜덤한 위치로 흔들기
            float offsetX = Random.Range(-1f, 1f) * magnitude;
            float offsetY = Random.Range(-1f, 1f) * magnitude;
            target.anchoredPosition = originalPos + new Vector3(offsetX, offsetY, 0f);
            
            elapsed += Time.deltaTime; // 시간 업데이트
            yield return null; // 다음 프레임까지 대기
        }

        target.anchoredPosition = originalPos; // 원래 위치로 되돌리기
        isShaking = false; // 흔들림 종료
    }
}