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

    [Header("패널 리스트 (왼쪽부터 오른쪽 순서)")]
    public List<GameObject> menuPanels = new List<GameObject>(); // 0: Equipment, 1: Inventory, 2: Skill, 3: Setting 등
    public int currentPanelIndex = 0;


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

        introManager = FindObjectOfType<BossIntroManager>(); 
        // 이제 Awake에서 호출되니까 제대로 찾아올 거다.
        // 만약 BossIntroManager가 Pause 스크립트보다 나중에 활성화될 수도 있다면 Start()에서 초기화하는 게 더 안전할 수도 있다.
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }

        if (isPaused)
        {
            if (Input.GetKeyDown(KeyCode.Q))
                NavigatePanel(-1); //메뉴 좌측이동
            else if (Input.GetKeyDown(KeyCode.E))
                NavigatePanel(1);  //메뉴 우측이동
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            Time.timeScale = 0f;
            Debug.Log("ESC 게임 중지!"); // 게임이 일시정지됨!
            
            if (PauseMenuUI != null)
                PauseMenuUI.SetActive(true);
            
            // , introManager가 null일 수도 있으니 null 체크 해라!
            if (introManager != null) 
            {
                //introManager.StopIntro(); // 보스 인트로 관련 뭔가를 멈춰야 할 것 같으면 여기에 넣어라
            }
            
            Sound.Instance.PlaySFX(PauseSound, 1f); // Sound.Instance도 null 체크 필요할 수도 있음


            if (currentPanelIndex >= 0 && currentPanelIndex < menuPanels.Count)
            {
                menuPanels[currentPanelIndex].SetActive(true);
                Debug.Log($"현재 패널: {menuPanels[currentPanelIndex].name}");
            }


    }
        else
        {
            Time.timeScale = 1f;
            Debug.Log("ESC 게임 재개!"); // 게임이 재개됨!

            if (PauseMenuUI != null)
                PauseMenuUI.SetActive(false);
            
            // 정지 해제 시에도 introManager 관련 동작이 필요하면 여기에 넣어라
        }
    }

    private void NavigatePanel(int direction)
    {
        if (menuPanels == null || menuPanels.Count == 0)
        {
            Debug.LogWarning("menuPanel이 비어 있거나 null입니다.");
            return;
        }

        // 현재 인덱스가 유효한지 확인한 후에만 비활성화
        if (currentPanelIndex >= 0 && currentPanelIndex < menuPanels.Count)
        {
            menuPanels[currentPanelIndex].SetActive(false);
        }

        // 인덱스 계산 (순환 구조)
        currentPanelIndex += direction;
        if (currentPanelIndex < 0)
            currentPanelIndex = menuPanels.Count - 1;
        else if (currentPanelIndex >= menuPanels.Count)
            currentPanelIndex = 0;

        // 새 패널 활성화
        menuPanels[currentPanelIndex].SetActive(true);
        Debug.Log($"현재 패널: {menuPanels[currentPanelIndex].name}");
    }

}