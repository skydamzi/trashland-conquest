using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : Unit
{
    private Vector3 targetPosition;
    private bool canTakeDamage = true;
    public float damageCooldown = 1.0f;
    private bool isInvincible = false;
    public float invincibleDuration = 1f;

    public Transform destinationTarget;

    void Start()
    {
        // 목표 오브젝트가 할당되지 않았을 경우 오류 메시지를 출력합니다.
        if (destinationTarget == null)
        {
            Debug.LogError("목표 오브젝트가 할당되지 않았습니다! Inspector에서 'Destination' 오브젝트를 할당해주세요.");
        }
    }

    void Update()
    {
        // 목표 오브젝트가 있고, 아직 도착하지 않았을 때만 이동합니다.
        if (destinationTarget != null && Vector3.Distance(transform.position, destinationTarget.position) > 0.1f)
        {
            // Vector3.MoveTowards를 사용해 목표를 향해 부드럽게 이동합니다.
            transform.position = Vector3.MoveTowards(
                transform.position,
                destinationTarget.position,
                moveSpeed * Time.deltaTime
            );
        }
        else if (destinationTarget != null)
        {
            GameManager.instance.EndGame(GameManager.GameEndReason.WinByNpcArrival);
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (canTakeDamage)
        {
            float damageToTake = 0f;

            if (other.CompareTag("Enemy"))
            {
                Enemy enemy = other.GetComponent<Enemy>();
                if (enemy != null)
                {
                    damageToTake = enemy.GetBaseDamage();
                }
            }

            if (damageToTake > 0)
            {
                // 데미지 적용
                TakeDamage(damageToTake);

                // 데미지를 입힌 후 무적 및 쿨다운 코루틴 시작
                StartCoroutine(DamageCooldownCoroutine());
            }
        }
    }

    public void TakeDamage(float damage)
    {
        // 기존 코드와 동일
        float finalDamage = damage - armor;
        if (finalDamage < 0) finalDamage = 0;

        if (currentShield > 0)
        {
            currentShield -= (int)finalDamage;
            if (currentShield < 0)
            {
                currentHP += currentShield;
                currentShield = 0;
            }
        }
        else
        {
            currentHP -= finalDamage;
            Debug.Log("공격받음");
        }

        if (currentHP <= 0)
        {
            Die();
        }
    }

    IEnumerator DamageCooldownCoroutine()
    {
        canTakeDamage = false;
        // isInvincible 코루틴과 함께 실행
        StartCoroutine(TriggerInvincibility());
        yield return new WaitForSeconds(damageCooldown);
        canTakeDamage = true;
    }

    IEnumerator TriggerInvincibility()
    {
        isInvincible = true;

        float blinkInterval = 0.1f;
        float elapsed = 0f;

        while (elapsed < invincibleDuration)
        {
            SetAlpha(0.3f);
            yield return new WaitForSeconds(blinkInterval / 2f);

            SetAlpha(1f);
            yield return new WaitForSeconds(blinkInterval / 2f);

            elapsed += blinkInterval;
        }

        SetAlpha(1f);
        isInvincible = false;
    }

    void SetAlpha(float alpha)
    {
        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();
        foreach (var renderer in renderers)
        {
            Color color = renderer.color;
            color.a = alpha;
            renderer.color = color;
        }
    }

    void Die()
    {
        gameObject.SetActive(false); // 플레이어 오브젝트 비활성화

        if (GameManager.instance != null)
        {
            GameManager.instance.EndGame(GameManager.GameEndReason.LossByNpcDeath); // isWin = false (패배)
        }
    }
}
