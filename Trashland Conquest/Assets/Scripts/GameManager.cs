using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("UI References")]
    public Text timerText; // Ÿ�̸Ӹ� ǥ���� UI Text ������Ʈ
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
                EndGame(true); // �ð��� 0�� �Ǹ� �¸��� ������ �����ϴ�.
            }

            UpdateTimerUI();
        }
    }

    void UpdateTimerUI()
    {
        // ���� �ð��� �а� �ʷ� ��ȯ�մϴ�.
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
            Debug.Log("���� �¸�! 3�� ���� �����߽��ϴ�.");
            // �¸� �� ȣ���� �̺�Ʈ �Ǵ� �Լ��� ���⿡ �߰��մϴ�.
            // ��: �¸� ȭ�� UI Ȱ��ȭ, ���� �� �ε� ��
        }
        else
        {
            Debug.Log("���� ����! �÷��̾ ����߽��ϴ�.");
            // �й� �� ȣ���� �̺�Ʈ �Ǵ� �Լ��� ���⿡ �߰��մϴ�.
            // ��: ���� ���� ȭ�� UI Ȱ��ȭ
            if (gameOverTextObject != null)
            {
                gameOverTextObject.SetActive(true);
            }
        }
    }
}
