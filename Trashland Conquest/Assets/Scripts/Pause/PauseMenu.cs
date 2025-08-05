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
    // ✅ 추가 : Setting Panel의 '타이틀로' 버튼에 연결
    public void ShowExitPanel()
    {
        Debug.Log("타이틀로 나가기  버튼을 눌렀습니다.");
        confirmExitPanel.SetActive(true);
        
    }

    // ✅ 추가 : Yes 버튼에 연결
    public void OnConfirmYes()
    {
        Debug.Log("타이틀로 이동합니다.");

        
        StartCoroutine(ExitWithFade());
        if (confirmExitPanel != null)
            confirmExitPanel.SetActive(false);
    }

    // ✅ 추가 : No 버튼에 연결
    public void OnConfirmNo()
    {
        if (confirmExitPanel != null)
            confirmExitPanel.SetActive(false);
    }

    // xkdlxmf타이틀 넘어갈때 함수로 fadeout-추가할것
    // pause창 닫기게도 위에거추가할때같이수정
    IEnumerator ExitWithFade()
    {
        // fadePanel을 만들었다면 여기에 연결 (아직 안 만들었으면 주석처리 가능)
        // fadePanel.SetActive(true);   

        yield return new WaitForSecondsRealtime(0.5f);
        if (Pause.Instance != null)
        {
            Pause.Instance.TogglePause();
        }
        SceneManager.LoadScene("Title");
    }
}