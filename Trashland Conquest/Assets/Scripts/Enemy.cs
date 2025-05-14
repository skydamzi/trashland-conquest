using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Unit
{
    [Header("Movement Settings")]
    public float detectionRange = 7f;            // 플레이어 탐지 범위
    public float attackRange = 1.5f;             // 공격 가능 범위
    public Transform[] waypoints;                // 순찰 경로 지점들
    private int currentWaypointIndex = 0;        // 현재 순찰 지점 인덱스

    [Header("Attack Settings")]
    public float attackCooldown = 2f;            // 공격 쿨타임
    private float attackTimer;

    private Transform playerTR;                  // 플레이어 Transform
    public Player player;                        // 플레이어 스크립트 참조
    public AudioClip glove_punchSound;           // 공격 사운드

    private bool isGrounded = false;             // 땅에 닿아 있는지 여부
    private bool isChasingPlayer = false;        // 플레이어 추적 여부

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    [Header("Layer Settings")]
    public string groundTag = "Floor";           // 바닥으로 인식할 태그명

    void Start()
    {
        playerTR = GameObject.FindGameObjectWithTag("Player").transform;
        attackTimer = attackCooldown;
        player = FindObjectOfType<Player>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;

        if (waypoints.Length == 0)
        {
            Debug.LogWarning("순찰 지점이 설정되어 있지 않습니다!");
        }
    }

    void Update()
    {
        attackTimer -= Time.deltaTime;
        float distanceToPlayer = Vector2.Distance(transform.position, playerTR.position);

        if (distanceToPlayer <= detectionRange && isGrounded)
        {
            isChasingPlayer = true;
            spriteRenderer.color = Color.red; // 플레이어를 발견하면 빨간색으로 변경
        }
        else
        {
            isChasingPlayer = false;
            spriteRenderer.color = originalColor; // 원래 색상으로 복귀
        }
    }

    void FixedUpdate()
    {
        if (!isGrounded) return; // 땅에 없으면 이동 X

        if (isChasingPlayer)
        {
            MoveTowardsPlayer(); // 플레이어 추적
        }
        else
        {
            Patrol(); // 순찰 행동
        }
    }

    void Patrol()
    {
        if (waypoints.Length == 0) return;

        Transform targetWaypoint = waypoints[currentWaypointIndex];
        float distanceToWaypoint = Vector2.Distance(transform.position, targetWaypoint.position);

        if (distanceToWaypoint < 0.2f)
        {
            // 다음 순찰 지점으로 이동
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        }

        Vector2 direction = (targetWaypoint.position - transform.position).normalized;
        rb.velocity = new Vector2(direction.x * moveSpeed, rb.velocity.y);
    }

    void MoveTowardsPlayer()
    {
        Vector2 direction = (playerTR.position - transform.position).normalized;
        rb.velocity = new Vector2(direction.x * moveSpeed * 1.5f, rb.velocity.y);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.collider.CompareTag(groundTag))
        {
            isGrounded = true; // 바닥에 닿아 있음
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.CompareTag(groundTag))
        {
            isGrounded = false; // 바닥에서 떨어짐
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Bullet"))
        {
            TakeDamage(player.GetBaseDamage()); // 총알 맞으면 데미지 입음
            Destroy(other.gameObject);          // 총알 제거
        }
    }

    public void TakeDamage(float damage)
    {
        currentHP -= damage;
        if (currentHP <= 0) Die();
    }

    void Die()
    {
        Destroy(gameObject); // 적 제거
    }
}
