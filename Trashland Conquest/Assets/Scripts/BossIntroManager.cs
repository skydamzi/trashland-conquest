using System.Collections;
using UnityEngine;
using UnityEngine.UI; 

public class BossIntroManager : MonoBehaviour
{
    public bool IsIntroPlaying { get; private set; } 
    [Header("카메라 & 타겟")]
    public Camera mainCamera;
    public Transform player; 
    public Transform boss;   

    [Header("UI 요소")]
    public GameObject vsScreenUI;
    public GameObject playerUI; 
    public GameObject bossUI;   
    private Vector3 playerUIOriginalLocalPosition; // 플레이어 UI의 원래 로컬 위치 저장
    private Vector3 bossUIOriginalLocalPosition;   // 보스 UI의 원래 로컬 위치 저장
    public float uiSlideInDuration = 1.5f; // UI 슬라이드 인 애니메이션 시간
    public float uiSlideInOffset = 150f;    // UI 초기 위치 오프셋 (화면 밖으로 밀어낼 거리)


    [Header("VS 연출 오브젝트 및 설정")]
    public RectTransform playerVsImage; 
    public RectTransform bossVsImage;   
    public RectTransform vsText;        

    public float charSlideInDuration = 0.3f; 
    public float charPushBackAmount = 100f;  
    public float charPushBackDuration = 0.05f;
    public int impactRepeats = 6; // 충돌 효과 반복 횟수 (이전 요청으로 추가됨)

    public float vsTextScaleDuration = 0.3f; 
    public float vsTextStartScale = 0f;     // 시작 스케일 (0)
    public float vsTextEndScale = 0.13f;        

    // **새로 추가: 퇴장 연출 시간 및 사운드**
    public float charSlideOutDuration = 0.3f; // 캐릭터들이 화면 밖으로 퇴장하는 시간
    public float vsTextShrinkDuration = 0.2f; // VS 텍스트가 작아지는 시간 (이전 요청으로 추가됨)
    public AudioClip slideOutSound; // 캐릭터 퇴장 사운드 (선택 사항)

    public AudioClip slideSound; 
    public AudioClip impactSound; 
    public AudioClip vsTextSound; 
    private AudioSource audioSource; 

    [Header("연출 설정")]
    public float zoomInDuration = 1.5f;
    public float vsScreenDuration = 2.0f; 
    public float zoomOutDuration = 1.0f;
    public float zoomedInOrthoSize = 4.0f; 

    private Vector3 originalCameraPos;
    private float originalCameraOrthoSize;

    private PlayerSideView playerScript; // PlayerSideView 스크립트 타입이 실제 프로젝트와 맞는지 확인해라.
    private ToxicWasteBoss bossScript; // ToxicWasteBoss 스크립트 타입이 실제 프로젝트와 맞는지 확인해라.
    private Pause pauseScript; // Pause 스크립트 타입이 실제 프로젝트와 맞는지 확인해라.

    // 추가: VS 텍스트 회전 관련 변수
    [Header("VS 텍스트 회전 설정")]
    public float vsTextStartRotation = -180f; // 시작 회전 각도
    public float vsTextEndRotation = 0f;      // 최종 회전 각도

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
        IsIntroPlaying = false; 
    }

    void Start()
    {
        if (vsScreenUI != null) vsScreenUI.SetActive(false);
        
        // Player UI 설정
        if (playerUI != null)
        {
            playerUIOriginalLocalPosition = playerUI.transform.localPosition; // 원래 위치 저장
            // 시작 위치를 화면 위로 오프셋. 위에서 아래로 내려올 거니까 y값을 더한다.
            playerUI.transform.localPosition = playerUIOriginalLocalPosition + Vector3.up * uiSlideInOffset; 
            playerUI.SetActive(false); // 비활성화
        }
        
        // Boss UI 설정
        if (bossUI != null)
        {
            bossUIOriginalLocalPosition = bossUI.transform.localPosition; // 원래 위치 저장
            // 시작 위치를 화면 아래로 오프셋. 아래에서 위로 올라올 거니까 y값을 뺀다.
            bossUI.transform.localPosition = bossUIOriginalLocalPosition - Vector3.up * uiSlideInOffset; 
            bossUI.SetActive(false); // 비활성화
        }

        // null 체크는 필수다. 인스펙터에서 제대로 할당했는지 확인해라.
        if (player != null)
        {
            playerScript = player.GetComponent<PlayerSideView>();
            if (playerScript == null) Debug.LogError("BossIntroManager: Player Transform에 Player 스크립트가 없습니다!");
        }
        else Debug.LogError("BossIntroManager: Player Transform이 할당되지 않았습니다!");

        if (boss != null)
        {
            bossScript = boss.GetComponent<ToxicWasteBoss>();
            if (bossScript == null) Debug.LogError("BossIntroManager: Boss Transform에 ToxicWasteBoss 스크립트가 없습니다!");
        }
        else Debug.LogError("BossIntroManager: Boss Transform이 할당되지 않았습니다!");
        IsIntroPlaying = true;

        StartCoroutine(BossIntroSequence());
    }
    void Update()
    {
        // 인트로 시퀀스가 진행 중일 때만 ESC 키 입력을 처리
        if (IsIntroPlaying)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                // 인트로가 진행 중일 때 ESC가 눌리면 아무것도 하지 않고 무시한다.
                Debug.Log("보스 인트로 시퀀스 중에는 ESC를 눌러도 효과가 없습니다.");
                // 필요하다면 여기에 "인스턴스를 건너뛸 수 없습니다" 같은 텍스트 UI를 잠시 띄워줄 수도 있다.
                return; // 중요한 건 여기서 바로 리턴해서 다른 로직이 실행되지 않게 하는 것!
            }
        }
    }
    private IEnumerator BossIntroSequence()
    {
        // --- 1. 준비 단계: 플레이어와 보스의 스크립트만 비활성화 ---
        if (playerScript != null) { playerScript.enabled = false; Debug.Log("플레이어 스크립트 비활성화!"); }
        else Debug.LogError("플레이어 스크립트가 null이라 비활성화 실패!");
        if (bossScript != null) { bossScript.enabled = false; Debug.Log("보스 스크립트 비활성화!"); }
        else Debug.LogError("보스 스크립트가 null이라 비활성화 실패!");

        // 2. 원래 카메라 위치랑 2D 사이즈 저장
        originalCameraPos = mainCamera.transform.position;
        originalCameraOrthoSize = mainCamera.orthographicSize;

        // --- 3. 카메라를 보스에게 확대 ---
        float timer = 0f;
        Vector3 startPos = originalCameraPos;
        Vector3 targetPos = new Vector3(boss.position.x, boss.position.y, originalCameraPos.z);

        while (timer < zoomInDuration)
        {
            timer += Time.deltaTime;
            float t = timer / zoomInDuration;
            mainCamera.transform.position = Vector3.Lerp(startPos, targetPos, t);
            mainCamera.orthographicSize = Mathf.Lerp(originalCameraOrthoSize, zoomedInOrthoSize, t);
            yield return null;
        }
        // 확대 연출 끝난 후 최종 위치와 사이즈 보정
        mainCamera.transform.position = targetPos;
        mainCamera.orthographicSize = zoomedInOrthoSize;

        // --- 4. VS 화면 띄우기 및 등장 연출 시작! ---
        if (vsScreenUI != null) vsScreenUI.SetActive(true);

        // VS 연출을 위한 초기 설정
        float finalPlayerXPos = -275f;
        float finalBossXPos = 275f;
        float charVerticalPos = 0f;

        // Screen.width 대신 RectTransform의 부모 크기를 기준으로 하는 게 더 정확할 수 있다.
        // 하지만 일단 Screen.width로 간다. UI 캔버스 설정에 따라 다름.
        Vector2 playerInitialPos = new Vector2(-Screen.width / 2f - playerVsImage.rect.width, charVerticalPos);
        Vector2 bossInitialPos = new Vector2(Screen.width / 2f + bossVsImage.rect.width, charVerticalPos);
        Vector2 playerPreImpactPos = new Vector2(finalPlayerXPos - charPushBackAmount + 200f, charVerticalPos);
        Vector2 bossPreImpactPos = new Vector2(finalBossXPos + charPushBackAmount - 200f, charVerticalPos);
        Vector2 playerPushBackPos = new Vector2(finalPlayerXPos - (charPushBackAmount * 1.5f), charVerticalPos);
        Vector2 bossPushBackPos = new Vector2(finalBossXPos + (charPushBackAmount * 1.5f), charVerticalPos);
        Vector2 playerFinalPos = new Vector2(finalPlayerXPos, charVerticalPos);
        Vector2 bossFinalPos = new Vector2(finalBossXPos, charVerticalPos);


        if (playerVsImage != null) playerVsImage.anchoredPosition = playerInitialPos;
        if (bossVsImage != null) bossVsImage.anchoredPosition = bossInitialPos;
        if (vsText != null)
        {
            vsText.localScale = Vector3.one * vsTextStartScale;
            vsText.localRotation = Quaternion.Euler(0, 0, vsTextStartRotation); // 초기 회전값 설정
        }

        // 캐릭터 슬라이드 인
        Debug.Log("VS 연출: 캐릭터 슬라이드 인 시작!");
        if (slideSound != null) audioSource.PlayOneShot(slideSound);
        float slideTimer = 0f;
        while (slideTimer < charSlideInDuration)
        {
            slideTimer += Time.deltaTime;
            float t = slideTimer / charSlideInDuration;
            if (playerVsImage != null) playerVsImage.anchoredPosition = Vector2.Lerp(playerInitialPos, playerPreImpactPos, t);
            if (bossVsImage != null) bossVsImage.anchoredPosition = Vector2.Lerp(bossInitialPos, bossPreImpactPos, t);
            yield return null;
        }
        // 최종 위치 보정
        if (playerVsImage != null) playerVsImage.anchoredPosition = playerPreImpactPos;
        if (bossVsImage != null) bossVsImage.anchoredPosition = bossPreImpactPos;

        Debug.Log("VS 연출: 캐릭터 충돌 및 반복 밀려남 연출 시작!");

        // VS 텍스트 확대 연출을 동시에 시작
        StartCoroutine(VsTextScaleRoutine());


        // --- 1회차 충돌 및 제자리 복귀 ---
        // 1단계: 밀려남 (1회차)
        if (impactSound != null) audioSource.PlayOneShot(impactSound);
        float pushBackTimer = 0f;
        while (pushBackTimer < charPushBackDuration)
        {
            pushBackTimer += Time.deltaTime;
            float t = pushBackTimer / charPushBackDuration;
            if (playerVsImage != null) playerVsImage.anchoredPosition = Vector2.Lerp(playerPreImpactPos, playerPushBackPos, t);
            if (bossVsImage != null) bossVsImage.anchoredPosition = Vector2.Lerp(bossPreImpactPos, bossPushBackPos, t);
            yield return null;
        }
        // 2단계: 제자리 복귀 (1회차)
        pushBackTimer = 0f;
        while (pushBackTimer < charPushBackDuration * 0.5f)
        {
            pushBackTimer += Time.deltaTime;
            float t = pushBackTimer / (charPushBackDuration * 0.5f);
            if (playerVsImage != null) playerVsImage.anchoredPosition = Vector2.Lerp(playerPushBackPos, playerFinalPos, t);
            if (bossVsImage != null) bossVsImage.anchoredPosition = Vector2.Lerp(bossPushBackPos, bossFinalPos, t);
            yield return null;
        }


        // 최종 위치 보정
        if (playerVsImage != null) playerVsImage.anchoredPosition = playerFinalPos;
        if (bossVsImage != null) bossVsImage.anchoredPosition = bossFinalPos;

        // VS 텍스트 확대 연출은 위에서 이미 StartCoroutine으로 시작했으니, 이 부분은 삭제된다.
        // 그리고 이전에 계산했던 remainingVsScreenTime도 이제는 퇴장 연출 시간만 고려하도록 바뀐다.
        float remainingVsScreenTime = vsScreenDuration - (charSlideInDuration + charPushBackDuration * 1.5f); // 등장 연출 시간만 뺀다.
        remainingVsScreenTime -= Mathf.Max(charSlideOutDuration, vsTextShrinkDuration); // 퇴장 연출 중 가장 긴 시간을 뺀다.

        if (remainingVsScreenTime > 0)
        {
            yield return new WaitForSeconds(remainingVsScreenTime);
        }

        // --- 4.5. VS 연출 종료 (퇴장 연출 시작!) ---
        Debug.Log("VS 연출: 캐릭터 및 VS 텍스트 퇴장 시작!");
        if (slideOutSound != null) audioSource.PlayOneShot(slideOutSound); // 퇴장 사운드

        // 퇴장 목표 위치 (화면 밖)
        Vector2 playerExitPos = new Vector2(-Screen.width / 2f - playerVsImage.rect.width, charVerticalPos);
        Vector2 bossExitPos = new Vector2(Screen.width / 2f + bossVsImage.rect.width, charVerticalPos);

        // VS 텍스트 축소 목표 크기
        Vector3 vsTextShrinkTargetScale = Vector3.one * vsTextStartScale; // 시작 크기 (0)로 다시 줄어들게
        // VS 텍스트 회전 목표 각도 (다시 원래 시작 회전 각도로 돌아가게)
        Quaternion vsTextShrinkTargetRotation = Quaternion.Euler(0, 0, vsTextStartRotation);

        float exitTimer = 0f;
        // 캐릭터 퇴장 시간과 VS 텍스트 축소 시간 중 더 긴 시간을 기준으로 루프를 돌림
        float maxExitDuration = Mathf.Max(charSlideOutDuration, vsTextShrinkDuration);

        while (exitTimer < maxExitDuration)
        {
            exitTimer += Time.deltaTime;
            float t_char = exitTimer / charSlideOutDuration; // 캐릭터 퇴장 Lerp를 위한 t
            float t_vsText = exitTimer / vsTextShrinkDuration; // VS 텍스트 축소 Lerp를 위한 t

            // 캐릭터 퇴장 (charSlideOutDuration에 맞춰 진행)
            if (playerVsImage != null) playerVsImage.anchoredPosition = Vector2.Lerp(playerFinalPos, playerExitPos, Mathf.Clamp01(t_char));
            if (bossVsImage != null) bossVsImage.anchoredPosition = Vector2.Lerp(bossFinalPos, bossExitPos, Mathf.Clamp01(t_char));

            // VS 텍스트 축소 및 회전 (vsTextShrinkDuration에 맞춰 진행)
            if (vsText != null)
            {
                vsText.localScale = Vector3.Lerp(Vector3.one * vsTextEndScale, vsTextShrinkTargetScale, Mathf.Clamp01(t_vsText));
                // 회전에도 SmoothStep 적용
                vsText.localRotation = Quaternion.Slerp(Quaternion.Euler(0, 0, vsTextEndRotation), vsTextShrinkTargetRotation, Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t_vsText)));
            }

            yield return null;
        }
        // 최종적으로 위치 및 스케일 보정 (완전히 화면 밖으로, 스케일 0)
        if (playerVsImage != null) playerVsImage.anchoredPosition = playerExitPos;
        if (bossVsImage != null) bossVsImage.anchoredPosition = bossExitPos;
        if (vsText != null)
        {
            vsText.localScale = vsTextShrinkTargetScale; // 최종 스케일 적용 (0으로 완전히 사라지도록)
            vsText.localRotation = vsTextShrinkTargetRotation; // 최종 회전 적용
        }


        // VS 화면 UI 비활성화 (모든 연출 끝난 후)
        if (vsScreenUI != null) vsScreenUI.SetActive(false);


        // --- 5. 카메라 원래 위치로 복귀 ---
        timer = 0f;
        startPos = mainCamera.transform.position;
        float startOrthoSize = mainCamera.orthographicSize;

        while (timer < zoomOutDuration)
        {
            timer += Time.deltaTime;
            float t = timer / zoomOutDuration;
            mainCamera.transform.position = Vector3.Lerp(startPos, originalCameraPos, t);
            mainCamera.orthographicSize = Mathf.Lerp(startOrthoSize, originalCameraOrthoSize, t);
            yield return null;
        }
        // 최종 위치 보정
        mainCamera.transform.position = originalCameraPos;
        mainCamera.orthographicSize = originalCameraOrthoSize;

        // --- 6. 연출 끝나고 게임 시작 (UI 슬라이드 인 - 이지 아웃 적용) ---
        if (playerUI != null)
        {
            playerUI.SetActive(true);
            // 플레이어 UI는 위에서 아래로 (시작 위치는 위, 목표 위치는 원래 위치)
            StartCoroutine(SlideInUI(playerUI.transform, playerUIOriginalLocalPosition, uiSlideInDuration));
        }
        if (bossUI != null)
        {
            bossUI.SetActive(true);
            // 보스 UI는 아래에서 위로 (시작 위치는 아래, 목표 위치는 원래 위치)
            StartCoroutine(SlideInUI(bossUI.transform, bossUIOriginalLocalPosition, uiSlideInDuration));
        }
        IsIntroPlaying = false;
        Debug.Log("보스전 시작!");

        if (playerScript != null) { playerScript.enabled = true; Debug.Log("플레이어 스크립트 활성화!"); }
        if (bossScript != null) { bossScript.enabled = true; Debug.Log("보스 스크립트 활성화!"); }
    }

    // VS 텍스트 확대 및 회전 연출을 위한 분리된 코루틴
    private IEnumerator VsTextScaleRoutine()
    {
        Debug.Log("VS 연출: VS 텍스트 확대 및 회전 시작!");
        if (vsTextSound != null) audioSource.PlayOneShot(vsTextSound);
        float vsTextTimer = 0f;

        Quaternion startRotation = Quaternion.Euler(0, 0, vsTextStartRotation);
        Quaternion endRotation = Quaternion.Euler(0, 0, vsTextEndRotation);

        while (vsTextTimer < vsTextScaleDuration)
        {
            vsTextTimer += Time.deltaTime;
            float t = vsTextTimer / vsTextScaleDuration;
            // Mathf.SmoothStep을 사용하여 이지 아웃 효과 적용
            float easedT = Mathf.SmoothStep(0f, 1f, t); // 0에서 1까지의 t값을 부드럽게 변환

            if (vsText != null) 
            {
                vsText.localScale = Vector3.Lerp(Vector3.one * vsTextStartScale, Vector3.one * vsTextEndScale, easedT); // 스케일도 SmoothStep 적용
                vsText.localRotation = Quaternion.Slerp(startRotation, endRotation, easedT); // 회전에도 SmoothStep 적용!
            }
            yield return null;
        }
        // 최종 스케일 및 회전 보정
        if (vsText != null) 
        {
            vsText.localScale = Vector3.one * vsTextEndScale;
            vsText.localRotation = endRotation; // 최종 회전 적용
        }
    }

    // UI 슬라이드 인 코루틴 (이지 아웃 적용)
    private IEnumerator SlideInUI(Transform targetTransform, Vector3 targetLocalPosition, float slideDuration)
    {
        Vector3 startLocalPosition = targetTransform.localPosition; // 현재 위치가 시작점 (이미 오프셋 되어 있음)
        float timer = 0f;

        while (timer < slideDuration)
        {
            timer += Time.deltaTime;
            float t = timer / slideDuration;
            // Mathf.SmoothStep을 사용하여 이지 아웃 효과 적용
            float easedT = Mathf.SmoothStep(0f, 1f, t); // 0에서 1까지의 t값을 부드럽게 변환
            targetTransform.localPosition = Vector3.Lerp(startLocalPosition, targetLocalPosition, easedT);
            yield return null;
        }
        targetTransform.localPosition = targetLocalPosition; // 최종 위치 보정
    }
}