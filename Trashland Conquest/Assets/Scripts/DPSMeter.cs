using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DPSMeter : MonoBehaviour
{
    public Text dpsText;
    public Boss boss;

    private float totalDamage = 0f;
    private float startTime = -1f;
    private float lastHitTime = -1f;
    private float lastDPS = 0f;

    private const float idleThreshold = 1f;

    void Update()
    {
        if (boss == null || boss.currentHP <= 0f)
        {
            dpsText.text = "Damage per Second: 0.0";
            return;
        }

        if (Time.time - lastHitTime > idleThreshold && startTime > 0f)
        {
            dpsText.text = "Damage per Second: 0.0";
            ResetMeter();
            return;
        }

        if (startTime > 0f)
        {
            float elapsed = Time.time - startTime;
            float dps = elapsed > 0f ? totalDamage / elapsed : 0f;
            lastDPS = dps;
            dpsText.text = $"Damage per Second: {dps}";
        }
    }

    public void ReportDamage(float damage)
    {
        if (startTime < 0f)
            startTime = Time.time;

        totalDamage += damage;
        lastHitTime = Time.time;
    }

    private void ResetMeter()
    {
        totalDamage = 0f;
        startTime = -1f;
        lastHitTime = -1f;
    }
}
