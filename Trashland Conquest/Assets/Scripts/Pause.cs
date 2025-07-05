using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pause : MonoBehaviour
{
    public static Pause Instance;
    public static bool isPaused = false;
    BossIntroManager introManager = FindObjectOfType<BossIntroManager>();
    [Header("����")]
    public AudioClip PauseSound;

    [Header("�Ͻ����� �޴� UI")]
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
            Debug.Log("������ �Ͻ�������!");
            

            if (PauseMenuUI != null)
                PauseMenuUI.SetActive(true);
            Sound.Instance.PlaySFX(PauseSound, 1f);
        }
        else
        {
            Time.timeScale = 1f;
            Debug.Log("������ �簳��!");

            if (PauseMenuUI != null)
                PauseMenuUI.SetActive(false);
        }
    }
}
