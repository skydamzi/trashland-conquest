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
    private bool isGrounded = false;
    private bool wasGroundedLastFrame = false;

    [Header("Neck Attack")]
    public Transform neckTransform;
    public Collider2D neckCollider;
    private bool isStretching = false;
    public bool isNeckAttacking = false;

    [Header("Fire Control")]
    private float fireRate = 0.2f;
    private float fireTimer = 0f;

    [Header("Neck Stretch FX")]
    private float stretchTimer = 0f;

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
        stretchTimer += Time.deltaTime;
        animator.SetFloat("verticalVelocity", player_rb.velocity.y);
        animator.SetBool("isJumping", !isGrounded);
        if (!isGrounded && wasGroundedLastFrame && jumpCount == 0)
        {
            jumpCount = 1;
        }

        wasGroundedLastFrame = isGrounded;
        if (Input.GetMouseButton(0) && fireTimer >= fireRate)
        {
            Fire();
            stretchTimer = 0f;
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

            if (jumpCount == 0)
                player_rb.AddForce(Vector2.up * 20f, ForceMode2D.Impulse);
            else if (jumpCount == 1)
                player_rb.AddForce(Vector2.up * 15f, ForceMode2D.Impulse);

            jumpCount++;
            animator.SetInteger("jumpCount", jumpCount);
        }
    }
    void LookAt()
    {
        if (!isLookAt || neckTransform == null) return;

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;
        Vector3 direction = (mouseWorldPos - neckTransform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(Vector3.forward, direction);
        neckTransform.rotation = Quaternion.Lerp(neckTransform.rotation, targetRotation, Time.deltaTime * 20f);
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

        float tiltAngle = 10f;
        float targetZ;

        if (!isGrounded)
            targetZ = 0f;
        else
        {
            if (moveX > 0)
                targetZ = -tiltAngle;
            else if (moveX < 0)
                targetZ = tiltAngle;
            else
                targetZ = 0f;
        }

        transform.rotation = Quaternion.Euler(0, 0, targetZ);
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
        Destroy(bullet, 1f);
        StartCoroutine(QuickStretch());
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Floor"))
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y > 0.5f)
                {
                    isGrounded = true;
                    jumpCount = 0;
                    animator.SetInteger("jumpCount", 0);
                    break;
                }
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Floor"))
            isGrounded = false;
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

    IEnumerator QuickStretch()
    {
        if (isStretching) yield break; // 이미 늘어지는 중이면 스킵

        isStretching = true;

        Vector3 originalScale = Vector3.one;
        Vector3 stretchScale = new Vector3(1f, 1.5f, 1f); // 짧게 팡! 하는 느낌

        float duration = 0.05f;

        // 늘어남
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            neckTransform.localScale = Vector3.Lerp(originalScale, stretchScale, t / duration);
            yield return null;
        }

        // 복귀
        t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            neckTransform.localScale = Vector3.Lerp(stretchScale, originalScale, t / duration);
            yield return null;
        }

        neckTransform.localScale = originalScale;
        isStretching = false;
    }


    public void TakeDamage(float rawDamage)
    {
        float reducedDamage = Mathf.Max(rawDamage - armor, 1f);

        if (currentShield > 0)
        {
            float shieldHit = Mathf.Min(currentShield, reducedDamage);
            currentShield -= shieldHit;
            reducedDamage -= shieldHit;
        }

        currentHP = Mathf.Max(currentHP - reducedDamage, 0);
    }
}