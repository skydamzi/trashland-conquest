using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossUI : MonoBehaviour
{
    public GameObject who;             // 체력 주인
    public RectTransform fillRT;       // 체력바 fill 오브젝트

    private Boss bossScript;

    void Start()
    {
        if (who != null)
            bossScript = who.GetComponent<Boss>(); // Boss 스크립트 연결
    }

    void Update()
    {
        if (bossScript != null)
        {
            float rate = (float)bossScript.hp_current / bossScript.hp_max;
            SetHPbar(rate);
        }
    }

    public void SetHPbar(float rate)
    {
        if (bossScript.hp_current <= 0)
        {
            bossScript.hp_current = 0;
        }
        else 
            fillRT.localScale = new Vector2(rate, 1f);
    }
}
