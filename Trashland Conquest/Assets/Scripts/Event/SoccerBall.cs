using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoccerBall : MonoBehaviour
{
    public float rollForce = 50f; // 공 굴러가는 힘, 적절히 조절해
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("BoxingGlove"))
        {
            Vector2 rollDirection = (transform.position - other.transform.position).normalized;
            rb.AddForce(rollDirection * rollForce);
        }
    }

    void FixedUpdate()
    {
        // 공의 현재 속도를 가져옴
        float currentSpeed = rb.velocity.magnitude;
        
        // 공의 속도에 비례해서 회전 속도를 계산
        // -100f는 회전 속도 계수. 이 값을 조절해서 굴러가는 회전 속도를 바꿔 봐.
        // 마이너스(-)를 붙여야 공이 앞으로 굴러가는 방향과 회전 방향이 맞음.
        float rotationSpeed = currentSpeed * -100f;
        
        // 계산된 회전 속도를 Rigidbody에 적용
        rb.angularVelocity = rotationSpeed;
    }
}
