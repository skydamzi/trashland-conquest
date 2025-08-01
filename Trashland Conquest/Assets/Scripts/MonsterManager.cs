using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterManager : MonoBehaviour
{
    // 쫄몹 프리팹 (계속 스폰될 몬스터)
    public GameObject normalMonsterPrefab;

    // 엘리트몹 프리팹 (미션 목표)
    public GameObject eliteMonsterPrefab;

    // 몬스터 스폰 주기 (단위: 초)
    public float spawnInterval = 5f;

    // 맵에 존재할 수 있는 최대 몬스터 수
    public int maxMonsters = 200;
    
    // 한 번에 스폰할 쫄몹 수
    public int monstersPerWave = 8;

    // 몬스터가 스폰될 원의 반지름
    public float spawnRadius = 10f;

    // 플레이어 오브젝트를 여기에 끌어다 넣어
    public Transform playerTransform;

    // 현재 맵에 있는 모든 몬스터 리스트
    private List<GameObject> activeMonsters = new List<GameObject>();

    // 엘리트 몬스터 오브젝트
    private GameObject eliteMonster;

    // 쫄몹 스폰을 멈추기 위한 코루틴 변수
    private Coroutine spawnCoroutine;
    
    // 미션 클리어 여부
    private bool isMissionClear = false;

    private void Start()
    {
        if (playerTransform == null)
        {
            Debug.LogError("Player Transform이 할당되지 않았습니다. 인스펙터에서 플레이어 오브젝트를 할당해주세요.");
            return;
        }

        // 쫄몹 스폰 코루틴 시작 및 변수에 저장
        spawnCoroutine = StartCoroutine(SpawnNormalMonsters());
        
        // 미션 시작 시 엘리트몹을 딱 한 마리 스폰
        SpawnEliteMonster();
    }
    
    private void Update()
    {
        // 미션 클리어 상태가 아닐 때만 엘리트몹의 죽음을 체크
        if (!isMissionClear && eliteMonster == null)
        {
            MissionClear();
        }

        // 죽은 몬스터는 리스트에서 제거 (Update에서 매 프레임마다 체크하는 건 비효율적일 수 있지만, 간단하게 구현하기 위해 여기에 둠)
        CleanUpMonsters();
    }

    // 쫄몹을 계속 스폰하는 코루틴
    IEnumerator SpawnNormalMonsters()
    {
        while (true)
        {
            // 설정된 쿨타임 기다리기
            yield return new WaitForSeconds(spawnInterval);

            // 현재 몬스터 수가 최대치를 넘지 않았을 때만 스폰
            if (activeMonsters.Count < maxMonsters)
            {
                SpawnMonstersInWave(normalMonsterPrefab, monstersPerWave);
            }
        }
    }

    // 한 번에 여러 몬스터를 원의 둘레에 같은 간격으로 생성하는 메서드
    void SpawnMonstersInWave(GameObject monsterPrefab, int count)
    {
        float startAngle = Random.Range(0f, 360f);

        for (int i = 0; i < count; i++)
        {
            if (activeMonsters.Count < maxMonsters)
            {
                float angle = startAngle + (i * (360f / count));
                float angleInRadians = angle * Mathf.Deg2Rad;

                float x = Mathf.Cos(angleInRadians) * spawnRadius;
                float y = Mathf.Sin(angleInRadians) * spawnRadius;

                Vector3 spawnPosition = playerTransform.position + new Vector3(x, y, 0);
                
                GameObject newMonster = Instantiate(monsterPrefab, spawnPosition, Quaternion.identity);
                activeMonsters.Add(newMonster);
            }
        }
    }

    // 엘리트 몬스터를 한 마리 스폰하는 메서드
    void SpawnEliteMonster()
    {
        // 쫄몹 스폰과 같은 방식으로 원형 위치 계산
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float x = Mathf.Cos(angle) * spawnRadius;
        float y = Mathf.Sin(angle) * spawnRadius;
        Vector3 spawnPosition = playerTransform.position + new Vector3(x, y, 0);

        // 엘리트 몬스터 생성 및 리스트에 추가
        eliteMonster = Instantiate(eliteMonsterPrefab, spawnPosition, Quaternion.identity);
        activeMonsters.Add(eliteMonster);
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

    // 미션 클리어 처리 메서드
    void MissionClear()
    {
        Debug.Log("미션 클리어! 엘리트 몬스터를 처치했습니다!");
        isMissionClear = true;
        
        // 쫄몹 스폰 코루틴을 중지
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }
    }
}