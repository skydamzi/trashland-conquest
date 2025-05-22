using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToxicWasteBoss : Boss
{
    [Header("Poop Rain Settings")]
    public GameObject gooBombPrefab;         // 떨어지는 똥 프리팹
    public Transform bombArcPoint;           // 던지는 기준점 (보스 머리 위쪽)

    public float poopInterval = 0.05f;        // 똥 떨어지는 간격
    public float arcHeight = 8f;             // 똥 곡사 높이
    public float dropRangeX = 0.5f;          // 좌우 범위 (보스 기준)
    public float dropOffsetY = 0f;           // 드롭 위치 y 오프셋
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    public Color maxRedColor = new Color(1f, 0.3f, 0.3f, 1f); // 완전 빨간 느낌
    

    void Start()
    {
        currentHP = maxHP;

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;

        poopRoutine = StartCoroutine(PoopRoutine());
    }


    Coroutine activePoopCoroutine;
    IEnumerator PoopRoutineContinuous()
    {
        while (true)
        {
            DropPoop();
            yield return new WaitForSeconds(poopInterval);      // 텀 주고
        }
    }
    private Coroutine poopRoutine;
    IEnumerator PoopRoutine()
    {
        while (true)
        {
            float preWait = 3f;
            float poopDuringPrewaitInterval = 0.5f;
            float poopTimer = 0f;

            // 1. 부글부글 대기하면서 천천히 빨개짐
            for (float t = 0f; t < preWait; t += Time.deltaTime)
            {
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

            // 2. 대분출 → 완전 빨간색
                spriteRenderer.color = maxRedColor;
            Debug.Log("대분출!!!");

            activePoopCoroutine = StartCoroutine(PoopRoutineContinuous());
            yield return new WaitForSeconds(2f);
            StopCoroutine(activePoopCoroutine);

            // 3. 원래 색으로 되돌아가기
            float recoverDuration = 0.5f;
            for (float t = 0f; t < recoverDuration; t += Time.deltaTime)
            {
                float lerpFactor = t / recoverDuration;
                spriteRenderer.color = Color.Lerp(maxRedColor, originalColor, lerpFactor);
                yield return null;
            }
            spriteRenderer.color = originalColor;

            // 4. 나머지 대기 시간
            float totalCooldown = 5f;
            float remainingDelay = totalCooldown - (preWait + 2f);
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