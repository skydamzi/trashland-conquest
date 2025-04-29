using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Player : MonoBehaviour
{
    float speed = 0.01f;
    [SerializeField]
    private bool isLookAt = true;
    // 초기 forward 방향 저장
    private Vector2 initialDirection;
    [SerializeField]
    private float spriteDefaultAngle = 90f;
    [SerializeField]
    public GameObject bulletPrefab;
    [SerializeField] private GameObject weaponPrefab; // 무기 프리팹
    [SerializeField] private Transform weaponSpawnPoint; // 무기 생성 위치
    [SerializeField] private float swingAngle = 90f;
    [SerializeField] private float swingSpeed = 8f;
    [SerializeField] private float angleOffset = -90f; // 무기 기본 방향 보정
    private bool isSwinging = false;

    private void Start()
    {
        // 스프라이트의 기본 방향을 초기화
        initialDirection = transform.right; // 오른쪽을 기본으로 가정
    }

    private void Update()
    {
        LookAt();
        if (Input.GetMouseButtonDown(0))
            Fire(); // 총 발사

        if (Input.GetKeyDown(KeyCode.Space) && !isSwinging)
            StartCoroutine(SwingWeapon()); // 휘두르기 시작
        Movement();
    }

    void LookAt()
    {
        if (isLookAt == false) return;

        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (Vector2)(mouseWorldPosition - transform.position);

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle - spriteDefaultAngle);
    }
    
    void Movement()
    {
        if (Input.GetKey(KeyCode.W))
        {
            transform.position += new Vector3(0.0f, speed, 0);
        }
        if (Input.GetKey(KeyCode.S))
        {
            transform.position -= new Vector3(0.0f, speed, 0);
        }
        if (Input.GetKey(KeyCode.A))
        {
            transform.position -= new Vector3(speed, 0.0f, 0.0f);
        }
        if (Input.GetKey(KeyCode.D))
        {
            transform.position += new Vector3(speed, 0.0f, 0.0f);
        }
    }
    void Fire()
    {
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPosition.z = 0;

        Vector2 direction = (mouseWorldPosition - transform.position).normalized;

        GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.velocity = direction * 10f;
        else
            Debug.LogError("bulletPrefab에 Rigidbody2D 없음!");
    }

    IEnumerator SwingWeapon()
    {
        isSwinging = true;

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
        isSwinging = false;
    }

}


