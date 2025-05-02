using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : Unit
{
    public Text attack_powerText;
    public Text levelText;
    public bool isLookAt = true;
    // 초기 forward 방향 저장
    private Vector2 initialDirection;
    public float spriteDefaultAngle = 90f;
    public GameObject bulletPrefab;
    public GameObject weaponPrefab; // 무기 프리팹
    public Transform weaponSpawnPoint; // 무기 생성 위치
    public float swingAngle = 90f;
    public float swingSpeed = 4f;
    public float angleOffset = -90f; // 무기 기본 방향 보정
    //private bool isSwinging = false;
    int jumpCount = 0;
    public Rigidbody2D player_rb;
    public Transform neckTransform; // Inspector에서 NeckSprite 연결
    private bool isStretching = false;
    private Animator animator; // 애니메이터 참조
    public Collider2D neckCollider;  // Inspector에서 neck 오브젝트의 Collider 드래그해 연결

    private void Start()
    {
        // 스프라이트의 기본 방향을 초기화
        initialDirection = transform.right; // 오른쪽을 기본으로 가정
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        attack_powerText.text = $"공격력: {attackPower}";
        levelText.text = $"레벨: {unitLV}";

        LookAt();
        if (Input.GetMouseButtonDown(0))
            Fire(); // 총 발사

        if (Input.GetMouseButtonDown(1) && !isStretching)
        {
            StartCoroutine(StretchNeckAnim()); // 휘두르기 시작
        }
        Movement();
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (jumpCount < 2)
            {
                player_rb.velocity = new Vector2(0, 0);
                player_rb.AddForce(Vector2.up * 20, ForceMode2D.Impulse);
                jumpCount++;
            }
        }
        /*if (Input.GetKeyDown(KeyCode.E) && !isStretching)
        {
            StartCoroutine(StretchNeckAnim());
        }*/
    }

    void LookAt()
    {
        if (!isLookAt || neckTransform == null) return;

        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPosition.z = 0;

        Vector2 direction = (mouseWorldPosition - neckTransform.position).normalized;

        // 주둥이가 항상 마우스를 향하도록 회전 (Flip 무시)
        neckTransform.up = direction;
    }

    void Movement()
    {
        /*if (Input.GetKey(KeyCode.A))
        {
            transform.position += new Vector3(-1, 0, 0) * Time.deltaTime * 6f;
            //transform.rotation = Quaternion.Euler(0, 180, 0);
        }
        if (Input.GetKey(KeyCode.D))
        {
            transform.position += new Vector3(1, 0, 0) * Time.deltaTime * 6f;
            //transform.rotation = Quaternion.Euler(0, 0, 0);
        }*/
        float moveX = 0f;

        if (Input.GetKey(KeyCode.A))
        {
            moveX = -1f;
            // 왼쪽 바라보기
            transform.localScale = new Vector3(-1f, 1f, 1f);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            moveX = 1f;
            // 오른쪽 바라보기
            transform.localScale = new Vector3(1f, 1f, 1f);
        }
        animator.SetBool("isRunning", moveX != 0);
        Vector2 velocity = player_rb.velocity;
        velocity.x = moveX * 6f;
        player_rb.velocity = velocity;
    }
        void Fire()
    {
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPosition.z = 0;

        Vector2 direction = (mouseWorldPosition - transform.position).normalized;

        GameObject bullet = Instantiate(bulletPrefab, weaponSpawnPoint.position, Quaternion.identity);
        Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();

        if (bulletRb != null)
        {
            bulletRb.velocity = direction * 30f;

            // ➕ 플레이어와 충돌 무시
            Collider2D[] playerCols = GetComponentsInChildren<Collider2D>();
            Collider2D bulletCol = bullet.GetComponent<Collider2D>();
            foreach (var col in playerCols)
            {
                Physics2D.IgnoreCollision(bulletCol, col);
            }
        }
        else
        {
            Debug.LogError("bulletPrefab에 Rigidbody2D 없음!");
        }
    }

    /*IEnumerator SwingWeapon()
    {
        isSwinging = true;

        float temp = damage;
        damage *= 1.3f;
        GameObject weapon = Instantiate(weaponPrefab, weaponSpawnPoint.position, weaponSpawnPoint.rotation, transform);
        Transform wt = weapon.transform;

        float timer = 0f;
        while (timer < 1f)
        {
            timer += Time.deltaTime * swingSpeed;
            float angle = Mathf.Sin(timer * Mathf.PI) * swingAngle;
            wt.localRotation = Quaternion.Euler(0f, 0f, angle + angleOffset);
            yield return null;
        }

        Destroy(weapon);
        damage = temp;
        isSwinging = false;
    }*/

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Floor"))
        {
            jumpCount = 0;
        }
    }
    IEnumerator StretchNeckAnim()
    {
        isStretching = true;
        neckCollider.enabled = true;
        float stretchTime = 0.3f;
        float returnTime = 0.3f;

        Vector3 originalScale = new Vector3(1f, 1f, 1f);

        // 마우스 월드 좌표
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;

        // 거리 계산
        float distance = Vector2.Distance(neckTransform.position, mouseWorldPos);

        // SpriteRenderer의 원본 스프라이트 픽셀 길이
        float spritePixelHeight = neckTransform.GetComponent<SpriteRenderer>().sprite.rect.height;

        // 픽셀을 월드 유닛으로 바꾸기
        float spriteWorldHeight = spritePixelHeight / neckTransform.GetComponent<SpriteRenderer>().sprite.pixelsPerUnit;

        // 정확한 scaleY
        float scaleY = distance / spriteWorldHeight;

        // 목표 스케일
        Vector3 targetScale = new Vector3(1f, scaleY, 1f);

        float timer = 0f;
        while (timer < stretchTime)
        {
            timer += Time.deltaTime;
            float t = timer / stretchTime;
            neckTransform.localScale = Vector3.Lerp(originalScale, targetScale, t);
            yield return null;
        }

        timer = 0f;
        while (timer < returnTime)
        {
            timer += Time.deltaTime;
            float t = timer / returnTime;
            neckTransform.localScale = Vector3.Lerp(targetScale, originalScale, t);
            yield return null;
        }

        neckTransform.localScale = originalScale;
        neckCollider.enabled = false;
        isStretching = false;
    }

}


