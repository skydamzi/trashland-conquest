using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Unit
{
    [Header("Movement Settings")]
    public float detectionRange = 7f;
    public float attackRange = 1.5f;
    public Transform[] waypoints;
    private int currentWaypointIndex = 0;

    [Header("Attack Settings")]
    public float attackCooldown = 2f;
    private float attackTimer;

    private Transform playerTR;
    public Player player;
    public AudioClip glove_punchSound;

    private bool isGrounded = false;
    private bool isChasingPlayer = false;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    [Header("Layer Settings")]
    public string groundTag = "Floor";

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
            Debug.LogWarning("���� ������ �������� �ʾ���!");
        }
    }

    void Update()
    {
        attackTimer -= Time.deltaTime;
        float distanceToPlayer = Vector2.Distance(transform.position, playerTR.position);

        if (distanceToPlayer <= detectionRange && isGrounded)
        {
            isChasingPlayer = true;
            spriteRenderer.color = Color.red;  // �÷��̾� ���� �� ������
        }
        else
        {
            isChasingPlayer = false;
            spriteRenderer.color = originalColor;  // ���� �������� ����
        }
    }

    void FixedUpdate()
    {
        if (!isGrounded) return; // ���� �پ����� ������ �̵����� ����

        if (isChasingPlayer)
        {
            MoveTowardsPlayer();
        }
        else
        {
            Patrol();
        }
    }

    void Patrol()
    {
        if (waypoints.Length == 0) return;

        Transform targetWaypoint = waypoints[currentWaypointIndex];
        float distanceToWaypoint = Vector2.Distance(transform.position, targetWaypoint.position);

        if (distanceToWaypoint < 0.2f)
        {
            // ���� �������� �̵�
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
            isGrounded = true;
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.CompareTag(groundTag))
        {
            isGrounded = false;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Bullet"))
        {
            TakeDamage(player.GetBaseDamage());
            Destroy(other.gameObject);
        }
    }

    public void TakeDamage(float damage)
    {
        currentHP -= damage;
        if (currentHP <= 0) Die();
    }

    void Die()
    {
        Destroy(gameObject);
    }
}
