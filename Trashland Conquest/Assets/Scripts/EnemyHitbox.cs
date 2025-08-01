using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHitbox : MonoBehaviour, IDamageDealer
{
    public AudioClip glove_punchSound; // 복싱 글러브 펀치 사운드
    private Enemy enemy; // 공격 주체인 적 스크립트

    private void Awake()
    {
        enemy = GetComponentInParent<Enemy>();
    }

    public float GetDamage()
    {
        return enemy.GetBaseDamage(); // 적 스크립트에서 데미지 값을 가져옴
    }

    public GameObject GetOwner()
    {
        return enemy.gameObject; // 적 오브젝트를 반환
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        // 내 공격이 나 자신을 때리지 않도록
        if (other.gameObject == GetOwner())
        {
            return;
        }

        IDamageable damageable = other.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(GetDamage());
        }
    }
}