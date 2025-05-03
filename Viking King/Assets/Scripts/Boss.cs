using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Boss : Unit
{
    public GameObject playerDamage;
    public Text damageText;
    void Start()
    {
        currentHP = maxHP;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.name.Contains("BoxingGlove")) // 또는 태그 사용
        {
            Player player = other.GetComponentInParent<Player>();
            if (player != null && player.isNeckAttacking)
            {
                TakeDamage(player.TotalAttack());
                Debug.Log("보스: 글러브 맞고 데미지 받음");
            }
            else
            {
                Debug.Log("보스: 닿긴 했지만 공격 상태 아님");
            }
        }
        if (other.CompareTag("Bullet"))
        {
            float damage = playerDamage.GetComponent<Player>().TotalAttack();
            TakeDamage(damage); // 맞으면 10 깎기
            Destroy(other.gameObject);
        }
    }

    public void TakeDamage(float damage)
    {
        damageText.text = damage.ToString();
        currentHP -= damage;
        
        Debug.Log("Boss HP: " + currentHP);

        if (currentHP <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("Boss Dead!");
        Destroy(gameObject);
    }
}
