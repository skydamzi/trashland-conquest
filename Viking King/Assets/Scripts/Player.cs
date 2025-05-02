using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Unit
{
    [Header("Direction & Rotation")]
    public bool isLookAt = true;
    private Vector2 initialDirection;
    public float spriteDefaultAngle = 90f;

    [Header("Prefabs & Spawn")]
    public GameObject bulletPrefab;
    public GameObject weaponPrefab;
    public Transform weaponSpawnPoint;

    [Header("Swing Settings")]
    public float swingAngle = 90f;
    public float swingSpeed = 4f;
    public float angleOffset = -90f;

    [Header("Jump & Physics")]
    public Rigidbody2D player_rb;
    private int jumpCount = 0;

    [Header("Neck Attack")]
    public Transform neckTransform;
    public Collider2D neckCollider;
    private bool isStretching = false;
    public bool isNeckAttacking = false;

    [Header("Fire Control")]
    private float fireRate = 0.2f;
    private float fireTimer = 0f;

    private Animator animator;

    private void Start()
    {
        initialDirection = transform.right;
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        LookAt();
        fireTimer += Time.deltaTime;

        if (Input.GetMouseButton(0) && fireTimer >= fireRate)
        {
            Fire();
            fireTimer = 0f;
        }

        if (Input.GetMouseButtonDown(1) && !isStretching)
        {
            StartCoroutine(StretchNeckAnim());
        }

        Movement();

        if (Input.GetKeyDown(KeyCode.Space) && jumpCount < 2)
        {
            player_rb.velocity = Vector2.zero;
            player_rb.AddForce(Vector2.up * 20, ForceMode2D.Impulse);
            jumpCount++;
        }
    }

    void LookAt()
    {
        if (!isLookAt || neckTransform == null) return;

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;
        neckTransform.up = (mouseWorldPos - neckTransform.position).normalized;
    }

    void Movement()
    {
        float moveX = 0f;

        if (Input.GetKey(KeyCode.A))
        {
            moveX = -1f;
            transform.localScale = new Vector3(-1f, 1f, 1f);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            moveX = 1f;
            transform.localScale = new Vector3(1f, 1f, 1f);
        }

        animator.SetBool("isRunning", moveX != 0);

        Vector2 velocity = player_rb.velocity;
        velocity.x = moveX * 6f;
        player_rb.velocity = velocity;
    }

    void Fire()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;
        Vector2 direction = (mouseWorldPos - transform.position).normalized;

        GameObject bullet = Instantiate(bulletPrefab, weaponSpawnPoint.position, Quaternion.identity);
        Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();

        if (bulletRb != null)
        {
            bulletRb.velocity = direction * 30f;

            Collider2D[] playerCols = GetComponentsInChildren<Collider2D>();
            Collider2D bulletCol = bullet.GetComponent<Collider2D>();

            foreach (var col in playerCols)
                Physics2D.IgnoreCollision(bulletCol, col);
        }
        else
        {
            Debug.LogError("bulletPrefab에 Rigidbody2D 없음!");
        }

        Destroy(bullet, 1f);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Floor"))
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y > 0.5f)
                {
                    jumpCount = 0;
                    break;
                }
            }
        }
    }

    IEnumerator StretchNeckAnim()
    {
        isStretching = true;
        isNeckAttacking = true;
        neckCollider.enabled = true;

        Vector3 originalScale = Vector3.one;
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;

        float distance = Vector2.Distance(neckTransform.position, mouseWorldPos);
        float spriteHeight = neckTransform.GetComponent<SpriteRenderer>().sprite.rect.height;
        float unitPerPixel = neckTransform.GetComponent<SpriteRenderer>().sprite.pixelsPerUnit;
        float scaleY = distance / (spriteHeight / unitPerPixel);
        Vector3 targetScale = new Vector3(1f, scaleY, 1f);

        yield return ScaleOverTime(neckTransform, originalScale, targetScale, 0.3f);
        yield return ScaleOverTime(neckTransform, targetScale, originalScale, 0.3f);

        neckTransform.localScale = originalScale;
        neckCollider.enabled = false;
        isNeckAttacking = false;
        isStretching = false;
    }

    IEnumerator ScaleOverTime(Transform tr, Vector3 from, Vector3 to, float duration)
    {
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            tr.localScale = Vector3.Lerp(from, to, timer / duration);
            yield return null;
        }
    }

    public void TakeDamage(float rawDamage)
    {
        float reducedDamage = Mathf.Max(rawDamage - armorPower, 1f);

        if (currentShield > 0)
        {
            float shieldHit = Mathf.Min(currentShield, reducedDamage);
            currentShield -= shieldHit;
            reducedDamage -= shieldHit;
        }

        currentHP = Mathf.Max(currentHP - reducedDamage, 0);
    }
}