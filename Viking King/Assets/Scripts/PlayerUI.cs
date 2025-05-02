using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    public GameObject who;             // 체력 주인
    public RectTransform fillRT;       // 체력바 fill 오브젝트

    private Player playerScript;

    void Start()
    {
        if (who != null)
            playerScript = who.GetComponent<Player>();
    }

    void Update()
    {
        if (playerScript != null)
        {
            float rate = (float)playerScript.hp_current / playerScript.hp_max;
            SetHPbar(rate);
        }
    }

    public void SetHPbar(float rate)
    {
        if (playerScript.hp_current <= 0)
        {
            playerScript.hp_current = 0;
        }
        else
            fillRT.localScale = new Vector2(rate, 1f);
    }
}
