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
            if (bossScript.currentHP < 0)
            {
                fillRT.gameObject.SetActive(false);
            }
            float rate = bossScript.currentHP / bossScript.maxHP;
            SetHPbar(rate);
        }
    }

    public void SetHPbar(float rate)
    {
        if (bossScript.currentHP <= 0)
        {
            bossScript.currentHP = 0;
            fillRT.gameObject.SetActive(false); // 체력 0이면 아예 안 보이게
            return;
        }

        fillRT.localScale = new Vector3(rate, 1f, 1f);
    }
}
