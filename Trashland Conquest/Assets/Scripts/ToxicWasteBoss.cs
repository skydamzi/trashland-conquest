using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToxicWasteBoss : Boss
{
    [Header("Poop Rain Settings")]
    public GameObject gooBombPrefab;          // 떨어지는 똥 프리팹
    public Transform bombArcPoint;            // 던지는 기준점 (보스 머리 위쪽)

    public float poopInterval = 0.05f;        // 똥 떨어지는 간격
    public float arcHeight = 15f;             // 똥 곡사 높이
    public float dropRangeX = 0.5f;           // 좌우 범위 (보스 기준)
    public float dropOffsetY = 0f;            // 드롭 위치 y 오프셋
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    public Color maxRedColor = new Color(1f, 0.3f, 0.3f, 1f); // 완전 빨간 느낌
    
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
        // **여기서 PoopRoutine을 바로 시작하지 않는다. OnEnable에서 시작할 거임.**
        // StartCoroutine(PoopRoutine()); // 이 줄은 제거!
    }

    // **추가: 오브젝트가 활성화될 때마다 호출되는 함수**
    void OnEnable()
    {
        Debug.Log("ToxicWasteBoss: OnEnable 호출됨. 패턴 코루틴 시작 시도.");

        // _canAct 플래그를 true로 설정 (이제 행동할 수 있다고 알림)
        _canAct = true; 

        // 만약 poopRoutine이 이미 실행 중이라면 중복 실행 방지를 위해 먼저 중지 (선택 사항)
        if (poopRoutine != null)
        {
            StopCoroutine(poopRoutine);
        }
        
        // 패턴 코루틴 시작!
        poopRoutine = StartCoroutine(PoopRoutine());
    }

    // **추가: 오브젝트가 비활성화될 때 호출되는 함수**
    void OnDisable()
    {
        Debug.Log("ToxicWasteBoss: OnDisable 호출됨. 패턴 코루틴 중지.");
        _canAct = false; // 행동 불가로 설정

        // 오브젝트가 비활성화되면 코루틴도 함께 중지
        if (poopRoutine != null)
        {
            StopCoroutine(poopRoutine);
            poopRoutine = null; // null로 만들어서 다음에 다시 시작할 수 있게 함
        }
        if (activePoopCoroutine != null) // 중첩된 코루틴도 멈춰야 함
        {
            StopCoroutine(activePoopCoroutine);
            activePoopCoroutine = null;
        }
    }


    Coroutine activePoopCoroutine;
    IEnumerator PoopRoutineContinuous()
    {
        while (true)
        {
            if (!_canAct) // 패턴 도중 _canAct가 false면 잠시 멈춤
            {
                yield return null; 
                continue; // 아래 로직 실행 안 하고 다음 프레임에 다시 체크
            }
            DropPoop();
            yield return new WaitForSeconds(poopInterval);      // 텀 주고
        }
    }

    private Coroutine poopRoutine;
    IEnumerator PoopRoutine()
    {
        while (true)
        {
            if (!_canAct) // 패턴 도중 _canAct가 false면 잠시 멈춤
            {
                yield return null; 
                continue; // 아래 로직 실행 안 하고 다음 프레임에 다시 체크
            }

            float preWait = 3f;
            float poopDuringPrewaitInterval = 0.5f;
            float poopTimer = 0f;

            // 1. 부글부글 대기하면서 천천히 빨개짐
            for (float t = 0f; t < preWait; t += Time.deltaTime)
            {
                if (!_canAct) { yield return null; break; } // 중간에도 _canAct 체크
                
                float lerpFactor = t / preWait;
                spriteRenderer.color = Color.Lerp(originalColor, maxRedColor, lerpFactor);
                poopTimer += Time.deltaTime;
                if (poopTimer >= poopDuringPrewaitInterval)
                {
                    DropPoop();
                    poopTimer = 0f;
                }
                yield return null;
            }

            if (!_canAct) { spriteRenderer.color = originalColor; yield break; } // 코루틴 탈출

            // 2. 대분출 → 완전 빨간색
            spriteRenderer.color = maxRedColor;
            Debug.Log("대분출!!!");

            activePoopCoroutine = StartCoroutine(PoopRoutineContinuous());
            yield return new WaitForSeconds(2f);
            if (!_canAct) { StopCoroutine(activePoopCoroutine); spriteRenderer.color = originalColor; yield break; } // 코루틴 탈출
            StopCoroutine(activePoopCoroutine);

            // 3. 원래 색으로 되돌아가기
            float recoverDuration = 0.5f;
            for (float t = 0f; t < recoverDuration; t += Time.deltaTime)
            {
                if (!_canAct) { yield return null; break; } // 중간에도 _canAct 체크
                
                float lerpFactor = t / recoverDuration;
                spriteRenderer.color = Color.Lerp(maxRedColor, originalColor, lerpFactor);
                yield return null;
            }
            spriteRenderer.color = originalColor;

            if (!_canAct) { yield break; } // 코루틴 탈출

            // 4. 나머지 대기 시간
            float totalCooldown = 5f;
            float remainingDelay = totalCooldown - (preWait + 2f + recoverDuration); // recoverDuration도 빼줘야 정확
            if (remainingDelay > 0f)
                yield return new WaitForSeconds(remainingDelay);
        }
    }

    void DropPoop()
    {
        if (bombArcPoint == null)
        {
            Debug.LogWarning("bombArcPoint 안 넣었음!");
            return;
        }
        if (!_canAct) return; // DropPoop도 _canAct가 false면 실행 안 함 (선택 사항이지만 안전)

        Vector2 targetPos = GetRandomDropPosition();
        GameObject bomb = Instantiate(gooBombPrefab, bombArcPoint.position, Quaternion.identity);

        Rigidbody2D rb = bomb.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.AddForce(CalculateArcForce(bombArcPoint.position, targetPos, arcHeight), ForceMode2D.Impulse);
        }
        Destroy(bomb, 3f); // 5초 후 폭탄 삭제
    }

    Vector2 GetRandomDropPosition()
    {
        float randomX = bombArcPoint.position.x + Random.Range(-dropRangeX / 2f, dropRangeX / 2f);
        float y = transform.position.y + dropOffsetY;
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

        float xForceMultiplier = 0.25f;
        Vector2 velocityXZ = (displacementXZ / totalTime) * xForceMultiplier;

        return velocityXZ + velocityY;
    }
}