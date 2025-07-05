using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pause : MonoBehaviour
{
    public static Pause Instance;
    public static bool isPaused = false;
    BossIntroManager introManager; // <<< 선언만 하고 초기화는 안 함

    [Header("사운드")] // 이거 인코딩 깨졌는데 '사운드' 일듯
    public AudioClip PauseSound;

    [Header("일시정지 메뉴 UI")] // 이것도 인코딩 깨졌는데 '일시정지 메뉴 UI' 일듯
    public GameObject PauseMenuUI;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        // <<< 씨발, 여기다! 여기다 이사시켜라!
        introManager = FindObjectOfType<BossIntroManager>(); 
        // 이제 Awake에서 호출되니까 제대로 찾아올 거다.
        // 만약 BossIntroManager가 Pause 스크립트보다 나중에 활성화될 수도 있다면 Start()에서 초기화하는 게 더 안전할 수도 있다.
        // 근데 일단 Awake()로 옮겨봐라.
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            Time.timeScale = 0f;
            Debug.Log(" Ͻ!"); // 게임이 일시정지됨!
            
            if (PauseMenuUI != null)
                PauseMenuUI.SetActive(true);
            
            // 씨발, introManager가 null일 수도 있으니 null 체크 해라!
            if (introManager != null) 
            {
                // introManager.StopIntro(); // 보스 인트로 관련 뭔가를 멈춰야 할 것 같으면 여기에 넣어라
            }
            Sound.Instance.PlaySFX(PauseSound, 1f); // Sound.Instance도 null 체크 필요할 수도 있음
        }
        else
        {
            Time.timeScale = 1f;
            Debug.Log(" 簳!"); // 게임이 재개됨!

            if (PauseMenuUI != null)
                PauseMenuUI.SetActive(false);
            
            // 정지 해제 시에도 introManager 관련 동작이 필요하면 여기에 넣어라
        }
    }
}