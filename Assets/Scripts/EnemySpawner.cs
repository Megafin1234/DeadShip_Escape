using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;

    public float spawnInterval = 2f;
    public float spawnHeight = 0.5f;
    private float timer;

    void Update()
    {
        timer += Time.deltaTime; // 시간 누적

        if (timer >= spawnInterval) //특정 시간마다 스폰에너미
        {
            SpawnEnemy();
            timer = 0f;
        }
    }

    void SpawnEnemy()
    {
        Vector3 spawnPosition = transform.position + new Vector3(0, spawnHeight, 0); //스포너 위치 기준으로 y축만큼 스폰height 위
        Instantiate(enemyPrefab, spawnPosition, Quaternion.identity); //스포너 위치에서 적을 생성
    }
}