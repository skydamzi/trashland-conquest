using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitlePanel : MonoBehaviour
{
    public GameObject titlePanel;
    public AudioClip mouseDown;
    public void ShowPanel()
    {
        Sound.Instance.PlaySFX(mouseDown, 0.5f);
        titlePanel.SetActive(true);
    }

    public void HidePanel()
    {
        Sound.Instance.PlaySFX(mouseDown, 0.5f);
        titlePanel.SetActive(false);
    }
}
