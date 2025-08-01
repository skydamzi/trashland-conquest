using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    // 게임 종료 이유를 명확히 구분하기 위한 열거형(Enum)
    public enum GameEndReason { WinByTime, WinByNpcArrival, LossByTimeOut, LossByNpcDeath, LossByDeath };

    [Header("UI References")]
    public Text timerText; // 타이머를 표시할 UI Text 컴포넌트
    public GameObject gameOverTextObject; // 게임 오버 시 표시할 UI GameObject
    public GameObject winByTimeTextObject; // 시간 초과로 승리 시 표시할 UI GameObject
    public GameObject winByNpcTextObject; // NPC 도착으로 승리 시 표시할 UI GameObject

    [Header("Game Settings")]
    public bool enableTimer = true; // 인스펙터에서 타이머 기능을 켜고 끌 수 있는 변수
    public float maxSurvivalTime = 180f; // 생존해야 할 최대 시간 (초 단위)
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
        // 타이머 기능이 활성화되었을 때만 초기화합니다.
        if (enableTimer)
        {
            currentSurvivalTime = maxSurvivalTime;
            UpdateTimerUI();
        }

        // 시작 시 모든 게임 종료 UI를 비활성화합니다.
        if (gameOverTextObject != null)
        {
            gameOverTextObject.SetActive(false);
        }
        if (winByTimeTextObject != null)
        {
            winByTimeTextObject.SetActive(false);
        }
        if (winByNpcTextObject != null)
        {
            winByNpcTextObject.SetActive(false);
        }
    }

    void Update()
    {
        if (isGameActive && enableTimer)
        {
            currentSurvivalTime -= Time.deltaTime;

            if (currentSurvivalTime <= 0)
            {
                currentSurvivalTime = 0;
                // 시간이 0이 되면 '시간 초과 승리'로 게임을 끝냅니다.
                EndGame(GameEndReason.WinByTime);
            }

            UpdateTimerUI();
        }
    }

    void UpdateTimerUI()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(currentSurvivalTime / 60);
            int seconds = Mathf.FloorToInt(currentSurvivalTime % 60);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    /// <summary>
    /// 게임을 종료하고 결과를 처리하는 함수.
    /// 게임이 끝난 이유를 GameEndReason enum으로 받습니다.
    /// </summary>
    /// <param name="reason">게임이 종료된 원인</param>
    public void EndGame(GameEndReason reason)
    {
        if (!isGameActive) return;

        isGameActive = false;
        Debug.Log("게임 종료! 원인: " + reason);

        // 종료 원인에 따라 다른 UI를 활성화합니다.
        switch (reason)
        {
            case GameEndReason.WinByTime:
                if (winByTimeTextObject != null)
                {
                    winByTimeTextObject.SetActive(true);
                }
                break;
            case GameEndReason.WinByNpcArrival:
                if (winByNpcTextObject != null)
                {
                    winByNpcTextObject.SetActive(true);
                }
                break;
            case GameEndReason.LossByNpcDeath:
                if (gameOverTextObject != null)
                {
                    gameOverTextObject.SetActive(true);
                }
                break;
        }
        Time.timeScale = 0; // 게임 시간을 멈춰 게임을 정지시킵니다.
    }
}