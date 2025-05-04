using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleButton : MonoBehaviour
{
    public GameObject titlePanel;

    public void Show()
    {
        titlePanel.SetActive(true);
    }

    public void Hide()
    {
        titlePanel.SetActive(false);
    }
}
