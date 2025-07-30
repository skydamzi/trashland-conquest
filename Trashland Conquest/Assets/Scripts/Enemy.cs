using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : Unit
{
    public GameObject experienceGemPrefab; // 경험치 보석 프리팹
    public int experienceAmount = 1; // 이 적이 죽었을 때 줄 경험치
    public GameObject healthBarPrefab; // 체력바 UI 프리팹 (Canvas)
    private Image healthBarFillImage; // 체력바의 채워지는 부분 Image 컴포넌트

    private Transform playerTransform; // 플레이어의 Transform
    
    // Unit 클래스의 Awake()나 Start()가 호출된 후 이 Awake()가 호출됩니다.
    void Awake()
    {
        if (healthBarPrefab != null)
        {
            GameObject healthBarInstance = Instantiate(healthBarPrefab, transform);
            // 체력바를 적 머리 위로 살짝 올립니다.
            // 필요에 따라 Offset을 조절하세요.
            healthBarInstance.transform.localPosition = new Vector3(0, 0.3f, 0);

            // HealthBarFill Image 컴포넌트를 찾아서 할당합니다.
            // HealthBarFill이 Canvas > HealthBarBackground 아래에 있다고 가정
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
        // 씬에서 "Player" 태그를 가진 오브젝트를 찾아 Transform을 가져옵니다.
        // 적이 생성되자마자 플레이어를 찾아 추격을 시작합니다.
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogError("Player 오브젝트를 찾을 수 없습니다. 'Player' 태그가 있는지 확인해주세요.");
            // 플레이어가 없으면 적이 할 일이 없으므로 스스로 파괴할 수도 있습니다.
            // Destroy(gameObject);
        }
        UpdateHealthBar();
    }

    void Update()
    {
        // 플레이어 트랜스폼이 유효한지 확인하고 추격 로직 실행
        if (playerTransform != null)
        {
            // 플레이어 방향으로 벡터 계산 및 정규화
            // 감지 범위 없이, 항상 플레이어를 향해 이동합니다.
            Vector3 direction = (playerTransform.position - transform.position).normalized;
            // 이동 속도와 Time.deltaTime을 곱하여 프레임에 독립적인 이동
            transform.position += direction * moveSpeed * Time.deltaTime;
        }
    }

    private void UpdateHealthBar()
    {
        if (healthBarFillImage != null)
        {
            healthBarFillImage.fillAmount = currentHP / maxHP;
        }

        if (currentHP <= 0 && healthBarFillImage != null)
        {
            healthBarFillImage.transform.parent.gameObject.SetActive(false); // HealthBarBackground를 비활성화
        }
    }

    // 다른 오브젝트와 충돌했을 때 (예: 플레이어의 공격)
    void OnTriggerEnter2D(Collider2D other)
    {
        // 플레이어의 공격 태그와 비교
        if (other.CompareTag("Bullet"))
        {
            
            TakeDamage(10);

            // 만약 플레이어의 공격이 투사체라면 파괴합니다.
            Destroy(other.gameObject);
        }
        // TODO: 플레이어와 닿았을 때 플레이어에게 데미지 주는 로직 추가 가능
        else if (other.CompareTag("Player"))
        {
            Player player = other.GetComponent<Player>(); // 플레이어 스크립트 가져오기
            if (player != null)
            {
                player.TakeDamage(GetBaseDamage()); // 적의 공격력으로 플레이어에게 데미지
            }
        }
    }

    public void TakeDamage(float damage)
    {
        float finalDamage = damage - armor; // 방어력 적용
        if (finalDamage < 0) finalDamage = 0; // 최소 데미지는 0

        if (currentShield > 0)
        {
            currentShield -= (int)finalDamage;
            if (currentShield < 0)
            {
                currentHP += currentShield; // 남은 데미지를 체력에 적용 (currentShield가 음수가 됨)
                currentShield = 0;
            }
        }
        else
        {
            currentHP -= finalDamage;
        }

        if (currentHP <= 0)
        {
            Die(); // 체력이 0 이하면 Die() 호출
        }
        UpdateHealthBar();
    }

    // Unit 클래스에서 선언된 추상 Die() 함수를 구현합니다.
    void Die()
    {
        Debug.Log($"{unitName} 사망!");

        // 경험치 보석 드랍
        if (experienceGemPrefab != null)
        {
            // 경험치 보석을 현재 적의 위치에 생성합니다.
            GameObject gem = Instantiate(experienceGemPrefab, transform.position, Quaternion.identity);

            // 경험치 보석 스크립트에 경험치 양을 설정합니다.
            ExperienceGem expGem = gem.GetComponent<ExperienceGem>();
            if (expGem != null)
            {
                expGem.SetExperience(experienceAmount);
            }
        }

        // 적 오브젝트를 파괴합니다.
        Destroy(gameObject);
    }
}