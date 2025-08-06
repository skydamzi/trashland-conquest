using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    // 게임 종료 이유를 정의하는 Enum입니다.
    public enum GameEndReason { WinByTime, WinByNpcArrival,WinByGoal, LossByTimeOut, LossByNpcDeath, LossByDeath };

    [Header("UI References")]
    public Text timerText; // 타이머 표시 UI Text 오브젝트
    public GameObject gameOverTextObject;// 게임 오버 시 표시할 UI GameObject
    public GameObject gameClearTextObject;
    public GameObject winByTimeTextObject; // 시간 초과 승리 시 표시할 UI GameObject
    public GameObject winByNpcTextObject; // NPC 도착 승리 시 표시할 UI GameObject
    public GameObject winByGoalTextObject; // 골인 승리 시 표시할 UI GameObject
    public GameObject gameOverlayObject;

    [Header("Game Settings")]
    public bool enableTimer = true; // 타이머 기능 활성화 여부
    public float maxSurvivalTime = 180f; // 최대 생존 시간 (초 단위)
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
        // 타이머 관련 초기화를 진행합니다.
        if (enableTimer)
        {
            currentSurvivalTime = maxSurvivalTime;
            UpdateTimerUI();
        }

        // 게임 오버 및 클리어 관련 UI를 비활성화합니다.
        if (gameOverTextObject != null) gameOverTextObject.SetActive(false);
        if (gameClearTextObject != null) gameClearTextObject.SetActive(false);
        if (winByTimeTextObject != null) winByTimeTextObject.SetActive(false);
        if (winByNpcTextObject != null) winByNpcTextObject.SetActive(false);
        if (winByGoalTextObject != null) winByGoalTextObject.SetActive(false);

        if (gameOverlayObject != null) gameOverlayObject.SetActive(false);
    }

    void Update()
    {
        if (isGameActive && enableTimer)
        {
            currentSurvivalTime -= Time.deltaTime;

            if (currentSurvivalTime <= 0)
            {
                currentSurvivalTime = 0;
                // 시간이 0이 되었을 때 '시간 초과 패배'로 게임 종료.
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
    /// 게임을 종료하는 함수입니다.
    /// 게임 종료 이유에 따라 UI를 업데이트하고 게임을 멈춥니다.
    /// </summary>
    /// <param name="reason"></param>
    public void EndGame(GameEndReason reason)
    {
        if (!isGameActive) return;

        isGameActive = false;
        Debug.Log("게임 종료! 이유: " + reason);

        // 게임 종료 이유에 따라 UI를 업데이트합니다.
        switch (reason)
        {
            case GameEndReason.WinByTime:
                if (winByTimeTextObject != null) winByTimeTextObject.SetActive(true);
                if (gameClearTextObject != null) gameClearTextObject.SetActive(true);
                break;
            case GameEndReason.WinByNpcArrival:
                if (winByNpcTextObject != null) winByNpcTextObject.SetActive(true);
                if (gameClearTextObject != null) gameClearTextObject.SetActive(true);
                break;
            case GameEndReason.WinByGoal:
                if (winByGoalTextObject != null) winByGoalTextObject.SetActive(true);
                if (gameClearTextObject != null) gameClearTextObject.SetActive(true);
                break;
            case GameEndReason.LossByNpcDeath:
            case GameEndReason.LossByDeath:
                if (gameOverTextObject != null) gameOverTextObject.SetActive(true);
                break;
        }

        if (gameOverlayObject != null)
        {
            gameOverlayObject.SetActive(true);
        }
        Time.timeScale = 0;
    }
}