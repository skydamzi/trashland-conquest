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

        //ShowPanel("Equipment"); 최초 진입 시 장비창이 뜨게함 (현: 최초진입시 메뉴만 나옴)
    }

    public void ShowPanel(string panelName)
    {
        foreach (var kvp in panels)
        {
            kvp.Value.SetActive(kvp.Key == panelName);
        }
    }
}
