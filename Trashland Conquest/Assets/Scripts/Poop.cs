using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Poop : Boss
{
    public float damage = 10f;
    public AudioClip hitSound; // 맞았을 때 효과음 (선택)
    public float destroyDelay = 0.1f; // 닿은 직후 바로 파괴하려면 0으로

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("플레이어가 똥에 맞았다!");

            Player player = other.GetComponent<Player>();
            if (player != null)
            {
                player.TakeDamage(damage);
                Sound.Instance?.PlaySFX(hitSound); // 효과음 선택
            }

            Destroy(gameObject, destroyDelay);
        }
    }
}
