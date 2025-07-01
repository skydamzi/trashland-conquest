using System.Collections;
using UnityEngine;
using UnityEngine.UI; 

public class BossIntroManager : MonoBehaviour
{
    [Header("카메라 & 타겟")]
    public Camera mainCamera;
    public Transform player; 
    public Transform boss;   

    [Header("UI 요소")]
    public GameObject vsScreenUI;
    public GameObject playerUI; 
    public GameObject bossUI;   

    [Header("VS 연출 오브젝트 및 설정")]
    public RectTransform playerVsImage; 
    public RectTransform bossVsImage;   
    public RectTransform vsText;        

    public float charSlideInDuration = 0.3f; 
    public float charPushBackAmount = 70f;   
    public float charPushBackDuration = 0.05f; 

    public float vsTextScaleDuration = 0.3f; 
    public float vsTextStartScale = 0f;    
    public float vsTextEndScale = 0.13f;      

    // **새로 추가: 퇴장 연출 시간 및 사운드**
    public float charSlideOutDuration = 0.3f; // 캐릭터들이 화면 밖으로 퇴장하는 시간
    public float vsTextShrinkDuration = 0.2f; // VS 텍스트가 작아지는 시간
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

    private Player playerScript;
    private ToxicWasteBoss bossScript;

    void Awake() 
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) 
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    void Start()
    {
        if (vsScreenUI != null) vsScreenUI.SetActive(false);
        if (playerUI != null) playerUI.SetActive(false); 
        if (bossUI != null) bossUI.SetActive(false);     

        if (player != null)
        {
            playerScript = player.GetComponent<Player>();
            if (playerScript == null) Debug.LogError("BossIntroManager: Player Transform에 Player 스크립트가 없습니다!");
        }
        else Debug.LogError("BossIntroManager: Player Transform이 할당되지 않았습니다!");

        if (boss != null)
        {
            bossScript = boss.GetComponent<ToxicWasteBoss>();
            if (bossScript == null) Debug.LogError("BossIntroManager: Boss Transform에 ToxicWasteBoss 스크립트가 없습니다!");
        }
        else Debug.LogError("BossIntroManager: Boss Transform이 할당되지 않았습니다!");

        StartCoroutine(BossIntroSequence());
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

        // --- 4. VS 화면 띄우기 및 등장 연출 시작! ---
        if (vsScreenUI != null) vsScreenUI.SetActive(true);
        
        // VS 연출을 위한 초기 설정
        float finalPlayerXPos = -300f; 
        float finalBossXPos = 300f;   
        float charVerticalPos = 0f; 

        Vector2 playerInitialPos = new Vector2(-Screen.width / 2f - playerVsImage.rect.width, charVerticalPos); 
        Vector2 bossInitialPos = new Vector2(Screen.width / 2f + bossVsImage.rect.width, charVerticalPos);     
        Vector2 playerPreImpactPos = new Vector2(finalPlayerXPos - charPushBackAmount, charVerticalPos); 
        Vector2 bossPreImpactPos = new Vector2(finalBossXPos + charPushBackAmount, charVerticalPos);     
        Vector2 playerPushBackPos = new Vector2(finalPlayerXPos - (charPushBackAmount * 1.5f), charVerticalPos); 
        Vector2 bossPushBackPos = new Vector2(finalBossXPos + (charPushBackAmount * 1.5f), charVerticalPos);     
        Vector2 playerFinalPos = new Vector2(finalPlayerXPos, charVerticalPos);
        Vector2 bossFinalPos = new Vector2(finalBossXPos, charVerticalPos);


        if (playerVsImage != null) playerVsImage.anchoredPosition = playerInitialPos;
        if (bossVsImage != null) bossVsImage.anchoredPosition = bossInitialPos;
        if (vsText != null) vsText.localScale = Vector3.one * vsTextStartScale; 

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
        if (playerVsImage != null) playerVsImage.anchoredPosition = playerPreImpactPos;
        if (bossVsImage != null) bossVsImage.anchoredPosition = bossPreImpactPos;


        // 충돌 및 살짝 밀려났다가 제자리로
        Debug.Log("VS 연출: 캐릭터 충돌 및 밀려남 연출!");
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
        if (playerVsImage != null) playerVsImage.anchoredPosition = playerPushBackPos;
        if (bossVsImage != null) bossVsImage.anchoredPosition = bossPushBackPos;


        pushBackTimer = 0f;
        while (pushBackTimer < charPushBackDuration * 0.5f) 
        {
            pushBackTimer += Time.deltaTime;
            float t = pushBackTimer / (charPushBackDuration * 0.5f);
            if (playerVsImage != null) playerVsImage.anchoredPosition = Vector2.Lerp(playerPushBackPos, playerFinalPos, t); 
            if (bossVsImage != null) bossVsImage.anchoredPosition = Vector2.Lerp(bossPushBackPos, bossFinalPos, t);         
            yield return null;
        }
        if (playerVsImage != null) playerVsImage.anchoredPosition = playerFinalPos;
        if (bossVsImage != null) bossVsImage.anchoredPosition = bossFinalPos;

        


        // VS 텍스트 확대 연출
        Debug.Log("VS 연출: VS 텍스트 확대 시작!");
        if (vsTextSound != null) audioSource.PlayOneShot(vsTextSound);
        float vsTextTimer = 0f;
        while (vsTextTimer < vsTextScaleDuration)
        {
            vsTextTimer += Time.deltaTime;
            float t = vsTextTimer / vsTextScaleDuration;
            if (vsText != null) vsText.localScale = Vector3.Lerp(Vector3.one * vsTextStartScale, Vector3.one * vsTextEndScale, t);
            yield return null;
        }
        if (vsText != null) vsText.localScale = Vector3.one * vsTextEndScale; 

        // VS 화면이 유지되는 시간 (퇴장 연출 시작 전 대기)
        // vsScreenDuration 전체에서 슬라이드인, 푸시백, 텍스트 확대 시간 등을 뺀 나머지 시간
        float totalIntroEffectDuration = charSlideInDuration + charPushBackDuration * 1.5f + vsTextScaleDuration;
        float remainingVsScreenTime = vsScreenDuration - totalIntroEffectDuration - charSlideOutDuration; // 퇴장 시간도 미리 뺌

        if (remainingVsScreenTime > 0)
            yield return new WaitForSeconds(remainingVsScreenTime);

        // --- 4.5. VS 연출 종료 (퇴장 연출 시작!) ---
        Debug.Log("VS 연출: 캐릭터 및 VS 텍스트 퇴장 시작!");
        if (slideOutSound != null) audioSource.PlayOneShot(slideOutSound); // 퇴장 사운드

        // 퇴장 목표 위치 (화면 밖)
        Vector2 playerExitPos = new Vector2(-Screen.width / 2f - playerVsImage.rect.width, charVerticalPos); 
        Vector2 bossExitPos = new Vector2(Screen.width / 2f + bossVsImage.rect.width, charVerticalPos);     
        
        // VS 텍스트 축소 목표 크기
        Vector3 vsTextShrinkScale = Vector3.one * vsTextStartScale; // 시작 크기 (0)로 다시 줄어들게

        float exitTimer = 0f;
        while (exitTimer < charSlideOutDuration) // 캐릭터 퇴장 시간 동안
        {
            exitTimer += Time.deltaTime;
            float t = exitTimer / charSlideOutDuration;
            
            // 캐릭터 퇴장
            if (playerVsImage != null) playerVsImage.anchoredPosition = Vector2.Lerp(playerFinalPos, playerExitPos, t);
            if (bossVsImage != null) bossVsImage.anchoredPosition = Vector2.Lerp(bossFinalPos, bossExitPos, t);
            
            // VS 텍스트 축소 (캐릭터 퇴장 시간과 병렬로 진행)
            if (vsText != null) vsText.localScale = Vector3.Lerp(Vector3.one * vsTextEndScale, vsTextShrinkScale, t / (charSlideOutDuration / vsTextShrinkDuration)); 
            // vsTextShrinkDuration에 맞춰 T 값을 조정하여 더 빨리 줄어들게 함.
            
            yield return null;
        }
        // 최종적으로 위치 및 스케일 보정 (완전히 화면 밖으로, 스케일 0)
        if (playerVsImage != null) playerVsImage.anchoredPosition = playerExitPos;
        if (bossVsImage != null) bossVsImage.anchoredPosition = bossExitPos;
        if (vsText != null) vsText.localScale = vsTextShrinkScale;

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

        mainCamera.transform.position = originalCameraPos;
        mainCamera.orthographicSize = originalCameraOrthoSize;

        // --- 6. 연출 끝나고 게임 시작 ---
        if (playerUI != null) playerUI.SetActive(true); 
        if (bossUI != null) bossUI.SetActive(true);     

        Debug.Log("보스전 시작!");

        if (playerScript != null) { playerScript.enabled = true; Debug.Log("플레이어 스크립트 활성화!"); }
        if (bossScript != null) { bossScript.enabled = true; Debug.Log("보스 스크립트 활성화!"); }
    }
}