using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.U2D; // SpriteAtlas 사용할 경우 (선택사항, 안 쓰면 지워도 됨)

public class PlayerUI : MonoBehaviour
{
    // 누가봐도 무슨 용도인지 모르는 변수는 일단 놔둠 (니가 원래 준 것)
    public GameObject who; 
    
    // HP UI (니가 원래 준 것)
    public RectTransform fillRT; // HP 바 (fill)
    public Text hpText; // HP 텍스트
    // EXP UI (니가 주석 처리했기에 그대로 주석 유지)
    //public RectTransform expRT; // 경험치 바

    // 스탯 텍스트 (니가 원래 준 것)
    public Text attack_powerText;
    public Text armorText;
    public Text levelText;
    public Text nameText;

    [Header("Ammo UI")] // (니가 원래 준 것)
    public Text bulletGaugeText;

    // 스태미나 UI (니가 새로 추가한 부분 그대로 살려둠)
    [Header("Stamina UI")]
    public RectTransform staminaFillRT; // 스태미나 바 fill RectTransform
    public Text staminaText;            // 스태미나 텍스트 (예: 100/100)

    // EXP UI (니가 새로 추가한 부분 그대로 살려둠)
    [Header("EXP UI")]
    public RectTransform expRT; // 경험치 바 fill RectTransform

    // 이 씨발놈의 쉴드 UI (동적으로 생성/삭제 + 크기 조절!) - 최종 방식
    [Header("Shield UI - Dynamic")] 
    public GameObject shieldIconPrefab; // 쉴드 아이콘으로 사용할 프리팹 (사각형 Image, Layout Element 필요!)
    public RectTransform shieldContainer; // 쉴드 아이콘들이 들어갈 부모 컨테이너 (하얀색 바 역할)
    
    // 쉴드 러프 보간용 변수 추가 (새로 추가됨)
    private float[] currentShieldAlphaRates; // 각 쉴드 아이콘의 현재 투명도 비율 (0.0 ~ 1.0)
    public float shieldLerpSpeed = 5f; // 쉴드 투명도 변화 속도 (에디터에서 조절)

    // 내부에서 사용할 레퍼런스 (모두 포함)
    private PlayerStatus playerStatus;
    private TraitSynergy traitSynergy;
    private List<GameObject> activeShieldIcons = new List<GameObject>(); // 현재 활성화된 쉴드 아이콘들 리스트

    // 이전 값 추적용 (모두 포함)
    private float prevHP = -1f;
    //private int prevShieldCount = -1; 
    private int prevMaxShield = -1; 
    private int prevEXP = -1; // 니가 주석 처리했던 EXP 관련 prev 변수도 넣어둠
    private int prevLV = -1;
    private float prevBaseAtk = -1;
    private float prevBonusAtk = -1;
    private float prevArmor = -1;

    // 스태미나 러프 보간용 변수 (니가 추가한 것)
    private float currentStaminaRate = 0f; 
    public float staminaLerpSpeed = 1f; 
    
    // 트레잇 이전 상태 저장용 (니가 원래 준 것)
    private Dictionary<TraitSynergy.TraitType, int> prevTraits = new();

    // UI 흔들림 효과 제어용 (니가 원래 준 것)
    private bool isShaking = false;

    // 시작할 때 딱 한 번 호출됨
    void Start()
    {
        playerStatus = PlayerStatus.instance;
        traitSynergy = TraitSynergy.Instance;
        
        if (playerStatus == null)
        {
            Debug.LogError("PlayerStatus 인스턴스를 찾을 수 없습니다. 게임 시작 시 PlayerStatus가 활성화되어 있는지 확인하세요.");
            enabled = false;
            return;
        }

        // 스태미나 초기화 (니가 원래 줬던 코드)
        currentStaminaRate = playerStatus.currentStamina / playerStatus.maxStamina;
        SetStaminaBar(currentStaminaRate);

        // 쉴드 UI 초기화 (UpdateShieldUIInitial이 maxShield에 따라 아이콘을 생성하고, 
        // Update가 첫 프레임에 UpdateShield를 호출하여 투명도를 업데이트할 것임)
        UpdateShieldUIInitial(); 
    }

    // 매 프레임마다 호출됨
    void Update()
    {
        if (playerStatus == null) return;

        UpdateHP();
        UpdateShield(); // 매 프레임마다 쉴드 투명도 업데이트를 위해 호출
        UpdateStamina(); 
        UpdateEXP();
        UpdateStatusText();

    }

    // HP UI 업데이트
    void UpdateHP()
    {
        float currentHP = Mathf.Max(playerStatus.currentHP, 0); 
        if (currentHP != prevHP)
        {
            float rate = currentHP / playerStatus.maxHP; 
            SetHPbar(rate); 
            hpText.text = $"{currentHP} / {playerStatus.maxHP}"; 
            fillRT.gameObject.SetActive(rate > 0); 
            if (currentHP < prevHP && !isShaking)
            {
                // UI 흔들림을 HP 바에 직접 적용하지 않고, 전체 UI를 흔드는 경우가 아니라면 
                // fillRT나 hpText의 부모 RectTransform을 흔드는 게 더 자연스러울 수 있다.
                // 일단 니 코드에 맞춰서 GetComponent<RectTransform>()으로 둠.
                StartCoroutine(ShakeUI(GetComponent<RectTransform>(), 0.1f, 5f));
            }
            prevHP = currentHP; 
        }
    }

    // 쉴드 UI 초기화 및 재구성용 함수 (Start에서 호출되거나 maxShield 변경 시 호출)
    private void UpdateShieldUIInitial()
    {
        if (shieldContainer == null || shieldIconPrefab == null || playerStatus == null) return;

        int maxShield = playerStatus.maxShield;

        // 기존 쉴드 아이콘들 전부 제거
        foreach (GameObject icon in activeShieldIcons)
        {
            Destroy(icon); 
        }
        activeShieldIcons.Clear(); 

        // currentShieldAlphaRates 배열도 maxShield에 맞춰 초기화 (새로 추가된 부분)
        currentShieldAlphaRates = new float[maxShield]; 

        // 새로운 maxShield 값에 맞춰 쉴드 아이콘들 생성
        for (int i = 0; i < maxShield; i++)
        {
            GameObject newIcon = Instantiate(shieldIconPrefab, shieldContainer);
            activeShieldIcons.Add(newIcon);
            currentShieldAlphaRates[i] = 1f; // 새로 생성된 쉴드는 일단 불투명하게 시작 (투명도 100%)
        }

        // 쉴드 아이콘들의 크기를 계산하여 Layout Element에 적용 (핵심!)
        if (maxShield > 0)
        {
            HorizontalLayoutGroup layoutGroup = shieldContainer.GetComponent<HorizontalLayoutGroup>();
            if (layoutGroup == null)
            {
                Debug.LogError("ShieldContainer에 HorizontalLayoutGroup이 없습니다. 추가해주세요.");
                return;
            }

            // 컨테이너의 실제 가로 폭 (패딩 제외)
            float containerWidth = shieldContainer.rect.width; 
            float totalPadding = layoutGroup.padding.left + layoutGroup.padding.right;
            float totalSpacing = layoutGroup.spacing * (maxShield - 1); 
            if (maxShield <= 1) totalSpacing = 0; // 아이콘이 하나일 때는 스페이싱 없음

            float availableWidth = containerWidth - totalPadding - totalSpacing;
            
            // 각 아이콘이 가져야 할 너비 계산
            float preferredIconWidth = (maxShield > 0) ? (availableWidth / maxShield) : 0f;
            
            // 각 아이콘의 Layout Element에 계산된 너비 및 컨테이너 높이 적용
            foreach (GameObject icon in activeShieldIcons)
            {
                LayoutElement layoutElement = icon.GetComponent<LayoutElement>();
                if (layoutElement == null)
                {
                    Debug.LogError("ShieldIconPrefab에 LayoutElement 컴포넌트가 없습니다. 추가해주세요.");
                    continue; 
                }
                layoutElement.preferredWidth = preferredIconWidth;
                // 쉴드 컨테이너의 높이 - 상하 패딩을 아이콘의 preferredHeight로 설정
                layoutElement.preferredHeight = shieldContainer.rect.height - layoutGroup.padding.top - layoutGroup.padding.bottom;
            }
        }
        
        // 레이아웃 그룹 강제 업데이트 (필수!)
        LayoutRebuilder.ForceRebuildLayoutImmediate(shieldContainer);

        // 쉴드 컨테이너 활성화/비활성화 (maxShield가 0이 아니면 켜두기)
        shieldContainer.gameObject.SetActive(maxShield > 0); 
        
        prevMaxShield = maxShield; 
        //prevShieldCount = -1; // currentShield 업데이트 로직 강제 실행을 위해 -1로 초기화
    }


    // 쉴드 UI 업데이트 (핵심! - 투명도 Lerp)
    void UpdateShield()
    {
        if (playerStatus == null) return; 

        int currentShield = playerStatus.currentShield; 
        int maxShield = playerStatus.maxShield; 

        // 1. 최대 쉴드 개수가 변경되면 UI를 재구성한다.
        if (maxShield != prevMaxShield)
        {
            UpdateShieldUIInitial(); 
        }

        // 2. 각 쉴드 아이콘의 투명도를 Lerp로 조절한다.
        // 현재 쉴드 개수(currentShield)가 변경되었을 때뿐만 아니라,
        // 매 프레임마다 currentShieldAlphaRates가 목표값(0 또는 1)에 도달하도록 Lerp를 적용한다.
        for (int i = 0; i < activeShieldIcons.Count; i++)
        {
            if (activeShieldIcons[i] != null) 
            {
                Image iconImage = activeShieldIcons[i].GetComponent<Image>();
                if (iconImage != null)
                {
                    float targetAlpha = (i < currentShield) ? 1f : 0f; // 목표 투명도 (채워지면 1, 비워지면 0)
                    
                    // 현재 투명도 비율을 목표 투명도 비율로 Lerp
                    currentShieldAlphaRates[i] = Mathf.Lerp(currentShieldAlphaRates[i], targetAlpha, Time.deltaTime * shieldLerpSpeed);
                    
                    // 실제 이미지의 알파값에 적용
                    Color originalColor = iconImage.color; // 프리팹에 지정된 색상을 가져옴
                    iconImage.color = new Color(originalColor.r, originalColor.g, originalColor.b, currentShieldAlphaRates[i]); 
                }
            }
        }
        // prevShieldCount = currentShield; // 이제 이 변수는 쉴드 감소 러프 효과에서는 직접적으로 사용되지 않음
    }
    
    // --- 아래는 이전 코드에서 가져온 기타 UI 업데이트 함수들 ---

    //경험치 업데이트 함수 (니가 주석 처리한 상태로 넣어둠)
    void UpdateEXP()
    {
        if (playerStatus.currentEXP != prevEXP)
        {
            float rate = (float)playerStatus.currentEXP / playerStatus.maxEXP;
            expRT.localScale = new Vector3(rate, 1f, 1f);
            prevEXP = playerStatus.currentEXP;
        }
    }

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
        if (playerStatus.baseAttackPower != prevBaseAtk || playerStatus.bonusAttackPower != prevBonusAtk)
        {
            attack_powerText.text = $"공격력: {playerStatus.baseAttackPower} (+ {playerStatus.bonusAttackPower})";
            prevBaseAtk = playerStatus.baseAttackPower;
            prevBonusAtk = playerStatus.bonusAttackPower;
        }
    }



    public void UpdateBulletGauge(int currentAmmo, int maxAmmo)
    {
        if (currentAmmo > 9)
            bulletGaugeText.text = $"{currentAmmo}";
        else
            bulletGaugeText.text = $"  {currentAmmo}"; 
    }

    public IEnumerator ReloadBulletGauge(int from, int to, float interval = 0.05f)
    {
        for (int i = from; i <= to; i++)
        {
            UpdateBulletGauge(i, to);
            yield return new WaitForSeconds(interval);
        }
    }

    void UpdateStamina()
    {
        float targetStamina = Mathf.Max(playerStatus.currentStamina, 0); 
        float maxStamina = playerStatus.maxStamina;
        float targetRate = targetStamina / maxStamina;

        currentStaminaRate = Mathf.Lerp(currentStaminaRate, targetRate, Time.deltaTime * staminaLerpSpeed);
        SetStaminaBar(currentStaminaRate); 

        if (staminaText != null)
        {
            staminaText.text = $"{Mathf.RoundToInt(targetStamina)} / {Mathf.RoundToInt(maxStamina)}";
        }
    }

    public void SetHPbar(float rate)
    {
        fillRT.localScale = new Vector3(rate, 1f, 1f);
    }

    public void SetStaminaBar(float rate)
    {
        staminaFillRT.localScale = new Vector3(rate, 1f, 1f);
    }



    IEnumerator ShakeUI(RectTransform target, float duration = 0.1f, float magnitude = 5f)
    {
        if (isShaking) yield break; 
        isShaking = true;

        Vector3 originalPos = target.anchoredPosition; 
        float elapsed = 0f; 

        while (elapsed < duration)
        {
            float offsetX = Random.Range(-1f, 1f) * magnitude;
            float offsetY = Random.Range(-1f, 1f) * magnitude;
            target.anchoredPosition = originalPos + new Vector3(offsetX, offsetY, 0f);
            
            elapsed += Time.deltaTime; 
            yield return null; 
        }

        target.anchoredPosition = originalPos; 
        isShaking = false; 
    }
}