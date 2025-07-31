using System.Collections;
using System.Collections.Generic;
// AttackHitbox.cs (새로 만들어서 복싱 글러브에 붙일 스크립트)

using UnityEngine;

public class AttackHitbox : MonoBehaviour
{
    
    // 이 공격이 주는 데미지 양
    private void OnTriggerEnter2D(Collider2D other)
    {
        Player player = GetComponentInParent<Player>();
        // 내 공격이 몬스터에 닿았을 때
        if (other.CompareTag("Boss") || other.CompareTag("Enemy"))
        {
            // 몬스터의 TakeDamage 함수를 찾아서 호출한다.
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(player.GetMeleeDamage());
                Sound.Instance.PlaySFX(player.glove_punchSound);
                return;
            }

            Boss boss = other.GetComponent<Boss>();
            if (boss != null)
            {
                boss.TakeDamage(player.GetMeleeDamage());
                Sound.Instance.PlaySFX(player.glove_punchSound);
                return;
            }
        }
    }
}
