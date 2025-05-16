using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Boss : Unit
{
   // public GameObject playerObject;
    private Player player;
    public Text damageText;
    public AudioClip glove_punchSound; 
    public DPSMeter dpsMeter;

    void Start()
    {
        //    if (playerObject != null)
        //       player = playerObject.GetComponent<Player>();
        player = FindObjectOfType<Player>();
        currentHP = maxHP;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("BoxingGlove"))
        {
            if (player != null && player.isNeckAttacking && !player.hitEnemiesThisAttack.Contains(gameObject))
            {
                player.hitEnemiesThisAttack.Add(gameObject);
                Sound.Instance.PlaySFX(glove_punchSound);
                TakeDamage(player.GetMeleeDamage());
                Debug.Log("보스: 글러브 맞고 데미지 받음");
            }
            else
                Debug.Log("보스: 닿긴 했지만 공격 상태 아님");
        }

        else if (other.CompareTag("Bullet"))
        {
            TakeDamage(player.GetBaseDamage());

            Destroy(other.gameObject);
        }
    }


    public void TakeDamage(float damage)
    {
        damageText.text = damage.ToString();
        currentHP -= damage;
        Debug.Log("Boss HP: " + currentHP);

        if (dpsMeter != null)
            dpsMeter.ReportDamage(damage);
        if (currentHP <= 0)
            Die();

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
