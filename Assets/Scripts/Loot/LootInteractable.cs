using UnityEngine;
using System.Collections.Generic;

public class LootInteractable : InteractableBase
{
    [Header("Loot")]
    [SerializeField] private string displayName = "Supply Crate";
    [SerializeField] private int slotCount = 8;

    [Header("Raid Spawn")]
    [Range(0f, 1f)]
    [SerializeField] private float spawnChance = 0.65f;

    private LootContainerRuntime runtimeContainer;
    private bool isInitialized = false;

    public LootContainerRuntime RuntimeContainer => runtimeContainer;

    public override bool IsBusy()
    {
        return false;
    }

    protected override void Interact(Transform interactor)
    {
        if (!gameObject.activeSelf)
            return;

        PlayerSquadBridge bridge = PlayerSquadBridge.Instance;
        if (bridge == null)
            return;

        EnsureInitialized();

        if (runtimeContainer == null)
            return;

        if (bridge.RaidOverlayUIController == null)
        {
            Debug.LogWarning("[LootInteractable] RaidOverlayUIController가 연결되지 않았습니다.");
            return;
        }

        bridge.RaidOverlayUIController.OpenLootOverlay(runtimeContainer);
        Debug.Log($"[LootInteractable] 루팅 UI 열기: {runtimeContainer.DisplayName}");
    }

    private void EnsureInitialized()
    {
        if (isInitialized)
            return;

        runtimeContainer = new LootContainerRuntime(displayName, slotCount);
        isInitialized = true;
    }

    /// <summary>
    /// 레이드 시작 시 확률적으로 상자 활성화 + 내부 랜덤 아이템 생성
    /// </summary>
    public void ResetForRaid(ItemDatabase itemDatabase, PlayerSquadBridge bridge)
    {
        if (itemDatabase == null || bridge == null)
        {
            Debug.LogWarning("[LootInteractable] ResetForRaid 실패 - itemDatabase 또는 bridge가 null");
            return;
        }

        float roll = Random.value;
        bool shouldSpawn = roll <= spawnChance;

        gameObject.SetActive(shouldSpawn);

        if (!shouldSpawn)
        {
            Debug.Log($"[LootInteractable] 상자 비활성화 ({roll:F2} > {spawnChance:F2}) / {displayName}");
            return;
        }

        EnsureInitialized();

        if (runtimeContainer == null)
            return;

        ResetContainerContents(itemDatabase, bridge);
    }

    private void ResetContainerContents(ItemDatabase itemDatabase, PlayerSquadBridge bridge)
    {
        runtimeContainer.IsOpened = false;

        ContainerState container = runtimeContainer.Container;
        if (container == null)
            return;

        // 기존 슬롯 비우기
        for (int i = 0; i < container.Slots.Count; i++)
        {
            container.Slots[i].ItemInstanceId = null;
        }

        IReadOnlyList<ItemDefinitionBase> defs = itemDatabase.ItemDefinitions;
        if (defs == null || defs.Count == 0)
        {
            runtimeContainer.RefreshEmptyState();
            return;
        }

        int spawnCount = Random.Range(0, container.Slots.Count + 1);

        for (int i = 0; i < spawnCount; i++)
        {
            ItemDefinitionBase def = GetWeightedRandomDefinition(defs);
            if (def == null)
                continue;

            // 현재는 수량 1 고정
            ItemInstance item = new ItemInstance(def.ItemId, 1);
            bridge.ItemRepository.Add(item);

            container.Slots[i].ItemInstanceId = item.InstanceId;
        }

        runtimeContainer.RefreshEmptyState();
        Debug.Log($"[LootInteractable] 상자 리셋 완료: {displayName}");
    }

    //가중치 부여
    private ItemDefinitionBase GetWeightedRandomDefinition(IReadOnlyList<ItemDefinitionBase> defs)
    {
        if (defs == null || defs.Count == 0)
            return null;

        float totalWeight = 0f;

        for (int i = 0; i < defs.Count; i++)
        {
            totalWeight += GetLootWeight(defs[i]);
        }

        if (totalWeight <= 0f)
            return defs[Random.Range(0, defs.Count)];

        float roll = Random.Range(0f, totalWeight);
        float accumulated = 0f;

        for (int i = 0; i < defs.Count; i++)
        {
            accumulated += GetLootWeight(defs[i]);

            if (roll <= accumulated)
                return defs[i];
        }

        return defs[defs.Count - 1];
    }

    private float GetLootWeight(ItemDefinitionBase def)
    {
        if (def == null)
            return 0f;

        // 1차 예시: rarity 기준
        switch (def.Rarity)
        {
            case ItemRarity.Common: return 10f;
            case ItemRarity.Uncommon: return 6f;
            case ItemRarity.Rare: return 3f;
            case ItemRarity.Epic: return 1.5f;
            case ItemRarity.Legendary: return 0.5f;
            default: return 1f;
        }
    }

    public void InitializeRuntimeContainer(LootContainerRuntime container, string overrideDisplayName = null)
    {
        runtimeContainer = container;
        isInitialized = runtimeContainer != null;

        if (!string.IsNullOrEmpty(overrideDisplayName))
            displayName = overrideDisplayName;
        else if (runtimeContainer != null)
            displayName = runtimeContainer.DisplayName;

        gameObject.SetActive(true);
    }
}