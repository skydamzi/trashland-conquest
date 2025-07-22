using System.Collections;

using System.Collections.Generic;

using UnityEngine;



public class ToxicWasteBoss : Boss
{
    [Header("Poop Rain Settings")]
    public GameObject gooBombPrefab;             // 떨어지는 똥 프리팹
    public Transform bombArcPoint;               // 던지는 기준점 (보스 머리 위쪽)

    public float poopInterval = 0.05f;           // 똥 떨어지는 간격
    public float arcHeight = 5f;                // 똥 곡사 높이
    public float dropRangeX = 100f;              // 좌우 범위 (보스 기준)
    public float dropOffsetY = 0f;               // 드롭 위치 y 오프셋
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    public Color maxRedColor = new Color(1f, 0.3f, 0.3f, 1f); // 완전 빨간 느낌

    // **추가: 장판 똥 패턴 설정**
    [Header("Puddle Poop Settings")]
    public GameObject puddlePoopPrefab;          // 장판을 남길 똥 프리팹 (gooBombPrefab과 다를 수 있음)
    public GameObject puddlePrefab;              // 땅에 닿으면 생성될 장판 프리팹
    public float puddleDuration = 5f;            // 장판 지속 시간
    public int numberOfPuddlePoops = 3;          // 장판 똥 발사 개수
    public float puddlePoopInterval = 1.0f;      // 장판 똥 사이의 발사 간격 (기존 poopInterval보다 길게)
    public float puddleArcHeight = 5f;          // 장판 똥 곡사 높이 (다르게 줄 수도 있음)
    public float puddleDropRangeX = 20f;          // 장판 똥 좌우 범위 (더 넓게 줄 수도 있음)


    // **추가: CanAct 플래그 (다른 스크립트에서 제어 안 하려면 그냥 내부용으로 씀)**
    private bool _canAct = true; // _로 시작해서 내부용임을 명시

    void Awake() // Start 전에 초기화가 필요하면 Awake에서
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }

    void Start() // Start는 첫 프레임에만 호출된다.
    {
        currentHP = maxHP;
    }

    // **추가: 오브젝트가 활성화될 때마다 호출되는 함수**
    void OnEnable()
    {
        Debug.Log("ToxicWasteBoss: OnEnable 호출됨. 패턴 코루틴 시작 시도.");

        _canAct = true;

        if (poopRoutine != null)
        {
            StopCoroutine(poopRoutine);
        }
        
        poopRoutine = StartCoroutine(PoopRoutine());
    }

    // **추가: 오브젝트가 비활성화될 때 호출되는 함수**
    void OnDisable()
    {
        Debug.Log("ToxicWasteBoss: OnDisable 호출됨. 패턴 코루틴 중지.");
        _canAct = false; // 행동 불가로 설정

        if (poopRoutine != null)
        {
            StopCoroutine(poopRoutine);
            poopRoutine = null;
        }
        if (activePoopCoroutine != null) // 중첩된 코루틴도 멈춰야 함
        {
            StopCoroutine(activePoopCoroutine);
            activePoopCoroutine = null;
        }
        if (activePuddlePoopCoroutine != null) // 장판 똥 코루틴도 멈춰야 함
        {
            StopCoroutine(activePuddlePoopCoroutine);
            activePuddlePoopCoroutine = null;
        }
    }


    private Coroutine activePoopCoroutine; // 기존 똥 비 내리는 코루틴
    IEnumerator PoopRoutineContinuous() // 기존 똥 비
    {
        while (true)
        {
            if (!_canAct)
            {
                yield return null;
                continue;
            }
            DropPoop(gooBombPrefab, bombArcPoint.position, arcHeight, dropRangeX); // 일반 똥 드롭
            yield return new WaitForSeconds(poopInterval);
        }
    }

    // **새로운 패턴 코루틴: 장판 똥 발사**
    private Coroutine activePuddlePoopCoroutine;
    IEnumerator PuddlePoopAttackRoutine()
    {
        for (int i = 0; i < numberOfPuddlePoops; i++)
        {
            if (!_canAct) { yield break; } // 중간에 캔액트 꺼지면 중지

            // 장판을 남길 똥을 드롭 (PuddlePoopPrefab 사용)
            DropPoop(puddlePoopPrefab, bombArcPoint.position, puddleArcHeight, puddleDropRangeX, true); 
            yield return new WaitForSeconds(puddlePoopInterval);
        }
        activePuddlePoopCoroutine = null; // 패턴 끝나면 코루틴 참조 해제
    }


    private Coroutine poopRoutine; // 메인 패턴 루틴
    IEnumerator PoopRoutine()
    {
        while (true)
        {
            if (!_canAct)
            {
                yield return null;
                continue;
            }

            // 1. 부글부글 대기하면서 천천히 빨개짐 (기존 패턴)
            float preWait = 3f;
            float poopDuringPrewaitInterval = 0.5f;
            float poopTimer = 0f;
            for (float t = 0f; t < preWait; t += Time.deltaTime)
            {
                if (!_canAct) { yield return null; break; }
                
                float lerpFactor = t / preWait;
                spriteRenderer.color = Color.Lerp(originalColor, maxRedColor, lerpFactor);
                poopTimer += Time.deltaTime;
                if (poopTimer >= poopDuringPrewaitInterval)
                {
                    DropPoop(gooBombPrefab, bombArcPoint.position, arcHeight, dropRangeX); // 일반 똥 드롭
                    poopTimer = 0f;
                }
                yield return null;
            }

            if (!_canAct) { spriteRenderer.color = originalColor; yield break; }

            // 2. 대분출 (기존 패턴)
            spriteRenderer.color = maxRedColor;
            Debug.Log("대분출!!!");
            activePoopCoroutine = StartCoroutine(PoopRoutineContinuous());
            yield return new WaitForSeconds(2f);
            if (!_canAct) { StopCoroutine(activePoopCoroutine); spriteRenderer.color = originalColor; yield break; }
            StopCoroutine(activePoopCoroutine);
            activePoopCoroutine = null; // 코루틴이 멈췄으니 참조 해제

            // **추가: 장판 똥 패턴 중간에 삽입!**
            Debug.Log("장판 똥 발사 준비!");
            // 스프라이트 색깔을 잠시 다르게 할 수도 있음 (예: 녹색으로 변했다가 발사)
            // spriteRenderer.color = Color.Lerp(maxRedColor, Color.green, 0.5f); // 예시

            if (!_canAct) { yield break; }
            activePuddlePoopCoroutine = StartCoroutine(PuddlePoopAttackRoutine());
            // 장판 똥 패턴이 끝날 때까지 기다림 (numberOfPuddlePoops * puddlePoopInterval 만큼)
            yield return new WaitUntil(() => activePuddlePoopCoroutine == null || !_canAct);
            
            if (!_canAct) { yield break; }


            // 3. 원래 색으로 되돌아가기 (기존 패턴)
            float recoverDuration = 0.5f;
            for (float t = 0f; t < recoverDuration; t += Time.deltaTime)
            {
                if (!_canAct) { yield return null; break; }
                
                float lerpFactor = t / recoverDuration;
                spriteRenderer.color = Color.Lerp(maxRedColor, originalColor, lerpFactor);
                yield return null;
            }
            spriteRenderer.color = originalColor;

            if (!_canAct) { yield break; }

            // 4. 나머지 대기 시간 (기존 패턴)
            float totalCooldown = 5f;
            float remainingDelay = totalCooldown - (preWait + 2f + recoverDuration + (numberOfPuddlePoops * puddlePoopInterval)); // 장판 똥 패턴 시간도 빼줘야 정확!
            if (remainingDelay > 0f)
                yield return new WaitForSeconds(remainingDelay);
        }
    }

    // DropPoop 함수 수정: 프리팹, 시작점, 높이, 범위, 장판 여부를 인자로 받도록
    void DropPoop(GameObject poopToInstantiate, Vector2 startPoint, float arcH, float dropRX, bool isPuddlePoop = false)
    {
        if (startPoint == null) 
        {
            Debug.LogWarning("발사 기준점 이상함!");
            return;
        }
        if (!_canAct) return; 

        Vector2 targetPos = GetRandomDropPosition(startPoint, dropRX); // 드롭 위치 계산할 때 기준점과 범위 전달
        GameObject bomb = Instantiate(poopToInstantiate, startPoint, Quaternion.identity);

        Rigidbody2D rb = bomb.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.AddForce(CalculateArcForce(startPoint, targetPos, arcH), ForceMode2D.Impulse);
        }
        else // Rigidbody2D가 없으면 경고 로그 (이전 오류 해결되면 이 로그는 안 뜰 것)
        {
            Debug.LogWarning($"ToxicWasteBoss: {poopToInstantiate.name}에 Rigidbody2D가 없습니다. 물리 적용 안 됨.");
        }

        // **장판 똥이라면 PuddlePoop 스크립트 붙여서 처리**
        if (isPuddlePoop)
        {
            PuddlePoop puddlePoop = bomb.AddComponent<PuddlePoop>();
            if (puddlePoop != null) // 컴포넌트 추가 성공 시에만 값 할당
            {
                puddlePoop.puddlePrefab = puddlePrefab;
                puddlePoop.puddleDuration = puddleDuration;
                Debug.Log($"ToxicWasteBoss: PuddlePoop 스크립트 추가 및 설정 완료! Prefab: {puddlePrefab?.name}, Duration: {puddleDuration}");
                // PuddlePoop 스크립트가 스스로 삭제를 관리하므로, 여기서 bomb 삭제는 하지 않는다!
                // Destroy(bomb, 5f); 이 줄은 삭제 또는 주석 처리해야 함
            }
            else
            {
                Debug.LogError("ToxicWasteBoss: PuddlePoop 컴포넌트를 추가할 수 없습니다! PuddlePoop 스크립트가 올바른지 확인하세요.");
                // 컴포넌트 추가 실패 시에도 똥이 남아있지 않도록 일단 삭제.
                Destroy(bomb, 5f); 
            }
        }
        else // 일반 똥일 경우에만 5초 뒤 삭제 (원래 로직)
        {
            Destroy(bomb, 5f); // 일반 똥은 여기서 5초 뒤 삭제
        }
    }

    // GetRandomDropPosition 함수 수정: 기준점과 범위를 인자로 받도록
    Vector2 GetRandomDropPosition(Vector2 currentBombArcPoint, float currentDropRangeX)
    {
        float randomX = currentBombArcPoint.x + Random.Range(-currentDropRangeX / 2f, currentDropRangeX / 2f);
        float y = transform.position.y + dropOffsetY; // 보스 자체의 y 위치에 오프셋 적용
        return new Vector2(randomX, y);
    }

    Vector2 CalculateArcForce(Vector2 start, Vector2 target, float height)
    {
        float gravity = Mathf.Abs(Physics2D.gravity.y);
        float displacementY = target.y - start.y;
        Vector2 displacementXZ = new Vector2(target.x - start.x, 0);

        float timeUp = Mathf.Sqrt(2 * height / gravity);
        float timeDown = Mathf.Sqrt(2 * Mathf.Max(0.1f, height - displacementY) / gravity);
        float totalTime = timeUp + timeDown;

        Vector2 velocityY = Vector2.up * Mathf.Sqrt(2 * gravity * height);

        float xForceMultiplier = 0.35f; // 이 값은 조절해야 할 수 있음. 똥이 날아가는 속도에 영향.
        Vector2 velocityXZ = (displacementXZ / totalTime) * xForceMultiplier;

        return velocityXZ + velocityY;
    }
}