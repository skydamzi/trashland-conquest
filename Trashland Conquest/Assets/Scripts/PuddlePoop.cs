using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuddlePoop : MonoBehaviour
{
    // ToxicWasteBoss에서 넘겨줄, 땅에 깔릴 실제 장판 프리팹
    public GameObject puddlePrefab;
    // 장판이 유지될 시간
    public float puddleDuration;

    private Rigidbody2D rb; // 충돌 감지를 위해 Rigidbody2D 컴포넌트가 필요하다.

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            // Rigidbody2D가 없으면 경고 메시지를 띄운다.
            Debug.LogWarning("PuddlePoop: 이 오브젝트에 Rigidbody2D가 없습니다! 충돌 감지가 제대로 안 될 수 있습니다. (PuddlePoop 프리팹을 확인하세요)");
        }
    }

    // 이 똥 오브젝트가 다른 오브젝트와 '물리적으로' 충돌했을 때 호출된다.
    // Collider2D의 Is Trigger가 '해제'되어 있어야 한다.
    void OnCollisionEnter2D(Collision2D collision)
    {
        // Debug.Log($"PuddlePoop: 충돌 발생! 충돌 상대: {collision.gameObject.name} (태그: {collision.gameObject.tag})");

        // 충돌한 오브젝트의 태그가 "Floor"인지 확인한다.
        if (collision.gameObject.CompareTag("Floor"))
        {
            Debug.Log($"PuddlePoop: Floor에 닿았다! 장판 생성 시작!");

            // puddlePrefab이 제대로 할당되어 있는지 확인
            if (puddlePrefab != null)
            {
                // 충돌 지점의 위치를 정확히 가져와서 장판을 생성한다.
                // 보통 2D에서 y축 보정을 위해 충돌 지점의 x는 똥의 현재 x를 사용하고, y는 충돌 지점의 y를 사용하는 경우가 많다.
                // 장판의 피벗(pivot) 위치에 따라 조정이 필요할 수 있다.
                Vector2 spawnPos = new Vector2(transform.position.x, collision.contacts[0].point.y);

                // 장판 프리팹을 충돌 지점에 생성한다.
                GameObject puddle = Instantiate(puddlePrefab, spawnPos, Quaternion.identity);
                Debug.Log($"PuddlePoop: 장판 '{puddle.name}'을 {spawnPos}에 생성했다! {puddleDuration}초 후에 사라진다.");

                // 생성된 장판은 지정된 puddleDuration 시간 후에 자동으로 삭제되도록 설정한다.
                Destroy(puddle, puddleDuration);
            }
            else
            {
                Debug.LogWarning("PuddlePoop: PuddlePrefab이 할당되지 않았습니다! 장판을 생성할 수 없습니다. (PuddlePoop 스크립트 인스펙터를 확인하세요)");
            }

            // 장판을 생성했으니, 이 '장판 똥' 오브젝트 자신은 바로 삭제한다.
            Destroy(gameObject);
        }
        else
        {
            // Floor가 아닌 다른 오브젝트에 닿았을 경우, 필요에 따라 추가적인 로직을 넣을 수 있다.
            // Debug.Log($"PuddlePoop: Floor가 아닌 '{collision.gameObject.name}'에 닿았음. 이 똥은 계속 이동합니다.");
        }
    }

    // 안전 장치: 만약 어떤 이유로든 땅에 닿지 못하고 너무 오래 떠있다면, 일정 시간(예: 10초) 후에 자동으로 삭제된다.
    void Start()
    {
        Destroy(gameObject, 10f); // 10초 후 자동 삭제 예약
    }
}