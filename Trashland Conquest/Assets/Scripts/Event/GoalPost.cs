using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalPost : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 골대 안에 들어온 놈이 '사커볼' 태그를 가지고 있는지 확인하는 거다.
        if (other.CompareTag("SoccerBall"))
        {
            Debug.Log("골인이다 씨발!");
            // 게임 매니저에 골인 승리로 게임을 끝내라고 요청하는 거다.
            GameManager.instance.EndGame(GameManager.GameEndReason.WinByGoal);
        }
        
    }
}
