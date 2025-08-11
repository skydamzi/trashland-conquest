using System.Collections;
using System.Collections.Generic;
// AttackHitbox.cs (새로 만들어서 복싱 글러브에 붙일 스크립트)

using UnityEngine;


public class PlayerAttackHitbox : MonoBehaviour, IDamageDealer
{
    public AudioClip glove_punchSound; // 복싱 글러브 펀치 사운드
    private Player player; // 공격 주체인 플레이어 스크립트

    private void Awake()
    {
        player = GetComponentInParent<Player>();
    }

    public float GetDamage()
    {
        return player.finalPunchDamage; // 플레이어 스크립트에서 데미지 값을 가져옴
    }

    public GameObject GetOwner()
    {
        return player.gameObject; // 플레이어 오브젝트를 반환
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 내 공격이 나 자신을 때리지 않도록
        if (other.gameObject == GetOwner())
        {
            return;
        }

        IDamageable damageable = other.GetComponent<IDamageable>();
        if (damageable != null)
        {
            Sound.Instance.PlaySFX(glove_punchSound);
            damageable.TakeDamage(GetDamage());
        }
    }
}