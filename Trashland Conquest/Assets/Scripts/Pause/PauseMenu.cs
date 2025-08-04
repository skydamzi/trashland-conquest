using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    [Header("패널 연결")]
    public GameObject equipmentPanel;
    public GameObject inventoryPanel;
    public GameObject skillPanel;
    public GameObject settingsPanel;

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
}