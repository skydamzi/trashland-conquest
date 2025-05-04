using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Boss : Unit
{
    public GameObject playerObject;
    private Player player;
    public Text damageText;
    private bool hasBeenHit = false;
    public AudioClip glove_punchSound;

    void Start()
    {
        if (playerObject != null)
            player = playerObject.GetComponent<Player>();
        currentHP = maxHP;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.name.Contains("BoxingGlove"))
        {
            if (player != null && player.isNeckAttacking && !hasBeenHit && player.isPunchFrame)
            {
                hasBeenHit = true;
                SoundManager.Instance.PlaySFX(glove_punchSound);
                TakeDamage(player.TotalAttack());
                Debug.Log("보스: 글러브 맞고 데미지 받음");
            }
            else
                Debug.Log("보스: 닿긴 했지만 공격 상태 아님");
        }

        else if (other.CompareTag("Bullet"))
        {
            if (player != null)
                TakeDamage(player.TotalAttack());
            else
                Debug.LogWarning("총알 충돌: Player 참조 없음");

            Destroy(other.gameObject);
        }
    }
    public void ResetHitFlag()
    {
        hasBeenHit = false;
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
        if (player != null)
        {
            player.GainEXP(currentEXP);
        }
        Destroy(gameObject);
    }
}
