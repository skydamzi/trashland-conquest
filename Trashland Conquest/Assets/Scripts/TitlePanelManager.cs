using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitlePanelManager : MonoBehaviour
{
    public GameObject titlePanel;
    public AudioClip mouseDown;
    public void ShowPanel()
    {
        SoundManager.Instance.PlaySFX(mouseDown, 0.5f);
        titlePanel.SetActive(true);
    }

    public void HidePanel()
    {
        SoundManager.Instance.PlaySFX(mouseDown, 0.5f);
        titlePanel.SetActive(false);
    }
}
