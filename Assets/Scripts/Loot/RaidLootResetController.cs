using UnityEngine;

public class RaidLootResetController : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private Transform lootRoot;
    [SerializeField] private ItemDatabase itemDatabase;
    [SerializeField] private PlayerSquadBridge bridge;

    public void ResetLoots()
    {
        if (lootRoot == null)
        {
            Debug.LogWarning("[RaidLootResetController] lootRoot가 연결되지 않았습니다.");
            return;
        }

        if (itemDatabase == null)
        {
            Debug.LogWarning("[RaidLootResetController] itemDatabase가 연결되지 않았습니다.");
            return;
        }

        if (bridge == null)
        {
            Debug.LogWarning("[RaidLootResetController] bridge가 연결되지 않았습니다.");
            return;
        }

        LootInteractable[] loots = lootRoot.GetComponentsInChildren<LootInteractable>(true);

        for (int i = 0; i < loots.Length; i++)
        {
            loots[i].ResetForRaid(itemDatabase, bridge);
        }

        Debug.Log($"[RaidLootResetController] 루팅 상자 리셋 완료 ({loots.Length})");
    }
}