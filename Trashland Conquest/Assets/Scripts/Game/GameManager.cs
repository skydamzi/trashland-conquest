using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    // ���� ���� ������ ��Ȯ�� �����ϱ� ���� ������(Enum)
    public enum GameEndReason { WinByTime, WinByNpcArrival, LossByTimeOut, LossByNpcDeath, LossByDeath };

    [Header("UI References")]
    public Text timerText; // Ÿ�̸Ӹ� ǥ���� UI Text ������Ʈ
    public GameObject gameOverTextObject;// ���� ���� �� ǥ���� UI GameObject
    public GameObject gameClearTextObject;
    public GameObject winByTimeTextObject; // �ð� �ʰ��� �¸� �� ǥ���� UI GameObject
    public GameObject winByNpcTextObject; // NPC �������� �¸� �� ǥ���� UI GameObject
    public GameObject gameOverlayObject;

    [Header("Game Settings")]
    public bool enableTimer = true; // �ν����Ϳ��� Ÿ�̸� ����� �Ѱ� �� �� �ִ� ����
    public float maxSurvivalTime = 180f; // �����ؾ� �� �ִ� �ð� (�� ����)
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
        // Ÿ�̸� ����� Ȱ��ȭ�Ǿ��� ���� �ʱ�ȭ�մϴ�.
        if (enableTimer)
        {
            currentSurvivalTime = maxSurvivalTime;
            UpdateTimerUI();
        }

        // ���� �� ��� ���� ���� UI�� ��Ȱ��ȭ�մϴ�.
        if (gameOverTextObject != null) gameOverTextObject.SetActive(false);
        if (gameClearTextObject != null) gameClearTextObject.SetActive(false);
        if (winByTimeTextObject != null) winByTimeTextObject.SetActive(false);
        if (winByNpcTextObject != null) winByNpcTextObject.SetActive(false);
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
                // �ð��� 0�� �Ǹ� '�ð� �ʰ� �¸�'�� ������ �����ϴ�.
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
    /// ������ �����ϰ� ����� ó���ϴ� �Լ�.
    /// ������ ���� ������ GameEndReason enum���� �޽��ϴ�.
    /// </summary>
    /// <param name="reason">������ ����� ����</param>
    public void EndGame(GameEndReason reason)
    {
        if (!isGameActive) return;

        isGameActive = false;
        Debug.Log("���� ����! ����: " + reason);

        // ���� ���ο� ���� �ٸ� UI�� Ȱ��ȭ�մϴ�.
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