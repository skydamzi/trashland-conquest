using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    // 게임 종료 이유를 정의하는 Enum입니다.
    public enum GameEndReason { WinByNpcArrival, LossByNpcDeath, WinByGoal, LossByDeath, WinByGoStop, WinByTime };

    [Header("UI References")]
    public Text timerText;
    public GameObject gameOverTextObject;
    public GameObject gameClearTextObject;
    public GameObject winByTimeTextObject;
    public GameObject winByNpcTextObject;
    public GameObject winByGoalTextObject;
    public GameObject gameOverlayObject;

    [Header("Go-Stop UI")]
    public GameObject choicePanel;
    public Text goCountText;

    [Header("Game Settings")]
    public bool enableTimer = true;
    public float maxSurvivalTime = 180f;
    private float currentSurvivalTime;

    private bool isGameActive = true;

    // MonsterManager의 '고' 횟수를 받아서 처리
    private int goCount = 0;
    private int maxGoCount = 0; // MonsterManager에서 받아올 값

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
        if (enableTimer)
        {
            currentSurvivalTime = maxSurvivalTime;
            UpdateTimerUI();
        }

        if (gameOverTextObject != null) gameOverTextObject.SetActive(false);
        if (gameClearTextObject != null) gameClearTextObject.SetActive(false);
        if (winByTimeTextObject != null) winByTimeTextObject.SetActive(false);
        if (winByNpcTextObject != null) winByNpcTextObject.SetActive(false);
        if (winByGoalTextObject != null) winByGoalTextObject.SetActive(false);
        if (gameOverlayObject != null) gameOverlayObject.SetActive(false);

        if (choicePanel != null) choicePanel.SetActive(false);
        UpdateGoCountUI();
    }

    void Update()
    {
        if (isGameActive && enableTimer)
        {
            currentSurvivalTime -= Time.deltaTime;
            if (currentSurvivalTime <= 0)
            {
                currentSurvivalTime = 0;
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

    void UpdateGoCountUI()
    {
        if (goCountText != null)
        {
            goCountText.text = $"최대 3번 도전 가능 ( {goCount} / {maxGoCount} )";
        }
    }

    public void WaveClear(int currentGoCount, int maxGo)
    {
        goCount = currentGoCount;
        maxGoCount = maxGo;
        UpdateGoCountUI();

        Debug.Log("웨이브 클리어! 다음 웨이브를 선택하세요.");

        if (goCount < maxGoCount)
        {
            if (choicePanel != null)
            {
                choicePanel.SetActive(true);
            }
        }
        else
        {
            // '고' 횟수가 최대치에 도달하면 최종 승리 처리
            EndGame(GameEndReason.WinByGoStop);
        }
    }

    public void PlayerChoosesGo()
    {
        if (choicePanel != null)
        {
            choicePanel.SetActive(false);
        }
        // MonsterManager에게 '고'를 선택했음을 알림
        if (FindObjectOfType<MonsterManager2>() != null)
        {
            FindObjectOfType<MonsterManager2>().PlayerChoosesGo();
        }
        Debug.Log("고를 선택했습니다! 다음 웨이브가 시작됩니다.");
    }

    public void PlayerChoosesStop()
    {
        if (choicePanel != null)
        {
            choicePanel.SetActive(false);
        }
        Debug.Log("스톱을 선택했습니다! 게임 클리어!");
        EndGame(GameEndReason.WinByGoStop);
    }

    public void EndGame(GameEndReason reason)
    {
        if (!isGameActive) return;

        isGameActive = false;
        Debug.Log("게임 종료! 이유: " + reason);

        switch (reason)
        {
            case GameEndReason.WinByTime:
            case GameEndReason.WinByGoStop:
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