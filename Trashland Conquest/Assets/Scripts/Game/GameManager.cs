using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("UI References")]
    public Text timerText; // 타이머를 표시할 UI Text 컴포넌트
    public GameObject gameOverTextObject;

    [Header("Game Settings")]
    public float maxSurvivalTime = 180f;
    private float currentSurvivalTime;

    private bool isGameActive = true;

    void Awake()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = -1;

        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        currentSurvivalTime = maxSurvivalTime;
        UpdateTimerUI();
        if (gameOverTextObject != null)
        {
            gameOverTextObject.SetActive(false);
        }
    }

    void Update()
    {
        if (isGameActive)
        {
            currentSurvivalTime -= Time.deltaTime;

            if (currentSurvivalTime <= 0)
            {
                currentSurvivalTime = 0;
                EndGame(true); // 시간이 0이 되면 승리로 게임을 끝냅니다.
            }

            UpdateTimerUI();
        }
    }

    void UpdateTimerUI()
    {
        // 남은 시간을 분과 초로 변환합니다.
        int minutes = Mathf.FloorToInt(currentSurvivalTime / 60);
        int seconds = Mathf.FloorToInt(currentSurvivalTime % 60);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public void EndGame(bool isWin)
    {
        if (!isGameActive) return;

        isGameActive = false;

        if (isWin)
        {
            Debug.Log("게임 승리! 3분 동안 생존했습니다.");
            // 승리 시 호출할 이벤트 또는 함수를 여기에 추가합니다.
            // 예: 승리 화면 UI 활성화, 다음 씬 로드 등
        }
        else
        {
            Debug.Log("게임 오버! 플레이어가 사망했습니다.");
            // 패배 시 호출할 이벤트 또는 함수를 여기에 추가합니다.
            // 예: 게임 오버 화면 UI 활성화
            if (gameOverTextObject != null)
            {
                gameOverTextObject.SetActive(true);
            }
        }
    }
}
