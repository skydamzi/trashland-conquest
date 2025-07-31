using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterSpawner : MonoBehaviour
{
    // 스폰할 몬스터 프리팹
    public GameObject monsterPrefab;

    // 몬스터 리스폰 주기 (단위: 초)
    public float spawnInterval = 10f;

    // 맵에 존재할 수 있는 최대 몬스터 수
    public int maxMonsters = 200;
    
    // 한 번에 스폰할 몬스터 수
    public int monstersPerWave = 8;

    // 몬스터가 스폰될 원의 반지름
    public float spawnRadius = 10f;

    // 플레이어 오브젝트를 여기에 끌어다 넣어
    public Transform playerTransform;

    // 현재 맵에 있는 몬스터 리스트 (이걸로 몬스터 수를 관리함)
    private List<GameObject> activeMonsters = new List<GameObject>();
    
    private void Start()
    {
        if (playerTransform == null)
        {
            Debug.LogError("Player Transform이 할당되지 않았습니다. 인스펙터에서 플레이어 오브젝트를 할당해주세요.");
            return;
        }

        // 일정 시간마다 몬스터를 스폰하는 코루틴 시작
        StartCoroutine(SpawnMonsters());
    }
    
    // 몬스터를 계속 스폰하는 코루틴
    IEnumerator SpawnMonsters()
    {
        while (true)
        {
            // 쿨타임 기다리기
            yield return new WaitForSeconds(spawnInterval);

            // 현재 몬스터 수가 최대치를 넘지 않았을 때만 스폰
            if (activeMonsters.Count < maxMonsters)
            {
                SpawnMonstersInWave();
            }

            // 죽은 몬스터는 리스트에서 제거
            CleanUpMonsters();
        }
    }

    // 한 번에 여러 몬스터를 원의 둘레에 같은 간격으로 생성하는 메서드
    void SpawnMonstersInWave()
    {
        // 첫 몬스터의 시작 각도를 랜덤으로 정해서 매번 다른 위치에서 스폰되게 함
        float startAngle = Random.Range(0f, 360f);

        for (int i = 0; i < monstersPerWave; i++)
        {
            if (activeMonsters.Count < maxMonsters)
            {
                // 몬스터가 스폰될 각도 계산
                float angle = startAngle + (i * (360f / monstersPerWave));
                
                // 각도를 라디안으로 변환
                float angleInRadians = angle * Mathf.Deg2Rad;

                // X, Y 좌표 계산 (원의 공식)
                float x = Mathf.Cos(angleInRadians) * spawnRadius;
                float y = Mathf.Sin(angleInRadians) * spawnRadius;

                // 플레이어 위치를 기준으로 최종 스폰 위치 결정
                // 2D 게임이므로 z는 0으로 유지
                Vector3 spawnPosition = playerTransform.position + new Vector3(x, y, 0);

                // 계산된 위치에 몬스터 생성
                GameObject newMonster = Instantiate(monsterPrefab, spawnPosition, Quaternion.identity);

                // 생성된 몬스터를 리스트에 추가
                activeMonsters.Add(newMonster);
            }
        }
    }
    
    // 죽은 몬스터를 리스트에서 제거하는 메서드
    void CleanUpMonsters()
    {
        for (int i = activeMonsters.Count - 1; i >= 0; i--)
        {
            if (activeMonsters[i] == null)
            {
                activeMonsters.RemoveAt(i);
            }
        }
    }
}