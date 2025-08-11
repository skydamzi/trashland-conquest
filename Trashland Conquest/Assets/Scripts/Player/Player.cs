using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Unit 클래스에서 maxStamina, currentStamina를 상속받았다고 가정
public class Player : Unit, IDamageable
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
    public Rigidbody2D player_rb; // Rigidbody2D는 그대로 유지!
    private int jumpCount = 0;
    private bool isGrounded = false;
    private bool wasGroundedLastFrame = false;

    [Header("Neck Attack")]
    public Transform neckTransform;
    public Transform neckCoverTransform;
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

    [Header("Animation Control")] // 새로운 헤더 추가
    public bool isFacingFront = false; // 정면 바라보는지 여부를 애니메이터에 넘겨줄 변수
    private Vector2 lastNonZeroMoveInput = Vector2.down; // 캐릭터가 멈췄을 때 마지막 유효 이동 방향 (기본값: 위)

    private Animator animator;
    public AudioClip shootSound;
    public AudioClip jump1stSound;
    public AudioClip jump2ndSound;
    public AudioClip glove_readySound;
    public AudioClip glove_punchSound;

    [Header("Stamina System")]
    public float shootStaminaCost = 5f;
    public float punchStaminaCost = 15f;
    public float staminaRegenRate = 30f;
    public float staminaRegenDelay = 1.5f; // 스테미나 리젠 딜레이 시간
    private float lastStaminaConsumedTime; // 마지막으로 스태미나를 소모한 시간으로 변경
    private Coroutine staminaRegenCoroutine; // 스태미나 회복 코루틴
    private bool isStaminaDepleted = false; 

    [Header("Tilt Settings")]
    public float tiltAngle = 15f; // 캐릭터가 좌우로 기울어지는 최대 각도
    public float tiltSpeed = 30f; // 기울어지는 속도

    [Header("Damage Cooldown")]
    private bool canTakeDamage = true; // 데미지를 받을 수 있는지 여부
    public float damageCooldown = 1.0f; // 데미지를 다시 받기까지의 쿨다운 시간

    [Header("Charging Attack")]
    private bool isPunchCharging = false; // 펀치 차지 중인지 확인하는 플래그
    private float punchChargeTimer = 0f; // 펀치 차지 시간
    public float maxPunchChargeTime = 0.5f; // 최대 차지 시간
    public float maxStaminaCost = 30f; // 최대 차지 시 소모할 스태미나
    public float minStaminaCost = 5f; // 최소 차지 시 소모할 스태미나
    public float finalPunchDamage = 0f; // 차지 펀치 기본 데미지

    void Start()
    {
        initialDirection = transform.right;
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        // Rigidbody2D 컴포넌트 가져오기 (만약 인스펙터에서 안 끌어왔다면)
        if (player_rb == null) 
        {
            player_rb = GetComponent<Rigidbody2D>();
        }

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
        if (Pause.isPaused) return;
        LookAt();
        Movement();
        // Jump(); // 점프 임시로 뺄 거라 했으니 주석 처리함. 나중에 필요하면 주석 풀어!
        PlayerStatusUpdate(); 

        fireTimer += Time.deltaTime;
        stretchTimer += Time.deltaTime;
        if (!isGrounded && wasGroundedLastFrame && jumpCount == 0)
        {
            jumpCount = 1;
        }

        wasGroundedLastFrame = isGrounded;

        
        PunchAttack();
        FireAttack();

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
        
        // isFacingFront 애니메이터 파라미터 업데이트는 Movement()에서 처리하므로 여기서 별도 호출 필요 없음
        // animator.SetBool("isFacingFront", isFacingFront); 
    }
    void PunchAttack()
    {
        // 스태미나가 고갈 상태이거나, 목 늘이기 중이면 공격 불가
        if (isStaminaDepleted || isStretching)
        {
            // 차지 중이었으면 강제로 초기화
            if (isPunchCharging)
            {
                isPunchCharging = false;
                punchChargeTimer = 0f;
                Debug.Log("스태미나 고갈 또는 목 늘이기 중이라 차지 취소됨.");
            }
            return; // 공격 시도조차 못 하게 막음
        }

        // 마우스 우클릭 누르고 있을 때 (펀치 차지 시작/진행)
        if (Input.GetMouseButton(1))
        {
            // 스태미나가 0보다 많고, 아직 차지 중이 아니면 차지 시작
            if (currentStamina > 0 && !isPunchCharging)
            {
                isPunchCharging = true;
                punchChargeTimer = 0f;
                Debug.Log("펀치 차지 시작!");
                // 애니메이션: 펀치 차지 시작 애니메이션 트리거 (예: "punchCharge")
                // animator.SetTrigger("punchCharge");
            }

            // 차지 중이면 타이머 증가 (스태미나도 서서히 소모시킬 수 있음)
            if (isPunchCharging)
            {
                punchChargeTimer += Time.deltaTime;
                punchChargeTimer = Mathf.Min(punchChargeTimer, maxPunchChargeTime); // 최대 차지 시간 제한

                if (punchChargeTimer < maxPunchChargeTime)
                {
                    float staminaDrainRate = (maxStaminaCost - minStaminaCost) / maxPunchChargeTime;
                    currentStamina -= staminaDrainRate * Time.deltaTime;
                    currentStamina = Mathf.Max(currentStamina, 0);
                    if (PlayerStatus.instance != null)
                    {
                        PlayerStatus.instance.currentStamina = currentStamina;
                    }
                }
                else
                {
                    float staminaDrainRate = (maxStaminaCost - minStaminaCost) / maxPunchChargeTime;
                    currentStamina -= staminaDrainRate * Time.deltaTime / 5;
                    currentStamina = Mathf.Max(currentStamina, 0);
                    if (PlayerStatus.instance != null)
                    {
                        PlayerStatus.instance.currentStamina = currentStamina;
                    }
                }
                // 목이 차지하면서 줄어드는 효과
                neckTransform.localScale = Vector3.Lerp(Vector3.one, new Vector3(1f, 0.5f, 1f), punchChargeTimer / maxPunchChargeTime);
                if (staminaRegenCoroutine != null)
                {
                    StopCoroutine(staminaRegenCoroutine);
                    staminaRegenCoroutine = null;
                }
            }
        }

        // 마우스 우클릭에서 손을 뗄 때 (펀치 공격 실행)
        if (Input.GetMouseButtonUp(1) && isPunchCharging)
        {
            isPunchCharging = false; // 차지 상태 해제

            // 차지 비율을 계산해서 데미지와 스태미나를 동적으로 계산
            float chargeRatio = punchChargeTimer / maxPunchChargeTime;
            finalPunchDamage = Mathf.Lerp(0f, GetMeleeDamage(), chargeRatio);
            

            if (PlayerStatus.instance != null)
            {
                PlayerStatus.instance.currentStamina = currentStamina;
            }
            lastStaminaConsumedTime = Time.time;

            // 펀치 애니메이션 실행
            animator.SetTrigger("attack");
            StartCoroutine(StretchNeckAnim());

            Debug.Log($"펀치 공격! 차지 시간: {punchChargeTimer:F2}초, 최종 데미지: {finalPunchDamage:F2}");

            // **2. 데미지를 전달하는 함수 호출 (예: Target에게 데미지 적용)**
            // ApplyDamage(finalDamage);

            // **3. 스태미나가 0이 되면 고갈 상태로 전환**
            if (currentStamina <= 0)
            {
                isStaminaDepleted = true;
                Debug.Log("스태미나 고갈 상태 진입! 100% 회복까지 공격 불가!");
            }

            punchChargeTimer = 0f; // 타이머 초기화
        }
    }
    void FireAttack()
    {
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

    // Jump 함수는 그대로 두되, Update에서 호출 부분만 주석 처리함.
    /*
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

            //jumpCount++;
            //animator.SetInteger("jumpCount", jumpCount);
            //animator.SetBool("isJumping", true);
        }
    }*/

  void LookAt()
{
    // 총알 조준용이니 그대로 유지
    if (!isLookAt || neckTransform == null || isStretching) return;

    Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    mouseWorldPos.z = 0;
    Vector3 direction = (mouseWorldPos - neckTransform.position).normalized;
    Quaternion targetRotation = Quaternion.LookRotation(Vector3.forward, direction);
    neckTransform.rotation = Quaternion.Lerp(neckTransform.rotation, targetRotation, Time.deltaTime * 20f);
}

// --- 여기부터 Movement 함수 수정 ---
void Movement()
{
    if (isStretching) // 목 늘이기 중이면 이동 불가
    {
        player_rb.velocity = new Vector2(0f, 0f);

        animator.SetBool("isRunning", false);
        animator.SetFloat("InputX", 0f);
        animator.SetFloat("InputY", 0f);
        return;
    }

    // WASD 입력 받기
    float moveX = Input.GetAxisRaw("Horizontal");
    float moveY = Input.GetAxisRaw("Vertical");

    Vector2 moveInput = new Vector2(moveX, moveY);
    Vector2 moveDirection = moveInput.normalized;

    player_rb.velocity = moveDirection * moveSpeed;

    bool isMoving = moveInput.magnitude > 0.1f;

    // 움직이는 중이면 lastNonZeroMoveInput 업데이트
    if (isMoving)
    {
        lastNonZeroMoveInput = moveInput;
    }

    // --- 스프라이트 좌우 반전 로직 (삭제!!!) ---
    // 애니메이션에서 직접 처리한다고 했으니 여기 코드는 필요없다, 새꺄!
    // float currentFlipDirectionX = moveX; 
    // if (Mathf.Abs(moveX) < 0.01f && Mathf.Abs(lastNonZeroMoveInput.x) > 0.01f)
    // {
    //     currentFlipDirectionX = lastNonZeroMoveInput.x;
    // }
    // if (Mathf.Abs(currentFlipDirectionX) > 0.01f)
    // {
    //     Vector3 currentScale = transform.localScale;
    //     currentScale.x = Mathf.Abs(currentScale.x) * Mathf.Sign(currentFlipDirectionX);
    //     transform.localScale = currentScale; // 이 라인도 삭제!
    // }
    // --- 스프라이트 좌우 반전 로직 끝 (싹 다 날려버림!) ---

    // --- 기울기 로직 (수정된 버전) ---
    float maxTilt = tiltAngle;
    Vector2 moveDir = moveInput.normalized;
    float tiltPercent = Mathf.Abs(moveDir.x); // X축 기울기 비중 계산
    float targetZ = -moveDir.x * maxTilt * tiltPercent;

    Quaternion currentRotation = transform.rotation;
    Quaternion targetRotation = Quaternion.Euler(0, 0, targetZ);
    transform.rotation = Quaternion.Lerp(currentRotation, targetRotation, Time.deltaTime * tiltSpeed);
    // --- 기울기 로직 끝 ---

    // --- 애니메이터 파라미터 갱신 ---
    animator.SetBool("isRunning", isMoving);

    // 애니메이터는 lastNonZeroMoveInput을 계속 사용해서 마지막 방향을 유지하도록.
    if (isMoving)
    {
        animator.SetFloat("InputX", moveInput.x);
        animator.SetFloat("InputY", moveInput.y);
    }
    else
    {
        animator.SetFloat("InputX", lastNonZeroMoveInput.x);
        animator.SetFloat("InputY", lastNonZeroMoveInput.y);
    }
}



    // --- Movement 함수 수정 끝 ---

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

    private void OnTriggerEnter2D(Collider2D other)
    {
        
    }

    /*private void OnTriggerStay2D(Collider2D other)
    {
        if (!isInvincible && canTakeDamage)
        {
            if (other.CompareTag("Boss") || other.CompareTag("Poop") || other.CompareTag("Enemy"))
            {
                float damageToTake = 0f;

                // ... (데미지 계산 로직) ...
                if (other.CompareTag("Poop"))
                {
                    damageToTake = 10f; // Poop의 고정 데미지
                }
                else if (other.CompareTag("Boss"))
                {
                    Boss boss = other.GetComponentInParent<Boss>();
                    if (boss != null)
                    {
                        damageToTake = boss.GetBaseDamage();
                    }
                }
                else if (other.CompareTag("Enemy"))
                {
                    Enemy enemy = other.GetComponent<Enemy>();
                    if (enemy != null)
                    {
                        damageToTake = enemy.GetBaseDamage();
                    }
                }

                if (damageToTake > 0)
                {
                    TakeDamage(damageToTake);
                    Sound.Instance.PlaySFX(glove_punchSound);

                    // 데미지를 입힌 후 쿨다운 코루틴 시작
                    StartCoroutine(DamageCooldownCoroutine());
                }
            }
        }
    }*/
    
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

    IEnumerator DamageCooldownCoroutine()
    {
        canTakeDamage = false;
        // isInvincible 코루틴과 함께 실행
        StartCoroutine(TriggerInvincibility());
        yield return new WaitForSeconds(damageCooldown);
        canTakeDamage = true;
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
        if (!canTakeDamage || isInvincible) return; // 데미지를 받을 수 없거나 무적 상태면 아무 일도 안 함
        float reducedDamage = Mathf.Max(rawDamage - armor, 1f);
        // 무적상태일때는 아무 일도 일어나지 않음
        Sound.Instance.PlaySFX(glove_punchSound);  

        // 쉴드가 0보다 크면 쉴드 카운트만 줄이고 데미지는 안 들어감
        if (currentShield > 0)
        {
            currentShield--;
        }

        else // 쉴드가 없으면 HP가 줄어든다.
        {
            currentHP = Mathf.Max(currentHP - reducedDamage, 0);
        }

        if (PlayerStatus.instance != null)
        {
            PlayerStatus.instance.currentHP = currentHP;
            PlayerStatus.instance.currentShield = currentShield;
        }

        if (currentHP <= 0)
        {
            Die(); // 체력이 0 이하면 Die() 호출
        }
        StartCoroutine(TriggerInvincibility());
    }

    void Die()
    {
        gameObject.SetActive(false); // 플레이어 오브젝트 비활성화

        if (GameManager.instance != null)
        {
            GameManager.instance.EndGame(GameManager.GameEndReason.LossByDeath); // isWin = false (패배)
        }
    }
}