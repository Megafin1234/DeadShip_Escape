using UnityEngine;

public class RaidWorldResetController : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private Transform droppedItemRoot;
    [SerializeField] private RaidLootResetController lootResetController;
    [SerializeField] private Transform enemySpawnRoot;

    public void ResetRaidWorld()
    {
        ClearDroppedItems();

        if (lootResetController != null)
            lootResetController.ResetLoots();

        ResetEnemySpawnPoints();

        Debug.Log("[RaidWorldResetController] 레이드 월드 초기화 완료");
    }

    private void ClearDroppedItems()
    {
        if (droppedItemRoot == null)
            return;

        for (int i = droppedItemRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(droppedItemRoot.GetChild(i).gameObject);
        }

        Debug.Log("[RaidWorldResetController] 월드 드랍 제거 완료");
    }

    private void ResetEnemySpawnPoints()
    {
        if (enemySpawnRoot == null)
            return;

        RaidEnemySpawnPoint[] spawnPoints = enemySpawnRoot.GetComponentsInChildren<RaidEnemySpawnPoint>(true);
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            spawnPoints[i].ResetEnemy();
        }

        Debug.Log($"[RaidWorldResetController] 적 스폰 포인트 리셋 완료 ({spawnPoints.Length})");
    }
}