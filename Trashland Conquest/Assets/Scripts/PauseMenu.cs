using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    [Header("�г� ����")]
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

        //ShowPanel("Equipment"); ���� ���� �� ���â�� �߰��� (��: �������Խ� �޴��� ����)
    }

    public void ShowPanel(string panelName)
    {
        foreach (var kvp in panels)
        {
            kvp.Value.SetActive(kvp.Key == panelName);
        }
    }
}
