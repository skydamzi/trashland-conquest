using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : Unit
{
    // Unit 클래스의 변수들과 메서드들은 Unit.cs에 이미 구현되어 있다고 가정합니다.
    protected Player player;
    public GameObject experienceGemPrefab;
    public int experienceAmount = 1;
    public GameObject healthBarPrefab;
    private Image healthBarFillImage;

    private Transform targetTransform; // 추격 대상의 Transform

    // Awake()는 기존 코드와 동일합니다.
    void Awake()
    {
        if (healthBarPrefab != null)
        {
            GameObject healthBarInstance = Instantiate(healthBarPrefab, transform);
            healthBarInstance.transform.localPosition = new Vector3(0, 0.3f, 0);

            Transform fillTransform = healthBarInstance.transform.Find("HealthBarBackground/HealthBar");
            if (fillTransform != null)
            {
                healthBarFillImage = fillTransform.GetComponent<Image>();
            }
            else
            {
                Debug.LogError("HealthBarFill Image를 찾을 수 없습니다. 경로를 확인해주세요.");
            }
        }
        else
        {
            Debug.LogError("Health Bar Prefab이 할당되지 않았습니다!");
        }
    }

    void Start()
    {
        // 게임 시작 시 NPC를 먼저 찾고, 없으면 플레이어를 찾습니다.
        FindTarget();
        UpdateHealthBar();
    }

    void Update()
    {
        // 타겟이 있으면 추격 로직을 실행합니다.
        if (targetTransform != null)
        {
            Vector3 direction = (targetTransform.position - transform.position).normalized;
            transform.position += direction * moveSpeed * Time.deltaTime;
        }
    }

    /// <summary>
    /// NPC를 먼저 찾고, 없으면 플레이어를 타겟으로 설정하는 함수.
    /// </summary>
    void FindTarget()
    {
        // NPC를 먼저 찾습니다.
        GameObject[] npcs = GameObject.FindGameObjectsWithTag("NPC");

        if (npcs.Length > 0)
        {
            // NPC가 존재하면, 그중에서 가장 가까운 NPC를 타겟으로 설정합니다.
            targetTransform = GetNearestTarget(npcs);
        }
        else
        {
            // NPC가 없으면 플레이어를 찾습니다.
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                targetTransform = playerObj.transform;
            }
        }
    }

    private Transform GetNearestTarget(GameObject[] targets)
    {
        Transform nearest = null;
        float minDistance = Mathf.Infinity;

        foreach (GameObject target in targets)
        {
            if (target == null) continue;

            float distance = Vector3.Distance(transform.position, target.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = target.transform;
            }
        }
        return nearest;
    }

    // 체력바 UI를 업데이트하는 함수
    void UpdateHealthBar()
    {
        if (healthBarFillImage != null)
        {
            // Unit 클래스의 currentHP와 maxHP를 사용
            healthBarFillImage.fillAmount = currentHP / maxHP;
        }

        if (currentHP <= 0 && healthBarFillImage != null)
        {
            healthBarFillImage.transform.parent.gameObject.SetActive(false);
        }
    }

    // 다른 오브젝트와 충돌했을 때 (예: 플레이어의 공격)
    void OnTriggerEnter2D(Collider2D other)
    {
        // 기존 코드와 동일
        player = FindObjectOfType<Player>();
        if (other.CompareTag("Bullet"))
        {
            Debug.Log($"[Enemy] {unitName}이(가) 플레이어의 공격을 받음!");
            TakeDamage(player.GetBaseDamage());
            Destroy(other.gameObject);
        }
        else if (other.CompareTag("BoxingGlove"))
        {
            TakeDamage(player.GetMeleeDamage());
        }
    }

    // 데미지를 입는 함수
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
        }

        if (currentHP <= 0)
        {
            Die();
        }
        UpdateHealthBar();
    }

    // 사망 시 호출되는 함수
    void Die()
    {
        Debug.Log($"{unitName} 사망!");

        if (experienceGemPrefab != null)
        {
            GameObject gem = Instantiate(experienceGemPrefab, transform.position, Quaternion.identity);

            ExperienceGem expGem = gem.GetComponent<ExperienceGem>();
            if (expGem != null)
            {
                expGem.SetExperience(experienceAmount);
            }
        }

        // 경험치 보석이 드랍된 후 2초 뒤에 적 오브젝트를 파괴하도록 코루틴을 사용
        // 코루틴 사용 예시
        // StartCoroutine(DestroyAfterDelay(2f)); 

        Destroy(gameObject);
    }
}