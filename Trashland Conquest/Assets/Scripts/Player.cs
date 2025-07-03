using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Unit 클래스에서 maxStamina, currentStamina를 상속받았다고 가정
public class Player : Unit
{
    [Header("Direction & Rotation")]
    public bool isLookAt = true;
    private Vector2 initialDirection;
    public float spriteDefaultAngle = 90f;

    [Header("Prefabs & Spawn")]
    public GameObject bulletPrefab;
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
    private Vector3 fixedNeckTarget;
    public HashSet<GameObject> hitEnemiesThisAttack = new HashSet<GameObject>();

    [Header("Fire Control")]
    private float fireRate = 0.2f;
    private float fireTimer = 0f;

    [Header("Neck Stretch FX")]
    private float stretchTimer = 0f;

    [Header("Glove")]
    public Transform gloveTransform;
    public Vector3 gloveOriginalScale;
    public bool isPunchFrame = false;

    [Header("isInvincible")]
    private bool isInvincible = false;
    public float invincibleDuration = 1f;
    private SpriteRenderer spriteRenderer;

    [Header("TraitSynergy")]
    public TraitSynergy traitSynergy;

    private Animator animator;
    public AudioClip shootSound;
    public AudioClip jump1stSound;
    public AudioClip jump2ndSound;
    public AudioClip glove_readySound;
    public AudioClip glove_punchSound;

    // **스태미나 시스템 변수 수정 및 추가**
    [Header("Stamina System")]
    public float shootStaminaCost = 5f;
    public float punchStaminaCost = 15f;
    public float staminaRegenRate = 30f;
    public float staminaRegenDelay = 1.5f; // 1.5초로 변경
    private float lastStaminaConsumedTime; // 마지막으로 스태미나를 소모한 시간으로 변경
    private Coroutine staminaRegenCoroutine; // 스태미나 회복 코루틴을 제어하기 위한 변수

    // **스태미나 고갈 상태 추적 - 0이 되고 100% 회복까지 공격 금지**
    private bool isStaminaDepleted = false; 

    void Start()
    {
        initialDirection = transform.right;
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (gloveTransform != null)
        {
            gloveOriginalScale = gloveTransform.localScale;
            gloveTransform.localScale = Vector3.zero;
            gloveTransform.gameObject.SetActive(false);
        }
        
        // PlayerStatus.instance가 있다면 maxStamina를 초기화해줘야 함 (Unit 클래스에서 안 한다면)
        if (PlayerStatus.instance != null) {
            maxStamina = PlayerStatus.instance.maxStamina; // PlayerStatus에서 가져와서 maxStamina 설정
            currentStamina = maxStamina; // 시작 시 스태미나 풀로 채움
            PlayerStatus.instance.currentStamina = currentStamina; // PlayerStatus도 업데이트
        }
        lastStaminaConsumedTime = -staminaRegenDelay; // 시작하자마자 스태미나 회복 가능하도록 설정
    }

    void Update()
    {
        LookAt();
        Movement();
        Jump();
        PlayerStatusUpdate(); 

        fireTimer += Time.deltaTime;
        stretchTimer += Time.deltaTime;
        animator.SetBool("isJumping", !isGrounded);
        animator.SetFloat("verticalVelocity", player_rb.velocity.y);
        if (!isGrounded && wasGroundedLastFrame && jumpCount == 0)
        {
            jumpCount = 1;
        }

        wasGroundedLastFrame = isGrounded;

        // **펀치 공격 (오른쪽 마우스 버튼)**
        // isStaminaDepleted 상태이거나, 목 늘이기 중이면 공격 불가
        if (Input.GetMouseButtonDown(1) && !isStretching)
        {
            // 스태미나가 고갈 상태가 아니고, 스태미나가 0보다 많을 때만 공격 시도 (자투리 스태미나 허용)
            if (!isStaminaDepleted && currentStamina > 0) 
            {
                // 공격 시 스태미나 회복 중단 (코루틴이 실행 중이면 멈춘다)
                if (staminaRegenCoroutine != null)
                {
                    StopCoroutine(staminaRegenCoroutine);
                    staminaRegenCoroutine = null;
                }
                
                // 실제 소모될 스태미나 계산 (currentStamina가 punchStaminaCost보다 작으면 currentStamina만큼만 소모)
                float staminaToConsume = Mathf.Min(currentStamina, punchStaminaCost);
                currentStamina -= staminaToConsume;
                
                if (PlayerStatus.instance != null) // PlayerStatus에 깎인 값 업데이트
                {
                    PlayerStatus.instance.currentStamina = currentStamina;
                }
                lastStaminaConsumedTime = Time.time; // 스태미나 소모 시간 업데이트
                Debug.Log($"[Punch] 스태미나 소모: {staminaToConsume}, 남은 스태미나: {currentStamina}");

                animator.SetTrigger("attack");
                StartCoroutine(StretchNeckAnim()); // 이 부분은 원래대로 유지

                // **스태미나가 0이 되면 고갈 상태로 전환**
                if (currentStamina <= 0) // 스태미나가 0 이하가 되면 고갈 상태로!
                {
                    isStaminaDepleted = true; // 스태미나 고갈 플래그 ON
                    Debug.Log("스태미나 고갈 상태 진입! 스태미나 0이다! 100% 회복까지 공격 불가!");
                }
            }
            else // 스태미나가 부족하거나 고갈 상태일 때
            {
                Debug.Log($"펀치 공격 불가! (스태미나 고갈: {isStaminaDepleted}, 현재 스태미나: {currentStamina})"); 
            }
        }

        // **총 발사 (왼쪽 마우스 버튼)**
        // isStaminaDepleted 상태이거나, 목 늘이기 중이면 공격 불가
        if (Input.GetMouseButton(0) && fireTimer >= fireRate && !isStretching)
        {
            // 스태미나가 고갈 상태가 아니고, 스태미나가 0보다 많을 때만 공격 시도 (자투리 스태미나 허용)
            if (!isStaminaDepleted && currentStamina > 0) 
            {
                // 공격 시 스태미나 회복 중단
                if (staminaRegenCoroutine != null)
                {
                    StopCoroutine(staminaRegenCoroutine);
                    staminaRegenCoroutine = null;
                }

                Fire(); // 총 발사
                // 실제 소모될 스태미나 계산 (currentStamina가 shootStaminaCost보다 작으면 currentStamina만큼만 소모)
                float staminaToConsume = Mathf.Min(currentStamina, shootStaminaCost);
                currentStamina -= staminaToConsume;

                if (PlayerStatus.instance != null) // PlayerStatus에 깎인 값 업데이트
                {
                    PlayerStatus.instance.currentStamina = currentStamina;
                }
                lastStaminaConsumedTime = Time.time; // 스태미나 소모 시간 업데이트
                Debug.Log($"[Shoot] 스태미나 소모: {staminaToConsume}, 남은 스태미나: {currentStamina}");

                Sound.Instance.PlaySFX(shootSound, 1f);
                stretchTimer = 0f;
                fireTimer = 0f;

                // **스태미나가 0이 되면 고갈 상태로 전환**
                if (currentStamina <= 0) // 스태미나가 0 이하가 되면 고갈 상태로!
                {
                    isStaminaDepleted = true; // 스태미나 고갈 플래그 ON
                    Debug.Log("스태미나 고갈 상태 진입! 스태미나 0이다! 100% 회복까지 공격 불가!");
                }
            }
            else // 스태미나가 부족하거나 고갈 상태일 때
            {
                Debug.Log($"총 발사 불가! (스태미나 고갈: {isStaminaDepleted}, 현재 스태미나: {currentStamina})");
            }
        }

        // **스태미나 회복 로직**
        // 스태미나가 최대치가 아니고, 회복 코루틴이 실행 중이 아니며, 회복 지연 시간이 지났을 때만 회복 시작
        // isStaminaDepleted 상태와 관계없이 회복은 계속 진행되어야 함.
        if (currentStamina < maxStamina && Time.time - lastStaminaConsumedTime >= staminaRegenDelay && staminaRegenCoroutine == null)
        {
            staminaRegenCoroutine = StartCoroutine(RegenerateStamina());
        }
        
        // **isStaminaDepleted 상태 해제 로직 (핵심)**
        // 스태미나가 꽉 찼을 때만 고갈 상태 해제!
        if (isStaminaDepleted && currentStamina >= maxStamina) 
        {
            isStaminaDepleted = false;
            Debug.Log("스태미나 100% 완전 회복! 고갈 상태 해제! 다시 공격 가능!");
        }

        if (!isInvincible)
            SetAlpha(1f);
    }

    void PlayerStatusUpdate()
    {
        if (PlayerStatus.instance != null)
        {
            unitName = PlayerStatus.instance.unitName;
            unitLV = PlayerStatus.instance.unitLV;

            baseAttackPower = PlayerStatus.instance.baseAttackPower;
            bonusAttackPower = PlayerStatus.instance.bonusAttackPower;
            armor = PlayerStatus.instance.armor;

            currentHP = PlayerStatus.instance.currentHP;
            maxHP = PlayerStatus.instance.maxHP;
            currentShield = PlayerStatus.instance.currentShield;
            maxShield = PlayerStatus.instance.maxShield;
            currentEXP = PlayerStatus.instance.currentEXP;
            maxEXP = PlayerStatus.instance.maxEXP;

            moveSpeed = PlayerStatus.instance.moveSpeed;
            criticalChance = PlayerStatus.instance.criticalChance;
        }
    }
    private void LateUpdate()
    {
        if (gloveTransform.gameObject.activeSelf)
        {
            SpriteRenderer neckSR = neckTransform.GetComponent<SpriteRenderer>();
            float spriteHeight = neckSR.sprite.rect.height;
            float unitPerPixel = neckSR.sprite.pixelsPerUnit;
            float worldLength = (spriteHeight / unitPerPixel) * neckTransform.localScale.y;

            Vector3 neckTip = neckTransform.position + neckTransform.up * worldLength;
            gloveTransform.position = neckTip;
            gloveTransform.rotation = neckTransform.rotation;
        }
    }

    void Jump()
    {
        if (Input.GetKeyDown(KeyCode.W) && jumpCount < 2)
        {
            player_rb.velocity = Vector2.zero;

            if (jumpCount == 0)
            {
                player_rb.AddForce(Vector2.up * 18f, ForceMode2D.Impulse);
                Sound.Instance.PlaySFX(jump1stSound, 1f);
            }
            else if (jumpCount == 1)
            {
                player_rb.AddForce(Vector2.up * 12f, ForceMode2D.Impulse);
                Sound.Instance.PlaySFX(jump2ndSound, 1f);
            }

            jumpCount++;
            animator.SetInteger("jumpCount", jumpCount);
            animator.SetBool("isJumping", true);
        }
    }

    void LookAt()
    {
        if (!isLookAt || neckTransform == null || isStretching) return;

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;
        Vector3 direction = (mouseWorldPos - neckTransform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(Vector3.forward, direction);
        neckTransform.rotation = Quaternion.Lerp(neckTransform.rotation, targetRotation, Time.deltaTime * 20f);
    }

    void Movement()
    {
        if (isStretching)
        {
            player_rb.velocity = new Vector2(0f, player_rb.velocity.y);
            animator.SetBool("isRunning", false);
            return;
        }

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
        velocity.x = moveX * moveSpeed;
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

        Quaternion currentRotation = transform.rotation;
        Quaternion targetRotation = Quaternion.Euler(0, 0, targetZ);
        transform.rotation = Quaternion.Lerp(currentRotation, targetRotation, Time.deltaTime * 30f);
    }

    void Fire() // 총알 발사 (탄약 시스템 제거)
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;
        Vector2 direction = (mouseWorldPos - weaponSpawnPoint.position).normalized;

        GameObject bullet = Instantiate(bulletPrefab, weaponSpawnPoint.position, Quaternion.identity);
        Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();

        if (bulletRb != null)
        {
            bulletRb.velocity = direction * 30f;

            Collider2D[] playerCols = GetComponentsInChildren<Collider2D>();
            Collider2D bulletCol = bullet.GetComponent<Collider2D>();

            if (bulletCol != null)
            {
                foreach (var col in playerCols)
                {
                    if (col != null)
                        Physics2D.IgnoreCollision(bulletCol, col);
                }
            }
        }

        Destroy(bullet, 1f);
        StartCoroutine(QuickStretch());
    }

    // **스태미나 회복 코루틴**
    IEnumerator RegenerateStamina()
    {
        // 스태미나 회복 지연 시간 기다림
        yield return new WaitForSeconds(staminaRegenDelay);

        Debug.Log("스태미나 회복 시작...");

        while (currentStamina < maxStamina)
        {
            currentStamina = Mathf.Min(currentStamina + staminaRegenRate * Time.deltaTime, maxStamina);
            if (PlayerStatus.instance != null)
            {
                PlayerStatus.instance.currentStamina = currentStamina;
            }
            yield return null;
        }

        // 스태미나 완전 회복 완료!
        currentStamina = maxStamina;
        if (PlayerStatus.instance != null)
        {
            PlayerStatus.instance.currentStamina = currentStamina;
        }
        
        staminaRegenCoroutine = null; // 회복 코루틴 참조 제거
        Debug.Log("스태미나 회복 완료!");
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
                    animator.SetBool("isJumping", false);
                    break;
                }
            }
        }
        if (collision.gameObject.CompareTag("Boss"))
        {
            if (isInvincible) return;
            Boss boss = collision.gameObject.GetComponentInParent<Boss>();
            Enemy enemy = collision.gameObject.GetComponentInParent<Enemy>();
            if (boss != null)
            {
                TakeDamage(boss.GetBaseDamage());
                Sound.Instance.PlaySFX(glove_punchSound);
                StartCoroutine(TriggerInvincibility());
            }
            else if (enemy != null)
            {
                TakeDamage(enemy.GetBaseDamage());
                Sound.Instance.PlaySFX(glove_punchSound);
                StartCoroutine(TriggerInvincibility());
            }
        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isInvincible) return;

        if (other.CompareTag("Poop"))
        {
            if (other.bounds.Intersects(GetComponent<Collider2D>().bounds))
            {
                TakeDamage(10f);
                Sound.Instance.PlaySFX(glove_punchSound);
                StartCoroutine(TriggerInvincibility());
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
        isStretching = true; // 목 늘이기 시작
        neckCollider.enabled = true;

        fixedNeckTarget = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        fixedNeckTarget.z = 0;

        Coroutine rotationFix = StartCoroutine(RestoreRotation());
        Coroutine neckStretch = StartCoroutine(NeckStretchAnimSequence());

        yield return rotationFix;
        yield return neckStretch;

        neckCollider.enabled = false;
        isStretching = false; // 목 늘이기 끝
    }

    IEnumerator RestoreRotation()
    {
        Quaternion targetRotation = Quaternion.Euler(0, 0, 0);
        while (Quaternion.Angle(transform.rotation, targetRotation) > 0.1f)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 30f);
            yield return null;
        }
        transform.rotation = targetRotation;
    }

    IEnumerator NeckStretchAnimSequence()
    {
        Vector3 originalScale = Vector3.one;
        Vector3 direction = (fixedNeckTarget - neckTransform.position).normalized;
        neckTransform.rotation = Quaternion.LookRotation(Vector3.forward, direction);

        float stretchTime = 0.1f;
        float returnTime = 0.2f;
        float gloveHoldTime = 0.05f;

        Vector3 targetNeckScale = new Vector3(1f, 3.5f, 1f);

        gloveTransform.localScale = Vector3.zero;
        gloveTransform.gameObject.SetActive(true);

        animator.SetTrigger("attack");
        isNeckAttacking = true;
        hitEnemiesThisAttack.Clear();
        Sound.Instance.PlaySFX(glove_readySound);
        yield return StartCoroutine(AnimateGlovePop(Vector3.zero, gloveOriginalScale, stretchTime));
        yield return new WaitForSeconds(gloveHoldTime);

        yield return StartCoroutine(AnimateGlovePop(gloveOriginalScale, gloveOriginalScale * 3f, 0.1f));
        yield return StartCoroutine(ScaleOverTime(neckTransform, originalScale, targetNeckScale, 0.1f));
        yield return new WaitForSeconds(0.15f);

        animator.SetTrigger("attackReverse");
        Coroutine neckShrink = StartCoroutine(ScaleOverTime(neckTransform, targetNeckScale, originalScale, returnTime));
        yield return new WaitForSeconds(returnTime / 2f);
        yield return StartCoroutine(AnimateGlovePop(gloveOriginalScale * 3f, Vector3.zero, returnTime / 2f));
        isNeckAttacking = false;

        gloveTransform.gameObject.SetActive(false);
        yield return neckShrink;
    }

    public void StartNeckStretch()
    {
        if (!isStretching) 
            StartCoroutine(StretchNeckAnim());
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
    IEnumerator AnimateGlovePop(Vector3 from, Vector3 to, float duration)
    {
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;
            gloveTransform.localScale = Vector3.Lerp(from, to, t);
            yield return null;
        }
    }

    IEnumerator QuickStretch()
    {
        if (isStretching) yield break;

        Vector3 originalScale = Vector3.one;
        Vector3 stretchScale = new Vector3(1f, 1.5f, 1f);

        float duration = 0.05f;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            neckTransform.localScale = Vector3.Lerp(originalScale, stretchScale, t / duration);
            yield return null;
        }

        t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            neckTransform.localScale = Vector3.Lerp(stretchScale, originalScale, t / duration);
            yield return null;
        }

        neckTransform.localScale = originalScale;
    }

    IEnumerator TriggerInvincibility()
    {
        isInvincible = true;

        float blinkInterval = 0.1f;
        float elapsed = 0f;

        while (elapsed < invincibleDuration)
        {
            SetAlpha(0.3f);
            yield return new WaitForSeconds(blinkInterval / 2f);

            SetAlpha(1f);
            yield return new WaitForSeconds(blinkInterval / 2f);

            elapsed += blinkInterval;
        }

        SetAlpha(1f);
        isInvincible = false;
    }
    void SetAlpha(float alpha)
    {
        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();
        foreach (var renderer in renderers)
        {
            Color color = renderer.color;
            color.a = alpha;
            renderer.color = color;
        }
    }
    public void OnSuccessfulPunch()
    {
        Sound.Instance.PlaySFX(glove_punchSound);
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
        if (PlayerStatus.instance != null)
        {
            PlayerStatus.instance.currentHP = currentHP;
            PlayerStatus.instance.currentShield = currentShield;
        }
    }
}