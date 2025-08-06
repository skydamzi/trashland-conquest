using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterManager2 : MonoBehaviour
{
    public GameObject monsterPrefab;
    public int monstersPerWave = 8;
    public float spawnRadius = 10f;
    public Transform playerTransform;

    private List<GameObject> activeMonsters = new List<GameObject>();
    private bool isWaveActive = false;
    private int goCount = 0;
    private const int maxGoCount = 3;

    // 몬스터 생성이 완료되었는지 확인하는 플래그
    private bool isSpawningComplete = false;

    private void Start()
    {
        if (playerTransform == null)
        {
            Debug.LogError("플레이어 트랜스폼이 할당되지 않았습니다.");
            return;
        }

        // 게임 시작 시 첫 번째 웨이브를 시작
        StartNextWave();
    }

    private void Update()
    {
        // isSpawningComplete가 true일 때만 몬스터 수를 확인
        if (isSpawningComplete)
        {
            CleanUpMonsters();

            if (activeMonsters.Count == 0)
            {
                EndWave();
            }
        }
    }

    public void StartNextWave()
    {
        isWaveActive = true;
        isSpawningComplete = false; // 새로운 웨이브 시작 시 false로 재설정

        activeMonsters.Clear();

        int currentMonstersToSpawn = monstersPerWave + (goCount * 5);
        StartCoroutine(SpawnMonstersRoutine(currentMonstersToSpawn));
    }

    private IEnumerator SpawnMonstersRoutine(int count)
    {
        yield return new WaitForSeconds(1f);

        float startAngle = Random.Range(0f, 360f);

        for (int i = 0; i < count; i++)
        {
            float angle = startAngle + (i * (360f / count));
            float angleInRadians = angle * Mathf.Deg2Rad;
            float x = Mathf.Cos(angleInRadians) * spawnRadius;
            float y = Mathf.Sin(angleInRadians) * spawnRadius;
            Vector3 spawnPosition = playerTransform.position + new Vector3(x, y, 0);

            GameObject newMonster = Instantiate(monsterPrefab, spawnPosition, Quaternion.identity);
            activeMonsters.Add(newMonster);
        }
        Debug.Log($"웨이브 {goCount + 1} 시작! 몬스터 {count}마리 소환.");

        isSpawningComplete = true; // 몬스터 생성이 완료되었음을 알림
    }

    // ... (EndWave, PlayerChoosesGo, CleanUpMonsters 함수는 동일)
    void EndWave()
    {
        isWaveActive = false;
        isSpawningComplete = false;

        if (GameManager.instance != null)
        {
            GameManager.instance.WaveClear(goCount, maxGoCount);
        }
    }

    public void PlayerChoosesGo()
    {
        goCount++;
        StartNextWave();
    }

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