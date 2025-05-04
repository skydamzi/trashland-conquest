using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitlePanelManager : MonoBehaviour
{
    public GameObject titlePanel;

    public void ShowPanel()
    {
        titlePanel.SetActive(true);
    }

    public void HidePanel()
    {
        titlePanel.SetActive(false);
    }
}
