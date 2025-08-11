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
        // ��ǥ ������Ʈ�� �Ҵ���� �ʾ��� ��� ���� �޽����� ����մϴ�.
        if (destinationTarget == null)
        {
            Debug.LogError("��ǥ ������Ʈ�� �Ҵ���� �ʾҽ��ϴ�! Inspector���� 'Destination' ������Ʈ�� �Ҵ����ּ���.");
        }
    }

    void Update()
    {
        // ��ǥ ������Ʈ�� �ְ�, ���� �������� �ʾ��� ���� �̵��մϴ�.
        if (destinationTarget != null && Vector3.Distance(transform.position, destinationTarget.position) > 0.1f)
        {
            // Vector3.MoveTowards�� ����� ��ǥ�� ���� �ε巴�� �̵��մϴ�.
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
                // ������ ����
                TakeDamage(damageToTake);

                // �������� ���� �� ���� �� ��ٿ� �ڷ�ƾ ����
                StartCoroutine(DamageCooldownCoroutine());
            }
        }
    }

    public void TakeDamage(float damage)
    {
        if (isInvincible) return;
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
            Debug.Log("���ݹ���");
        }

        if (currentHP <= 0)
        {
            Die();
        }
    }

    IEnumerator DamageCooldownCoroutine()
    {
        canTakeDamage = false;
        // isInvincible �ڷ�ƾ�� �Բ� ����
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
        gameObject.SetActive(false); // �÷��̾� ������Ʈ ��Ȱ��ȭ

        if (GameManager.instance != null)
        {
            GameManager.instance.EndGame(GameManager.GameEndReason.LossByNpcDeath); // isWin = false (�й�)
        }
    }
}
