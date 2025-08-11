using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoccerBall : MonoBehaviour
{
    public float rollForce = 50f;
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
        else if (other.CompareTag("Enemy"))
        {
            Vector2 rollDirection = (transform.position - other.transform.position).normalized;
            rb.AddForce(rollDirection * rollForce * 0.5f);
        }
    }

    void FixedUpdate()
    {
        float currentSpeed = rb.velocity.magnitude;
        float rotationSpeed = currentSpeed * -100f;
        rb.angularVelocity = rotationSpeed;
    }
}
