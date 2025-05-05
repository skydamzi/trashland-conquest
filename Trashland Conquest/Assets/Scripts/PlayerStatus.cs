using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatus : MonoBehaviour
{
    public static PlayerStatus Instance;

    // 기본 정보
    public string unitName = "신병족";
    public int unitLV = 1;

    // 전투 스탯
    public float baseAttackPower = 30f;
    public float bonusAttackPower = 5f;
    public float armor = 0f;

    // 체력/쉴드
    public float currentHP = 100f;
    public float maxHP = 100f;
    public float currentShield = 50f;
    public float maxShield = 50f;

    //경험치
    public int currentEXP = 0;
    public int maxEXP = 100;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
