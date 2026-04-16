using UnityEngine;

/// <summary>
/// 레이드 시작 시 일정 확률로 적을 생성하는 고정 스폰 포인트.
/// 
/// 1차 규칙:
/// - 각 포인트마다 60~70% 정도 확률로 생성
/// - 성공 시 enemyPrefab 하나 생성
/// - 기존 적이 남아 있으면 정리 후 다시 판단
/// </summary>
public class RaidEnemySpawnPoint : MonoBehaviour
{
    [Header("Spawn")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Transform spawnAnchor;
    [Range(0f, 1f)]
    [SerializeField] private float spawnChance = 0.65f;

    private GameObject spawnedEnemy;

    public void ResetEnemy()
    {
        // 기존 적 정리
        if (spawnedEnemy != null)
        {
            Destroy(spawnedEnemy);
            spawnedEnemy = null;
        }

        if (enemyPrefab == null)
        {
            Debug.LogWarning("[RaidEnemySpawnPoint] enemyPrefab이 연결되지 않았습니다.");
            return;
        }

        Transform anchor = spawnAnchor != null ? spawnAnchor : transform;

        float roll = Random.value;
        if (roll > spawnChance)
        {
            Debug.Log($"[RaidEnemySpawnPoint] 스폰 실패 ({roll:F2} > {spawnChance:F2}) / {name}");
            return;
        }

        spawnedEnemy = Instantiate(enemyPrefab, anchor.position, anchor.rotation);
        Debug.Log($"[RaidEnemySpawnPoint] 적 생성 성공 / {name}");
    }
}