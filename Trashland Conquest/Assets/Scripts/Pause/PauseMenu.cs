using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [Header("메인 패널 연결")]
    public GameObject equipmentPanel;
    public GameObject inventoryPanel;
    public GameObject skillPanel;
    public GameObject settingsPanel;

    [Header("부가 패널 연결")]
    public GameObject confirmExitPanel;
    public GameObject fadePanel;
    private Dictionary<string, GameObject> panels = new Dictionary<string, GameObject>();

    void Start()
    {
        panels.Add("Equipment", equipmentPanel);
        panels.Add("Inventory", inventoryPanel);
        panels.Add("Skill", skillPanel);
        panels.Add("Settings", settingsPanel);


    }

    public void ShowPanel(string panelName)
    {
        foreach (var kvp in panels)
        {
            kvp.Value.SetActive(kvp.Key == panelName);
        }
        if (Pause.Instance != null)
        {
            switch (panelName)
            {
                case "Equipment": Pause.Instance.currentPanelIndex = 0; break;
                case "Inventory": Pause.Instance.currentPanelIndex = 1; break;
                case "Skill": Pause.Instance.currentPanelIndex = 2; break;
                case "Settings": Pause.Instance.currentPanelIndex = 3; break;
            }

            Debug.Log($"패널 변경: {panelName} → 인덱스 {Pause.Instance.currentPanelIndex}");
        }
    }
    
    public void ShowExitPanel()
    {
        Debug.Log("타이틀로 나가기  버튼을 눌렀습니다.");
        confirmExitPanel.SetActive(true);
        
    }

    public void OnConfirmYes()
    {
        Debug.Log("타이틀로 이동합니다.");

        
        StartCoroutine(ExitWithFade()); //Wait하는 시간설정가능 (float형 매개변수 입력. 기본 0.2f)
        if (confirmExitPanel != null)
            confirmExitPanel.SetActive(false);
    }

    public void OnConfirmNo()
    {
        if (confirmExitPanel != null)
            confirmExitPanel.SetActive(false);
    }

    IEnumerator ExitWithFade(float duration = 0.2f)
    {
        
        fadePanel.SetActive(true);   
        yield return new WaitForSecondsRealtime(duration);
        if (Pause.Instance != null)
        {
            Pause.Instance.TogglePause();
        }
        
        SceneManager.LoadScene("Title");

        fadePanel.SetActive(false);
    }
}