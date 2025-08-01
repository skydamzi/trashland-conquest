using UnityEngine;

public class DynamicPlayerShadow : MonoBehaviour
{
    // === 인스펙터에서 할당해야 하는 변수들 ===
    [Header("Player Components (자동 할당될 수 있음)")]
    public SpriteRenderer playerSpriteRenderer; // 플레이어의 Sprite Renderer
    public Transform playerTransform;           // 플레이어의 Transform

    [Header("Shadow Components (PlayerShadow 오브젝트에서 할당)")]
    public SpriteRenderer shadowSpriteRenderer; // 그림자 오브젝트의 Sprite Renderer
    public Transform shadowTransform;           // 그림자 오브젝트의 Transform

    [Header("Shadow Settings")]
    [Range(0.01f, 1.0f)]
    public float squashAmount = 0.3f; // 그림자를 얼마나 Y축으로 짜부라트릴지
                                      // (니 스샷처럼 납작하게 하려면 0.2 ~ 0.4 사이로 조절)
    public float shadowOffsetY = -0.2f; // 그림자 Y축 오프셋 (플레이어 발밑 위치)

    [Range(0f, 1f)] // 그림자 투명도 (0 = 완전 투명, 1 = 완전 불투명)
    public float shadowOpacity = 0.5f; // 기본값 50% 투명도

    // ===================================================================

    void Awake()
    {
        // 플레이어 컴포넌트 자동 할당 시도
        if (playerSpriteRenderer == null)
            playerSpriteRenderer = GetComponent<SpriteRenderer>();
        if (playerTransform == null)
            playerTransform = GetComponent<Transform>();

        // 자식 오브젝트인 PlayerShadow에서 컴포넌트 찾기
        // (이름을 "PlayerShadow"로 정확히 만들었는지 확인해라!)
        if (shadowSpriteRenderer == null && transform.Find("PlayerShadow") != null)
        {
            shadowSpriteRenderer = transform.Find("PlayerShadow").GetComponent<SpriteRenderer>();
        }
        if (shadowTransform == null && transform.Find("PlayerShadow") != null)
        {
            shadowTransform = transform.Find("PlayerShadow").GetComponent<Transform>();
        }

        // 필수 컴포넌트가 누락되었을 때 경고 메시지 출력
        if (playerSpriteRenderer == null || playerTransform == null ||
            shadowSpriteRenderer == null || shadowTransform == null)
        {
            Debug.LogError("Error: Missing required components for DynamicPlayerShadow on " + gameObject.name +
                           ". Check player and PlayerShadow setup. Disabling script.");
            enabled = false; // 스크립트 비활성화하여 에러 방지
        }
    }

    void LateUpdate() // 플레이어 이동/애니메이션 업데이트 후에 그림자도 업데이트
    {
        // 필수 컴포넌트가 없으면 동작하지 않음
        if (!enabled) return;

        // 1. 플레이어의 현재 스프라이트를 그림자 스프라이트에 복사
        //    이렇게 하면 플레이어 애니메이션에 따라 그림자 형태도 변함.
        shadowSpriteRenderer.sprite = playerSpriteRenderer.sprite;

        // 2. 그림자 위치 조정
        //    플레이어의 X, Y (2D 기준) 위치는 따라가고, Y축 오프셋을 적용하여 발밑에 오게 함.
        //    Z축은 정렬 순서에 영향을 주므로 플레이어와 동일하게 유지.
        Vector3 newShadowPosition = playerTransform.position;
        newShadowPosition.y += shadowOffsetY; // 발밑으로 오게 Y값 낮춤
        newShadowPosition.z = playerTransform.position.z; // 2D에서 Z축은 깊이, 정렬에 영향 (플레이어와 동일하게)
        shadowTransform.position = newShadowPosition;

        Color currentColor = shadowSpriteRenderer.color; // 현재 그림자의 색깔 가져오기
        currentColor.a = shadowOpacity; // 알파(투명도)만 shadowOpacity 값으로 설정
        shadowSpriteRenderer.color = currentColor; // 변경된 색깔 다시 적용
        // 그림자 색깔은 플레이어와 동일하게 유지 (투명도만 조정)
        
        // 3. 그림자 크기 조정 (Y축으로 짜부라트리기)
        Vector3 newShadowScale = playerTransform.localScale;
        newShadowScale.y *= squashAmount; // Y축으로 짜부라트림
        shadowTransform.localScale = newShadowScale;

        // 4. 그림자는 플레이어의 SpriteRenderer.flipX 상태를 '그대로' 따라감
        //    플레이어가 오른쪽을 보면 그림자도 오른쪽을 보고, 왼쪽을 보면 그림자도 왼쪽을 본다.
        shadowSpriteRenderer.flipX = playerSpriteRenderer.flipX;
        
        // 5. !!! 핵심 변경 !!! 그림자를 기본적으로 Y축으로 180도 뒤집음 (상하 반전)
        //    '기본적으로 180도 회전'한다는 의미를 SpriteRenderer.flipY로 구현한다.
        shadowSpriteRenderer.flipY = true; 

        // 6. 플레이어가 Z축으로 기울어질 때, 그림자는 그 기울기의 '반대 방향'으로 회전시킴.
        //    이것은 플레이어 Transform의 Z축 회전값에만 반응한다.

        // 플레이어의 현재 Z축 회전 각도를 가져옴 (localEulerAngles는 0-360 범위)
        float currentPlayerZRotation = playerTransform.localEulerAngles.z;
        
        // 그림자의 Z축 회전을 플레이어 Z축 회전의 '반대'로 설정.
        // 예를 들어 플레이어가 +30도 기울면 그림자는 -30도 기울고,
        // 플레이어가 -30도 기울면 그림자는 +30도 기울게 된다.
        float desiredShadowZRotation = -currentPlayerZRotation; 
        
        // 그림자 오브젝트의 Z축 회전만 설정 (X, Y는 0으로 고정)
        // 이 회전은 그림자의 '기본 180도 상하 반전' 위에 추가적으로 적용된다.
        shadowTransform.localRotation = Quaternion.Euler(0, 0, desiredShadowZRotation);
    }
}