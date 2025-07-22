using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuddlePoop : MonoBehaviour
{
    public GameObject puddlePrefab; // 생성할 장판 프리팹
    public float puddleDuration = 5f; // 장판 지속 시간

    private bool hasHitGround = false; // 이미 땅에 닿았는지 체크 (중복 생성 방지)

    void OnCollisionEnter2D(Collision2D collision)
    {
        // 땅(Layer 이름을 "Ground" 등으로 설정했다고 가정)에 닿았을 때만 장판 생성
        // 또는 특정 태그(Tag를 "Ground" 등으로 설정)를 가진 오브젝트에 닿았을 때
        if (collision.gameObject.CompareTag("Floor") && !hasHitGround) // 태그 비교
        {
            hasHitGround = true; // 땅에 닿았음을 표시

            // 장판 생성
            GameObject puddle = Instantiate(puddlePrefab, transform.position, Quaternion.identity);
            
            // 장판 지속 시간 후 파괴
            Destroy(puddle, puddleDuration);

            // 자신(똥 프리팹)은 바로 파괴
            Destroy(gameObject);
        }
    }

    // 만약 땅에 닿지 않고 맵 밖으로 나가버리면 스스로 파괴되도록
    // (선택 사항) OnBecameInvisible이나 일정 시간 후 파괴 로직 추가 가능
    void Update()
    {
        // 만약 똥이 너무 오래 날아다니면 (예: 10초 이상) 스스로 파괴
        // Destroy(gameObject, 10f); // Awake나 Start에서 해도 됨
    }
}